namespace GoneDotNet.HeadsUp;


[ShellMap<TestPage>]
public partial class TestViewModel(IEnumerable<IAnswerDetector> detectors) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<EventItem> events = new();
    
    
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
    
    
    void DetectorOnAnswerDetected(object? sender, AnswerType e)
    {
        
    }
}

public record EventItem(string Text, DateTimeOffset Timestamp);