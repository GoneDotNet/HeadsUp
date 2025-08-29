namespace GoneDotNet.HeadsUp;


public static class Constants
{
    public const int MaxAnswersPerGame = 25;
    public const string AzureOpenAiModel = "gpt-4.1";
#if DEBUG
    public const string AzureOpenAiApiKey = "https://allan-mew0m8pi-eastus2.cognitiveservices.azure.com/";
    public const string AzureOpenAiEndpoint = "";
#else
    public const string AzureOpenAiApiKey = "${OPENAI_API_KEY}";
    public const string AzureOpenAiEndpoint = "{OPENAI_ENDPOINT}";
#endif
}