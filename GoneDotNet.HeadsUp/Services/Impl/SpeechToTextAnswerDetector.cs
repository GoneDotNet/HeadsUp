using System.Globalization;
using System.Text;
using Shiny.Speech;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class SpeechToTextAnswerDetector(
    ISpeechToTextService stt,
    IGameService gameService,
    ILogger<SpeechToTextAnswerDetector> logger
) : IAnswerDetector
{
    // spoken controls - live for every question, on top of the current answer itself
    // "next" on its own is deliberately absent - it turns up mid sentence far too often to spend a
    // question on ("the next door neighbour")
    static readonly string[] PassPhrases = ToPhrases(["next question", "pass", "skip"]);
    static readonly string[] SuccessPhrases = ToPhrases(["correct", "close enough", "got it"]);

    // the engine parks on the success/pass screen for 2s before it takes another answer
    static readonly TimeSpan FireCooldown = TimeSpan.FromSeconds(2);

    static readonly SpeechRecognitionOptions Options = new()
    {
        // on-device recognition has no session length cap and no network round trip, which is what a
        // 60 second continuous game (often played in a car) needs - it falls back when unavailable
        PreferOnDevice = true
    };

    public event EventHandler<AnswerType>? AnswerDetected;

    readonly Lock sync = new();
    string[] answerPhrases = [];
    int wordsConsumed;
    int lastWordCount;
    DateTimeOffset resumeAt;
    bool listening;
    bool restarting;


    public async Task Start()
    {
        if (this.listening)
            return;

        if (!stt.IsSupported)
        {
            logger.LogWarning("Speech-to-text is not supported on this device");
            return;
        }

        var access = await stt.RequestAccess();
        if (access != AccessState.Available)
        {
            logger.LogWarning("Speech-to-text access denied: {State}", access);
            return;
        }

        this.OnCurrentAnswerChanged(this, EventArgs.Empty);
        gameService.CurrentAnswerChanged += this.OnCurrentAnswerChanged;
        stt.ResultReceived += this.OnResultReceived;
        stt.Error += this.OnError;

        try
        {
            // one session for the whole game - Shiny holds the mic open and re-arms recognition
            // itself, so stopping and starting per question only tears the audio engine down and
            // loses whatever was said while it came back up
            await stt.Start(Options);
            this.listening = true;
            logger.LogDebug("Speech-to-text answer detection started");
        }
        catch (Exception ex)
        {
            this.Unsubscribe();
            logger.LogWarning(ex, "Failed to start speech-to-text");
        }
    }


    public async Task Stop()
    {
        if (!this.listening)
            return;

        this.listening = false;
        this.Unsubscribe();

        try
        {
            await stt.Stop();
            logger.LogDebug("Speech-to-text answer detection stopped");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to stop speech-to-text");
        }
    }


    void Unsubscribe()
    {
        gameService.CurrentAnswerChanged -= this.OnCurrentAnswerChanged;
        stt.ResultReceived -= this.OnResultReceived;
        stt.Error -= this.OnError;
    }


    void OnCurrentAnswerChanged(object? sender, EventArgs e)
    {
        var answer = gameService.CurrentAnswer;
        var values = new List<string>();

        if (answer != null)
        {
            values.Add(answer.DisplayValue);

            if (answer.AlternateVersions != null)
                values.AddRange(answer.AlternateVersions);
        }

        lock (this.sync)
        {
            this.answerPhrases = ToPhrases(values);
            // anything already in the transcript was said about the previous question
            this.wordsConsumed = this.lastWordCount;
        }
    }


    void OnResultReceived(object? sender, SpeechRecognitionResult result)
    {
        var words = ToWords(result.Text);
        AnswerType? detected = null;

        lock (this.sync)
        {
            // a recognition task builds one transcript up across partials and closes it on the final
            // result, so a shorter transcript than we have consumed means a new one has begun
            if (words.Length < this.wordsConsumed)
                this.wordsConsumed = 0;

            if (DateTimeOffset.UtcNow < this.resumeAt)
            {
                // the engine is showing the success/pass screen and will drop anything we submit -
                // swallow what is said over it rather than letting it score the next answer
                this.wordsConsumed = words.Length;
            }
            else
            {
                detected = Detect(ToPhrase(words.AsSpan(this.wordsConsumed)), this.answerPhrases);

                if (detected != null)
                {
                    this.wordsConsumed = words.Length;
                    this.resumeAt = DateTimeOffset.UtcNow.Add(FireCooldown);
                }
            }

            this.lastWordCount = words.Length;

            if (result.IsFinal)
            {
                // the next result opens a fresh transcript
                this.wordsConsumed = 0;
                this.lastWordCount = 0;
            }
        }

        if (detected == null)
            return;

        logger.LogDebug("Detected {Answer} in '{Transcript}'", detected, result.Text);
        this.AnswerDetected?.Invoke(this, detected.Value);
    }


    static AnswerType? Detect(string phrase, string[] answerPhrases)
    {
        // the answer wins over the controls - a category can easily hand out "Pass" as an answer
        if (ContainsAny(phrase, answerPhrases) || ContainsAny(phrase, SuccessPhrases))
            return AnswerType.Success;

        if (ContainsAny(phrase, PassPhrases))
            return AnswerType.Pass;

        return null;
    }


    static bool ContainsAny(string phrase, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (phrase.Contains(candidate, StringComparison.Ordinal))
                return true;
        }
        return false;
    }


    /// <summary>
    /// Splits into lower case, accent free, punctuation free words - the recognizer will not
    /// reproduce the punctuation or the accents an answer like "Beyonce Knowles" was written with
    /// </summary>
    static string[] ToWords(string? value)
    {
        if (String.IsNullOrWhiteSpace(value))
            return [];

        var builder = new StringBuilder(value.Length);

        foreach (var ch in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(Char.IsLetterOrDigit(ch) ? Char.ToLowerInvariant(ch) : ' ');
        }

        return builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }


    /// <summary>
    /// Space delimits both ends so that a plain substring check only matches whole words - otherwise
    /// "compass" reads as a "pass" and every answer containing "next" skips the question
    /// </summary>
    static string ToPhrase(ReadOnlySpan<string> words) => $" {String.Join(' ', words)} ";


    static string[] ToPhrases(IEnumerable<string> values) => values
        .Select(x => ToPhrase(ToWords(x)))
        .Where(x => x.Length > 2)
        .Distinct()
        .ToArray();


    void OnError(object? sender, SpeechRecognitionError e)
    {
        logger.LogWarning(e.Exception, "Speech recognition error: {Message}", e.Message);

        // the platform recognition task does not re-arm after a hard error, and nothing else notices
        // that the session is dead - the rest of the game would silently detect nothing
        _ = this.Restart();
    }


    async Task Restart()
    {
        lock (this.sync)
        {
            if (this.restarting || !this.listening)
                return;

            this.restarting = true;
        }

        try
        {
            await stt.Stop();
            await Task.Delay(500);

            if (this.listening)
            {
                await stt.Start(Options);
                logger.LogDebug("Speech-to-text session restarted after an error");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to restart speech-to-text");
        }
        finally
        {
            lock (this.sync)
                this.restarting = false;
        }
    }
}
