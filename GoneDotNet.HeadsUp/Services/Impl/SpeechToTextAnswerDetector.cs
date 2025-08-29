using CommunityToolkit.Maui.Media;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector : IAnswerDetector
{
    // TODO: listen for "close enough", "correct", or "the actual answer"
    // we need to pass in the actual answer to listen to....
        // could inject/steal game context?  maybe
    ISpeechToText Stt => SpeechToText.Default;
        
    public event Action<AnswerType>? AnswerDetected;
}