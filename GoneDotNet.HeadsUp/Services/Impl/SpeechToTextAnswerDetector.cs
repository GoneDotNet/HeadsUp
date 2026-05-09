using Shiny.Speech;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector(
    ISpeechToTextService stt,
    IGameService gameService,
    ILogger<SpeechToTextAnswerDetector> logger
) : IAnswerDetector
{
    public event EventHandler<AnswerType>? AnswerDetected;

    CancellationTokenSource? cts;


    public async Task Start()
    {
        var access = await stt.RequestAccess();
        if (access != AccessState.Available)
        {
            logger.LogWarning("Speech-to-text access denied: {State}", access);
            return;
        }

        try
        {
            cts = new CancellationTokenSource();
            _ = ListenLoop(cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start Speech-to-text");
        }
    }

    async Task ListenLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var keywords = BuildKeywords();
                var matched = await stt.ListenForKeyword(keywords, cancellationToken: ct);

                if (matched == null)
                    continue;

                logger.LogDebug("Matched keyword: {Keyword}", matched);
                var answer = matched is "next question" or "pass"
                    ? AnswerType.Pass
                    : AnswerType.Success;

                this.AnswerDetected?.Invoke(this, answer);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Speech-to-text listen loop error");
        }
    }

    string[] BuildKeywords()
    {
        var keywords = new List<string> { "next question", "pass", "close enough", "correct" };

        var currentAnswer = gameService.CurrentAnswer;
        if (currentAnswer != null)
        {
            keywords.Add(currentAnswer.DisplayValue);

            if (currentAnswer.AlternateVersions != null)
                keywords.AddRange(currentAnswer.AlternateVersions);
        }

        return keywords.ToArray();
    }


    public Task Stop()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        return Task.CompletedTask;
    }
}
