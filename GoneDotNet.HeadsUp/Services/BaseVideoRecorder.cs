namespace GoneDotNet.HeadsUp.Services;

public abstract class BaseVideoRecorder(ILogger logger) : IVideoRecorder
{
    public string? CurrentOutputPath { get; private set; }

    public bool IsRecording { get; private set; }

    public abstract bool IsSupported { get; }
    protected abstract Task StartRecordingPlatformAsync(string outputPath, bool useFrontCamera, bool captureAudio);
    protected abstract string? StopRecordingPlatform();

    protected virtual void OnError(Exception ex)
    {
        logger.LogError(ex, "Error in video recorder");
        ErrorOccurred?.Invoke(this, ex);
    }

    protected virtual void OnStatusChanged(bool isRecording)
    {
        IsRecording = isRecording;
        StatusChanged?.Invoke(this, isRecording);
    }

    public event EventHandler<bool>? StatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public async Task StartRecording(string outputPath, bool useFrontCamera = true, bool captureAudio = true)
    {
        if (IsRecording || !IsSupported)
            return;

        CurrentOutputPath = outputPath;
        if (File.Exists(outputPath))
            File.Delete(outputPath);
        
        await StartRecordingPlatformAsync(outputPath, useFrontCamera, captureAudio);
        IsRecording = true;
        StatusChanged?.Invoke(this, IsRecording);
    }

    public string? StopRecording()
    {
        try
        {
            if (!IsRecording)
                return null;

            var result = StopRecordingPlatform();
            IsRecording = false;
            
            StatusChanged?.Invoke(this, IsRecording);
            return result;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            IsRecording = false;
            throw;
        }
    }
}
