using Android;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace GoneDotNet.HeadsUp.Services;


[Singleton]
public class VideoRecorder : BaseVideoRecorder
{
    CameraDevice? _cameraDevice;
    CameraCaptureSession? _captureSession;
    MediaRecorder? _mediaRecorder;
    CameraManager? _cameraManager;
    string? _cameraId;

    // Android supports video recording
    public override bool IsSupported => true;

    
    protected override async Task StartRecordingPlatformAsync(string outputPath, bool useFrontCamera, bool captureAudio)
    {
        try
        {
            // Check permissions
            if (!await CheckPermissionsAsync())
                throw new UnauthorizedAccessException("Camera and audio recording permissions are required");

            _cameraManager = (CameraManager)Platform.CurrentActivity?.GetSystemService(Context.CameraService)!;
            
            // Find the appropriate camera
            _cameraId = this.GetCameraId(useFrontCamera);
            if (string.IsNullOrEmpty(_cameraId))
                throw new InvalidOperationException($"No {(useFrontCamera ? "front" : "back")} camera found");

            // Setup MediaRecorder
            _mediaRecorder = new MediaRecorder();
            
            _mediaRecorder.SetVideoSource(VideoSource.Surface);
            _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            _mediaRecorder.SetOutputFile(outputPath);
            _mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            
            _mediaRecorder.SetVideoSize(1920, 1080);
            _mediaRecorder.SetVideoFrameRate(30);
            _mediaRecorder.SetVideoEncodingBitRate(10000000);
            _mediaRecorder.Prepare();

            if (captureAudio)
            {
                _mediaRecorder.SetAudioSource(AudioSource.Mic);
                _mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            }

            // Open camera
            var cameraStateCallback = new CameraStateCallback(this);
            _cameraManager.OpenCamera(_cameraId, cameraStateCallback, null);

            // Wait for camera to open (simplified - in production you'd want proper async handling)
            await Task.Delay(1000);

            if (_cameraDevice == null)
                throw new InvalidOperationException("Failed to open camera");

            // Create capture session
            var surfaces = new List<Surface>();
            if (_mediaRecorder.Surface != null)
                surfaces.Add(_mediaRecorder.Surface);

            var captureStateCallback = new CaptureStateCallback(this);
            // this._cameraDevice.CreateCaptureSession(new SessionConfiguration());
            _cameraDevice.CreateCaptureSession(surfaces, captureStateCallback, null);
            
            // Wait for session to be ready
            await Task.Delay(500);

            // Start recording
            _mediaRecorder.Start();
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    protected override string? StopRecordingPlatform()
    {
        try
        {
            _mediaRecorder?.Stop();
            this.Cleanup();
            return CurrentOutputPath;
        }
        catch
        {
            this.Cleanup();
            throw;
        }
    }

    
    async Task<bool> CheckPermissionsAsync()
    {
        var cameraPermission = ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.Camera);
        var audioPermission = ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.RecordAudio);
        var storagePermission = ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.WriteExternalStorage);

        if (cameraPermission != Android.Content.PM.Permission.Granted ||
            audioPermission != Android.Content.PM.Permission.Granted ||
            storagePermission != Android.Content.PM.Permission.Granted)
        {
            if (Platform.CurrentActivity is AndroidX.AppCompat.App.AppCompatActivity activity)
            {
                ActivityCompat.RequestPermissions(activity, 
                    [Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.WriteExternalStorage], 
                    1001);
            }
            
            // Wait for permission result (simplified)
            await Task.Delay(2000);
            
            cameraPermission = ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.Camera);
            audioPermission = ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.RecordAudio);
            
            return cameraPermission == Android.Content.PM.Permission.Granted && 
                   audioPermission == Android.Content.PM.Permission.Granted;
        }

        return true;
    }

    string? GetCameraId(bool useFrontCamera)
    {
        if (_cameraManager == null) return null;

        var cameraIds = _cameraManager.GetCameraIdList();
        
        foreach (var id in cameraIds)
        {
            var characteristics = _cameraManager.GetCameraCharacteristics(id);
            var facing = (int)characteristics.Get(CameraCharacteristics.LensFacing)!;
            
            if (useFrontCamera && facing == (int)LensFacing.Front)
                return id;
            
            if (!useFrontCamera && facing == (int)LensFacing.Back)
                return id;
        }

        return null;
    }

    
    void Cleanup()
    {
        try
        {
            _captureSession?.Close();
            _captureSession = null;

            _cameraDevice?.Close();
            _cameraDevice = null;

            _mediaRecorder?.Release();
            _mediaRecorder = null;
        }
        catch (Exception ex)
        {
            this.OnError(ex);
        }
    }

    
    private class CameraStateCallback(VideoRecorder service) : CameraDevice.StateCallback
    {
        public override void OnOpened(CameraDevice camera)
        {
            service._cameraDevice = camera;
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            camera.Close();
            service._cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera.Close();
            
            service._cameraDevice = null;
            service.OnError(new InvalidOperationException($"Camera Error: {error}"));
        }
    }

    class CaptureStateCallback(VideoRecorder service) : CameraCaptureSession.StateCallback
    {
        public override void OnConfigured(CameraCaptureSession session)
        {
            service._captureSession = session;
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            // service.logger.LogError("Capture session configuration failed");
        }
    }
}