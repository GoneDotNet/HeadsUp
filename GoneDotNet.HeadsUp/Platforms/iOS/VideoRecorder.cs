using AVFoundation;
using Foundation;
using UIKit;

namespace GoneDotNet.HeadsUp.Services;


[Singleton]
public class VideoRecorder : BaseVideoRecorder
{
    AVCaptureSession? _captureSession;
    AVCaptureMovieFileOutput? _movieOutput;
    AVCaptureDevice? _videoDevice;
    AVCaptureDeviceInput? _videoInput;
    AVCaptureDeviceInput? _audioInput;
    NSUrl? _outputFileUrl;


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
            _captureSession = new AVCaptureSession();
            _captureSession.SessionPreset = AVCaptureSession.Preset1920x1080;

            // Setup video input
            var devicePosition = useFrontCamera ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            _videoDevice = GetCameraDevice(devicePosition);

            if (_videoDevice == null)
                throw new InvalidOperationException($"No {(useFrontCamera ? "front" : "back")} camera found");

            _videoInput = new AVCaptureDeviceInput(_videoDevice, out var videoError);
            if (videoError != null)
                throw new InvalidOperationException($"Failed to create video input: {videoError.LocalizedDescription}");

            if (!_captureSession.CanAddInput(_videoInput))
                throw new InvalidOperationException("Cannot add video input to capture session");

            _captureSession.AddInput(_videoInput);

            // Setup audio input
            if (captureAudio)
            {
                var audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
                if (audioDevice != null)
                {
                    _audioInput = new AVCaptureDeviceInput(audioDevice, out var audioError);
                    if (audioError == null && _captureSession.CanAddInput(_audioInput))
                        _captureSession.AddInput(_audioInput);
                }
            }

            // Setup movie file output
            _movieOutput = new AVCaptureMovieFileOutput();
            if (!_captureSession.CanAddOutput(_movieOutput))
                throw new InvalidOperationException("Cannot add movie output to capture session");
            
            _captureSession.AddOutput(_movieOutput);

            _outputFileUrl = NSUrl.FromFilename(outputPath);
            _captureSession.StartRunning();

            // Start recording
            var recordingDelegate = new MovieFileOutputRecordingDelegate(this);
            _movieOutput.StartRecordingToOutputFile(_outputFileUrl, recordingDelegate);

            return true;
        }
        finally
        {
            this.Cleanup();
        }
    }

    protected override string? StopRecordingPlatform()
    {
        try
        {
            _movieOutput?.StopRecording();
            this.Cleanup();
            return _outputFileUrl?.Path;
        }
        catch
        {
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
            _captureSession?.StopRunning();

            if (_videoInput != null)
            {
                _captureSession?.RemoveInput(_videoInput);
                _videoInput?.Dispose();
                _videoInput = null;
            }

            if (_audioInput != null)
            {
                _captureSession?.RemoveInput(_audioInput);
                _audioInput?.Dispose();
                _audioInput = null;
            }

            if (_movieOutput != null)
            {
                _captureSession?.RemoveOutput(_movieOutput);
                _movieOutput?.Dispose();
                _movieOutput = null;
            }

            _captureSession?.Dispose();
            _captureSession = null;

            _videoDevice?.Dispose();
            _videoDevice = null;
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
