namespace GoneDotNet.HeadsUp.Services;

public interface IBeepService
{
    /// <summary>
    /// Does a vibration and a sound
    /// </summary>
    void Countdown();
    void Success();
    void Pass();
    
    void SetThemeVolume(float volume);
    void PlayThemeSong();
    void StopThemeSong();
}