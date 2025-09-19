using System.Globalization;
using CommunityToolkit.Maui.Media;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector(
    IGameContext gameContext,
    ILogger<SpeechToTextAnswerDetector> logger
) : IAnswerDetector
{
    // TODO: listen for "close enough", "correct", or "the actual answer"
    // we need to pass in the actual answer to listen to....
        // could inject/steal game context?  maybe
    ISpeechToText Stt => SpeechToText.Default;
        
    public event EventHandler<AnswerType>? AnswerDetected;
    
    
    public async Task Start()
    {
        var result = await Stt.RequestPermissions();
        if (!result) // ||  Stt.CurrentState != SpeechToTextState.Stopped)
            return;

        //Stt.RecognitionResultCompleted += SttOnRecognitionResultCompleted;
        Stt.RecognitionResultUpdated += SttOnRecognitionResultUpdated;
        await Stt.StartListenAsync(new SpeechToTextOptions
        {
            Culture = new CultureInfo("en-US"),
            ShouldReportPartialResults = true
        });
    }

    private void SttOnRecognitionResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        ProcessText(args.RecognitionResult);
        
    }


    public async Task Stop()
    {
        Stt.RecognitionResultCompleted -= SttOnRecognitionResultCompleted;
        await Stt.StopListenAsync();
    }


    void ProcessText(string? text)
    {
        if (String.IsNullOrWhiteSpace(text))
            return;
        
        text = text.Trim().ToLower();
        logger.LogDebug("Incoming Text: " + text);
        if (String.IsNullOrWhiteSpace(text))
            return;

        switch (text)
        {
            case "pass":
                this.AnswerDetected?.Invoke(this, AnswerType.Pass);
                break;
            
            case "close enough":
            case "correct":
                this.AnswerDetected?.Invoke(this, AnswerType.Success);
                break;
            
            default:
                var result = gameContext.CurrentAnswer?.Contains(text, StringComparison.InvariantCultureIgnoreCase) ?? false;
                if (result)
                    this.AnswerDetected?.Invoke(this, AnswerType.Success);                        
                break;
        }
    }
    
    void SttOnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        ProcessText(args.RecognitionResult.Text);
    }
}