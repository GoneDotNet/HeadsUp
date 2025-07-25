namespace GoneDotNet.HeadsUp;

public partial class App : Application
{
    public App(MainPage mainPage)
    {
        this.InitializeComponent();
        this.MainPage = mainPage;
    }
}