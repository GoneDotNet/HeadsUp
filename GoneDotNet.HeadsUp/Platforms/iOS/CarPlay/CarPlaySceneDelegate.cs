using CarPlay;
using Foundation;

namespace GoneDotNet.HeadsUp;

[Register("CarPlaySceneDelegate")]
public class CarPlaySceneDelegate : CPTemplateApplicationSceneDelegate
{
    CPInterfaceController? interfaceController;
    IServiceProvider Services => IPlatformApplication.Current!.Services;

    IBeepService Beeper => Services.GetRequiredService<IBeepService>();
    ICategoryRespository CategoryRepo => Services.GetRequiredService<ICategoryRespository>();
    IGameService GameService => Services.GetRequiredService<IGameService>();
    IAnswerProvider AnswerProvider => Services.GetRequiredService<IAnswerProvider>();

    public override void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
    {
        this.interfaceController = interfaceController;
        ShowMainScreen();
    }

    public override void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController interfaceController)
    {
        this.interfaceController = null;
    }

    void ShowMainScreen()
    {
        if (interfaceController == null) return;
        var screen = new CarPlayMainScreen(interfaceController, Beeper, CategoryRepo, this);
        interfaceController.SetRootTemplate(screen.Template, true, null);
    }

    public void NavigateToGame(string category)
    {
        if (interfaceController == null) return;
        var screen = new CarPlayReadyScreen(interfaceController, Beeper, GameService, AnswerProvider, category, this);
        interfaceController.PushTemplate(screen.Template, true, null);
    }

    public void NavigateToGamePlay()
    {
        if (interfaceController == null) return;
        var screen = new CarPlayGameScreen(interfaceController, Beeper, GameService, this);
        interfaceController.PushTemplate(screen.Template, true, null);
    }

    public void NavigateToScore(Guid gameId, bool isFromGame)
    {
        if (interfaceController == null) return;
        var screen = new CarPlayScoreScreen(interfaceController, GameService, gameId, isFromGame, this);

        if (isFromGame)
        {
            // replace entire stack with main + score
            interfaceController.SetRootTemplate(screen.Template, true, null);
        }
        else
        {
            interfaceController.PushTemplate(screen.Template, true, null);
        }
    }

    public void NavigateToScoreList()
    {
        if (interfaceController == null) return;
        var screen = new CarPlayScoreListScreen(interfaceController, GameService, this);
        interfaceController.PushTemplate(screen.Template, true, null);
    }

    public void NavigateToCategoryList()
    {
        if (interfaceController == null) return;
        var screen = new CarPlayCategoryListScreen(interfaceController, CategoryRepo, this);
        interfaceController.PushTemplate(screen.Template, true, null);
    }

    public void PopToRoot()
    {
        if (interfaceController == null) return;
        ShowMainScreen();
    }

    public void GoBack()
    {
        interfaceController?.PopTemplate(true, null);
    }

    public void ShowAlert(string title, string message, Action? onDismiss = null)
    {
        if (interfaceController == null) return;

        var action = new CPAlertAction("OK", CPAlertActionStyle.Default, _ =>
            interfaceController?.DismissTemplate(true, (_, _) => onDismiss?.Invoke()));
        var alert = new CPAlertTemplate(new[] { title, message }, new[] { action });
        interfaceController.PresentTemplate(alert, true, null);
    }

    public void ShowConfirm(string title, string message, Action<bool> callback)
    {
        if (interfaceController == null) return;

        var yesAction = new CPAlertAction("Yes", CPAlertActionStyle.Default, _ =>
            interfaceController?.DismissTemplate(true, (_, _) => callback(true)));
        var noAction = new CPAlertAction("No", CPAlertActionStyle.Cancel, _ =>
            interfaceController?.DismissTemplate(true, (_, _) => callback(false)));
        var alert = new CPAlertTemplate(new[] { title, message }, new[] { yesAction, noAction });
        interfaceController.PresentTemplate(alert, true, null);
    }
}
