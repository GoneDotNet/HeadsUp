using Microsoft.Extensions.AI;

namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class AiAnswerProvider(IChatClient chatClient) : IAnswerProvider
{
    const string SystemPrompt = """
        You are a game host for the "heads up" game where one player has a word they can't see on their forehead, and other players have to give them clues until they guess the word.  You are responsible for coming up with answers for a given category.
        Try to keep the answers short, ideally one to four words.
        You should also include "sounds like" and misspellings that are close values for each answer to help with speech recognition matching (ie. Belle for Bell or "Star Trek" for "Star Track").
        """;

    const string UserPrompt = "Give me a list of {0} answers for the category: \"{1}\"";
    
    public async Task<ProvidedAnswer[]> GenerateAnswers(string category, int count, CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync<List<ProvidedAnswer>>([
            new(ChatRole.System, SystemPrompt), 
            new(ChatRole.User, String.Format(UserPrompt, count, category))
        ], cancellationToken: cancellationToken);

        // if (String.IsNullOrWhiteSpace(response.Text))
        //     throw new InvalidOperationException("AI sucks and failed to provide a response.");
        //
        // var array = response.Text.Split(',').Select(x => x.Trim()).ToArray();
        // if (array.Length == 0)
        //     throw new InvalidOperationException("AI sucks and failed to provide any answers.");
        
        return response.Result.ToArray();
    }
}