using System.Globalization;
using CommunityToolkit.Maui.Media;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector(
    IGameService gameService,
    ILogger<SpeechToTextAnswerDetector> logger
) : IAnswerDetector
{
    ISpeechToText Stt => SpeechToText.Default;
    public event EventHandler<AnswerType>? AnswerDetected;
    
    Timer? bufferTimer;
    readonly List<string> wordBuffer = new();
    readonly object syncLock = new object();

    
    public async Task Start()
    {
        var result = await Stt.RequestPermissions();
        if (!result || Stt.CurrentState != SpeechToTextState.Stopped)
            return;

        try
        {
            Stt.RecognitionResultUpdated += SttOnRecognitionResultUpdated;
            await Stt.StartListenAsync(new SpeechToTextOptions
            {
                Culture = new CultureInfo("en-US"),
                ShouldReportPartialResults = true
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start Speech-to-text");
        }
    }


    public async Task Stop()
    {
        Stt.RecognitionResultUpdated -= SttOnRecognitionResultUpdated;
        
        // Clean up the buffer timer
        lock (syncLock)
        {
            bufferTimer?.Dispose();
            bufferTimer = null;
            wordBuffer.Clear();
        }
        
        await Stt.StopListenAsync();
    }


    void SttOnRecognitionResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        var text = args.RecognitionResult?.Trim();
        if (String.IsNullOrWhiteSpace(text))
            return;
        
        logger.LogDebug("Incoming Text: " + text);
        lock (syncLock)
        {
            // Add the word/phrase to our buffer
            wordBuffer.Add(text);
            
            // Reset the buffer timer - we'll process after 800ms of no new words
            bufferTimer?.Dispose();
            bufferTimer = new Timer(ProcessBufferedUtterance, null, 800, Timeout.Infinite);
        }
    }

    private void ProcessBufferedUtterance(object? state)
    {
        string utterance;
        
        lock (syncLock)
        {
            // Combine all buffered words into a single utterance
            utterance = string.Join(" ", wordBuffer);
            
            // Clear the buffer and dispose timer
            wordBuffer.Clear();
            bufferTimer?.Dispose();
            bufferTimer = null;
        }
        
        if (!String.IsNullOrWhiteSpace(utterance))
        {
            logger.LogDebug("Processing buffered utterance: " + utterance);
            var answer = DetectAnswer(utterance);
            
            if (answer != null)
                this.AnswerDetected?.Invoke(this, answer.Value);
        }
    }


    AnswerType? DetectAnswer(string text)
    {
        logger.LogDebug("Detect Answer: " + text);
        switch (text)
        {
            case "next question":
            case "pass":
                return AnswerType.Pass;
            
            case "close enough":
            case "correct":
                return AnswerType.Success;
            
            default:
                
                var result = gameService.CurrentAnswer?.DisplayValue.Contains(text, StringComparison.InvariantCultureIgnoreCase) ?? false;
                if (result)
                    return AnswerType.Success;

                result = gameService
                    .CurrentAnswer?
                    .AlternateVersions?
                    .Any(x => x.Contains(text, StringComparison.InvariantCultureIgnoreCase)) ?? false;
                if (result)
                    return AnswerType.Success;
                
                return null;
        }
    }
}