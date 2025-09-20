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
    
    
    bool isPlaying = false;
    public void PlayThemeSong()
    {
        if (isPlaying)
            return;

        this.logger.LogDebug("Playing theme song");
        isPlaying = true;
        themeSong.Loop = true;
        themeSong.Play();
    }
    
    
    public void Countdown()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
        countdown.Play();
        this.logger.LogDebug("Countdown Beep");
    }

    
    public void Success()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
        success.Play();
        this.logger.LogDebug("Success Beep");
    }
    

    public void Pass()
    {
        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
        pass.Play();
        this.logger.LogDebug("Pass Beep");
    }
}