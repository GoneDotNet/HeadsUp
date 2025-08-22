namespace GoneDotNet.HeadsUp;


public static class Constants
{
    public const int MaxQuestionsPerGame = 25;
#if DEBUG
    public const string OpenAiApiKey = "";
#else
    public const string OpenAiApiKey = "${OPENAI_API_KEY}";
#endif
}