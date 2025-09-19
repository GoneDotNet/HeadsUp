namespace GoneDotNet.HeadsUp;


[ShellMap<TestPage>]
public partial class TestViewModel(
    IDispatcher dispatcher,
    IEnumerable<IAnswerDetector> detectors
) : ObservableObject, IPageLifecycleAware
{
    public List<EventItem> Events { get; } = new();
    
    
    public async void OnAppearing()
    {
        foreach (var detector in detectors)
        {
            detector.AnswerDetected += DetectorOnAnswerDetected;
            await detector.Start();
        }
    }
    

    public async void OnDisappearing()
    {
        foreach (var detector in detectors)
        {
            detector.AnswerDetected -= DetectorOnAnswerDetected;
            await detector.Stop();
        }
    }
    
    
    async void DetectorOnAnswerDetected(object? sender, AnswerType e)
    {
        await dispatcher.DispatchAsync(() =>
        {
            this.Events.Add(new EventItem($"{sender!.GetType().Name} - {e}", DateTimeOffset.Now));
            this.OnPropertyChanged(nameof(this.Events));
        });
    }
}

public record EventItem(string Text, DateTimeOffset Timestamp);