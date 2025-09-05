using System.Globalization;
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
    public async Task Start()
    {
        var result = await Stt.RequestPermissions();
        if (!result) // ||  Stt.CurrentState != SpeechToTextState.Stopped)
            return;

       
        Stt.RecognitionResultCompleted += SttOnRecognitionResultCompleted;

        await Stt.StartListenAsync(new SpeechToTextOptions
        {
            Culture = new CultureInfo("en-US"),
            ShouldReportPartialResults = false
        });
    }



    public async Task Stop()
    {
        Stt.RecognitionResultCompleted -= SttOnRecognitionResultCompleted;
        await Stt.StopListenAsync();
    }
    
    
    void SttOnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        var text = args.RecognitionResult.Text?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text))
            return;

        switch (text)
        {
            case "pass":
                this.AnswerDetected?.Invoke(AnswerType.Pass);
                break;
            
            case "close enough":
            case "correct":
                this.AnswerDetected?.Invoke(AnswerType.Success);
                break;
            
            default:
                // no one understands you bro!
                break;
        }
    }
}