namespace GoneDotNet.HeadsUp.Services;

public interface IVideoRecorder
{
    Task StartRecording(string outputPath, bool useFrontCamera = true, bool captureAudio = false);
    string? StopRecording();
    bool IsRecording { get; }
    bool IsSupported { get; }
    event EventHandler<bool>? StatusChanged;
    event EventHandler<Exception>? ErrorOccurred;
}
