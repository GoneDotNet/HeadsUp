using CommunityToolkit.Maui.Media;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector : IAnswerDetector
{
    ISpeechToText Stt => SpeechToText.Default;
        
    public event Action<AnswerType>? AnswerDetected;
}