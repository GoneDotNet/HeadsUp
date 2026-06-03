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

    CancellationTokenSource? loopCts;
    CancellationTokenSource? listenCts;


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
            loopCts = new CancellationTokenSource();
            gameService.CurrentAnswerChanged += OnCurrentAnswerChanged;
            _ = ListenLoop(loopCts.Token);
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
                listenCts?.Dispose();
                listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                var keywords = BuildKeywords();
                string? matched;
                try
                {
                    matched = await stt.WaitListenForKeywords(keywords, cancellationToken: listenCts.Token);
                }
                catch (OperationCanceledException)
                {
                    // either the loop was cancelled or the current answer changed — re-loop
                    continue;
                }

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

    void OnCurrentAnswerChanged(object? sender, EventArgs e)
    {
        // cancel just the current listen call so the loop reiterates with the new keyword set
        try { listenCts?.Cancel(); }
        catch (ObjectDisposedException) { }
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
        gameService.CurrentAnswerChanged -= OnCurrentAnswerChanged;
        try { listenCts?.Cancel(); }
        catch (ObjectDisposedException) { }
        listenCts?.Dispose();
        listenCts = null;

        loopCts?.Cancel();
        loopCts?.Dispose();
        loopCts = null;
        return Task.CompletedTask;
    }
}
