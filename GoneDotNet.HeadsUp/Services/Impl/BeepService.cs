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
    
    
    public BeepService()
    {
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

        isPlaying = true;
        themeSong.Loop = true;
        themeSong.Play();
    }
    
    
    public void PlayInGameSong()
    {
        // themeSong.Stop();
        // themeSong.Loop = false;
        // themeSong.Play();
    }
    
    public void Countdown()
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        countdown?.Play();
    }

    
    public void Success()
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        success?.Play();
    }
    

    public void Pass()
    {
        Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
        pass?.Play();
    }
}