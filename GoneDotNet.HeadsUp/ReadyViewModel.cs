using Microsoft.Extensions.AI;

namespace GoneDotNet.HeadsUp;

[ShellMap<ReadyPage>]
public partial class ReadyViewModel(INavigator navigator, IChatClient chatClient) : ObservableObject
{
    [ObservableProperty] string category;
}