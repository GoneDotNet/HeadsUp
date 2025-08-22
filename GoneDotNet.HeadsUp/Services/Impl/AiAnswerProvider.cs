using Microsoft.Extensions.AI;

namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class AiAnswerProvider(IChatClient chatClient) : IAnswerProvider
{
    const string SystemPrompt = """
        You are a game host for the "heads up" game where one player has a word they can't see on their forehead, and other players have to give them clues until they guess the word.  You are responsible for coming up with answers for a given category.
        The list must be a single comma separated string of answers.
        """;

    const string UserPrompt = "Give me a list of {0} answers for the category: \"{1}\"";
    
    public async Task<string[]> GenerateAnswers(string category, int count, CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync([
            new(ChatRole.System, SystemPrompt), 
            new(ChatRole.User, String.Format(UserPrompt, count, category))
        ], cancellationToken: cancellationToken);

        if (String.IsNullOrWhiteSpace(response.Text))
            throw new InvalidOperationException("AI sucks and failed to provide a response.");

        var array = response.Text.Split(',').Select(x => x.Trim()).ToArray();
        if (array.Length == 0)
            throw new InvalidOperationException("AI sucks and failed to provide any answers.");
        
        return array;
    }
}