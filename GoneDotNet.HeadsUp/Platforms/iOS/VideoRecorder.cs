using AVFoundation;
using Foundation;
using UIKit;

namespace GoneDotNet.HeadsUp.Services;


[Singleton(Type = typeof(IVideoRecorder))]
public class VideoRecorder(ILogger<VideoRecorder> logger) : BaseVideoRecorder(logger)
{
    AVCaptureSession? captureSession;
    AVCaptureMovieFileOutput? movieOutput;
    AVCaptureDevice? videoDevice;
    AVCaptureDeviceInput? videoInput;
    AVCaptureDeviceInput? audioInput;
    NSUrl? outputFileUrl;


    public override bool IsSupported => UIDevice.CurrentDevice.CheckSystemVersion(10, 0); // iOS 10.0+

    protected override async Task<bool> StartRecordingPlatformAsync(string outputPath, bool useFrontCamera, bool captureAudio)
    {
        try
        {
            // Request permissions
            var cameraStatus = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video);
            var audioStatus = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Audio);

            if (!cameraStatus || !audioStatus)
                throw new UnauthorizedAccessException("Camera and microphone permissions are required");

            // Setup capture session
            this.captureSession = new AVCaptureSession();
            this.captureSession.SessionPreset = AVCaptureSession.Preset1920x1080;

            // Setup video input
            var devicePosition = useFrontCamera ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            this.videoDevice = GetCameraDevice(devicePosition);

            if (this.videoDevice == null)
                throw new InvalidOperationException($"No {(useFrontCamera ? "front" : "back")} camera found");

            this.videoInput = new AVCaptureDeviceInput(this.videoDevice, out var videoError);
            if (videoError != null)
                throw new InvalidOperationException($"Failed to create video input: {videoError.LocalizedDescription}");

            if (!this.captureSession.CanAddInput(this.videoInput))
                throw new InvalidOperationException("Cannot add video input to capture session");

            this.captureSession.AddInput(this.videoInput);

            // Setup audio input
            if (captureAudio)
            {
                var audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
                if (audioDevice != null)
                {
                    this.audioInput = new AVCaptureDeviceInput(audioDevice, out var audioError);
                    if (audioError == null && this.captureSession.CanAddInput(this.audioInput))
                        this.captureSession.AddInput(this.audioInput);
                }
            }

            // Setup movie file output
            this.movieOutput = new AVCaptureMovieFileOutput();
            if (!this.captureSession.CanAddOutput(this.movieOutput))
                throw new InvalidOperationException("Cannot add movie output to capture session");
            
            this.captureSession.AddOutput(this.movieOutput);

            this.outputFileUrl = NSUrl.FromFilename(outputPath);
            this.captureSession.StartRunning();

            // Start recording
            var recordingDelegate = new MovieFileOutputRecordingDelegate(this);
            this.movieOutput.StartRecordingToOutputFile(this.outputFileUrl, recordingDelegate);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start video recorder");
            this.Cleanup();
            return false;
        }
    }

    protected override string? StopRecordingPlatform()
    {
        try
        {
            this.movieOutput?.StopRecording();
            this.Cleanup();
            return this.outputFileUrl?.Path;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop video recorder");
            this.Cleanup();
            throw;
        }
    }

    static AVCaptureDevice? GetCameraDevice(AVCaptureDevicePosition position)
    {
        var devices = AVCaptureDeviceDiscoverySession
            .Create(
                [
                    AVCaptureDeviceType.BuiltInDualCamera,
                    AVCaptureDeviceType.BuiltInDualWideCamera,
                    AVCaptureDeviceType.BuiltInTripleCamera,
                    AVCaptureDeviceType.BuiltInTrueDepthCamera,
                    AVCaptureDeviceType.BuiltInUltraWideCamera,
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInTelephotoCamera
                ],
                AVMediaTypes.Video,
                position
            );

        return devices.Devices.FirstOrDefault();
    }


    void Cleanup()
    {
        try
        {
            this.captureSession?.StopRunning();

            if (this.videoInput != null)
            {
                this.captureSession?.RemoveInput(this.videoInput);
                this.videoInput?.Dispose();
                this.videoInput = null;
            }

            if (this.audioInput != null)
            {
                this.captureSession?.RemoveInput(this.audioInput);
                this.audioInput?.Dispose();
                this.audioInput = null;
            }

            if (this.movieOutput != null)
            {
                this.captureSession?.RemoveOutput(this.movieOutput);
                this.movieOutput?.Dispose();
                this.movieOutput = null;
            }

            this.captureSession?.Dispose();
            this.captureSession = null;

            this.videoDevice?.Dispose();
            this.videoDevice = null;
        }
        catch (Exception ex)
        {
            this.OnError(ex);
        }
    }


    internal void ProcessDelegate(NSError? error, NSUrl outputFileUrl)
    {
        if (error != null)
        {
            this.OnError(new Exception(error.LocalizedDescription));
        }
        else
        {
            this.OnStatusChanged(false);
        }
    }

    class MovieFileOutputRecordingDelegate(VideoRecorder service) : AVCaptureFileOutputRecordingDelegate
    {
        public override void FinishedRecording(
            AVCaptureFileOutput output,
            NSUrl outputFileUrl,
            NSObject[] connections,
            NSError? error
        ) => service.ProcessDelegate(error, outputFileUrl);
    }
}
