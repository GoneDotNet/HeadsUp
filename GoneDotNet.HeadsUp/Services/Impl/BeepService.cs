using Shiny.Audio;
using System.Reflection;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class BeepService : IBeepService, IAsyncDisposable
{
    readonly byte[] successData;
    readonly byte[] passData;
    readonly byte[] countdownData;
    readonly byte[] themeSongData;

    // Shiny.Audio's IAudioPlayer stops whatever it is currently playing when handed a new stream, so
    // each sound gets its own player - a beep must not cut the looping theme song off
    readonly IAudioPlayer success;
    readonly IAudioPlayer pass;
    readonly IAudioPlayer countdown;
    readonly IAudioPlayer themeSong;
    readonly ILogger logger;

    readonly Lock themeLock = new();
    CancellationTokenSource? themeCts;
    Task themeTask = Task.CompletedTask;

    public BeepService(ILoggerFactory loggerFactory, ILogger<BeepService> logger)
    {
        this.logger = logger;

        this.themeSongData = LoadAsset("theme.mp3");
        this.successData = LoadAsset("success.mp3");
        this.passData = LoadAsset("pass.mp3");
        this.countdownData = LoadAsset("countdown.mp3");

        this.themeSong = CreatePlayer(loggerFactory);
        this.success = CreatePlayer(loggerFactory);
        this.pass = CreatePlayer(loggerFactory);
        this.countdown = CreatePlayer(loggerFactory);
    }


    static byte[] LoadAsset(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"GoneDotNet.HeadsUp.Assets.{fileName}")!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }


    static IAudioPlayer CreatePlayer(ILoggerFactory loggerFactory)
#if ANDROID
        => new AndroidAudioPlayer(loggerFactory.CreateLogger<AndroidAudioPlayer>());
#elif APPLE
        => new AppleAudioPlayer(loggerFactory.CreateLogger<AppleAudioPlayer>());
#else
        => throw new PlatformNotSupportedException("No audio player is available for this platform");
#endif


    public void SetThemeVolume(float volume)
        // Shiny.Audio has no per-player attenuation - IAudioPlayer.Volume is the device output volume
        // (Android) and throws on Apple, so ducking the theme song is not available here
        => this.logger.LogDebug("SetThemeVolume({Volume}) ignored - per-player volume is not supported", volume);


    public void PlayThemeSong()
    {
        if (!Preferences.Default.Get("ThemeSongEnabled", true))
            return;

        CancellationTokenSource cts;
        lock (this.themeLock)
        {
            if (this.themeCts != null)
                return;

            cts = this.themeCts = new CancellationTokenSource();
        }

        this.logger.LogDebug("Playing theme song");

        // IAudioPlayer.PlayAsync completes when the track ends - replay it to loop
        this.themeTask = Task.Run(async () =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    using var stream = new MemoryStream(this.themeSongData, false);
                    await this.themeSong.PlayAsync(stream, cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Theme song playback failed");
            }
            finally
            {
                lock (this.themeLock)
                {
                    if (this.themeCts == cts)
                        this.themeCts = null;
                }
                cts.Dispose();
            }
        });
    }


    public void StopThemeSong()
    {
        CancellationTokenSource? cts;
        lock (this.themeLock)
        {
            cts = this.themeCts;
            this.themeCts = null;
        }

        if (cts == null)
            return;

        this.logger.LogDebug("Stopping theme song");
        try { cts.Cancel(); }
        catch (ObjectDisposedException) { } // the loop already ended and disposed it
    }


    public void Countdown()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        this.Play(this.countdown, this.countdownData);
        this.logger.LogDebug("Countdown Beep");
    }


    public void Success()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        this.Play(this.success, this.successData);
        this.logger.LogDebug("Success Beep");
    }


    public void Pass()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        this.Play(this.pass, this.passData);
        this.logger.LogDebug("Pass Beep");
    }


    void Play(IAudioPlayer player, byte[] data) => _ = Task.Run(async () =>
    {
        try
        {
            using var stream = new MemoryStream(data, false);
            await player.PlayAsync(stream);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Beep playback failed");
        }
    });


    public async ValueTask DisposeAsync()
    {
        this.StopThemeSong();
        await this.themeTask;

        foreach (var player in new[] { this.themeSong, this.success, this.pass, this.countdown })
        {
            if (player is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
