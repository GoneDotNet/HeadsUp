using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

/// <summary>
/// Resolves shared services from the MAUI DI container for Android Auto screens
/// </summary>
public static class CarServiceResolver
{
    static IServiceProvider Services => IPlatformApplication.Current!.Services;

    public static IBeepService Beeper => Services.GetRequiredService<IBeepService>();
    public static ICategoryRespository CategoryRepo => Services.GetRequiredService<ICategoryRespository>();
    public static IGameService GameService => Services.GetRequiredService<IGameService>();
    public static IAnswerProvider AnswerProvider => Services.GetRequiredService<IAnswerProvider>();
}
