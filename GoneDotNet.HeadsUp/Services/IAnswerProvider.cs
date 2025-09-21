using System.ComponentModel;

namespace GoneDotNet.HeadsUp.Services;

public interface IAnswerProvider
{
    Task<ProvidedAnswer[]> GenerateAnswers(string category, int count, CancellationToken cancellationToken);
}

public class ProvidedAnswer
{
    [Description("The value we display to the user")]
    public string DisplayValue { get; set; }
    
    [Description("The alternate spelling and values we use for matching against speech input")]
    public string[]? AlternateVersions { get; set; }
}