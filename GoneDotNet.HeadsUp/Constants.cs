namespace GoneDotNet.HeadsUp;


public static class Constants
{
    public const int MaxAnswersPerGame = 25;
    public const string AzureOpenAiModel = "gpt-4.1";
#if DEBUG
    public const string AzureOpenAiApiKey = "FRiLwR72ZuvOWu58pjk1E77ifEb5JpKl0oAiw5u1tsadvEzJkaR9JQQJ99BJACYeBjFXJ3w3AAABACOGr6Bu";
    public const string AzureOpenAiEndpoint = "https://shinyopenai.openai.azure.com/";
#else
    public const string AzureOpenAiApiKey = "${OPENAI_API_KEY}";
    public const string AzureOpenAiEndpoint = "{OPENAI_ENDPOINT}";
#endif
}