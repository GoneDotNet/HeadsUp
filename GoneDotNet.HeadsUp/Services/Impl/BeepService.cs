using Plugin.Maui.Audio;
using System.Reflection;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class BeepService : IBeepService
{
    readonly IAudioPlayer success;
    readonly IAudioPlayer pass;
    readonly IAudioPlayer countdown;
    readonly IAudioPlayer themeSong;
    readonly ILogger logger;
    
    public BeepService(ILogger<BeepService> logger)
    {
        this.logger = logger;
        var assembly = Assembly.GetExecutingAssembly();
        
        this.themeSong = AudioManager.Current.CreatePlayer(assembly.GetManifestResourceStream("GoneDotNet.HeadsUp.Assets.theme.mp3")!);
        this.success = AudioManager.Current.CreatePlayer(assembly.GetManifestResourceStream("GoneDotNet.HeadsUp.Assets.success.mp3")!);
        this.pass = AudioManager.Current.CreatePlayer(assembly.GetManifestResourceStream("GoneDotNet.HeadsUp.Assets.pass.mp3")!);
        this.countdown = AudioManager.Current.CreatePlayer(assembly.GetManifestResourceStream("GoneDotNet.HeadsUp.Assets.countdown.mp3")!);
    }
    
    
    public void SetThemeVolume(float volume) => themeSong.Volume = volume;
    public void PlayThemeSong()
    {
        if (themeSong.IsPlaying)
            return;

        this.logger.LogDebug("Playing theme song");
        themeSong.Loop = true;
        themeSong.Play();
    }
    
    
    public void StopThemeSong()
    {
        if (!themeSong.IsPlaying)
            return;

        this.logger.LogDebug("Stopping theme song");
        themeSong.Stop();
    }
    
    
    public void Countdown()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        countdown.Play();
        this.logger.LogDebug("Countdown Beep");
    }

    
    public void Success()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        success.Play();
        this.logger.LogDebug("Success Beep");
    }
    

    public void Pass()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        pass.Play();
        this.logger.LogDebug("Pass Beep");
    }
}