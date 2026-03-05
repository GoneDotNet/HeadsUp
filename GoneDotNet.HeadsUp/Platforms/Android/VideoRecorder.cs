using Android;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace GoneDotNet.HeadsUp.Services;


[Singleton(Type = typeof(IVideoRecorder))]
public class VideoRecorder(ILogger<VideoRecorder> logger) : BaseVideoRecorder(logger)
{
    CameraDevice? cameraDevice;
    CameraCaptureSession? captureSession;
    MediaRecorder? mediaRecorder;
    CameraManager? cameraManager;
    string? cameraId;

    // Android supports video recording
    public override bool IsSupported => true;

    
    protected override async Task StartRecordingPlatformAsync(string outputPath, bool useFrontCamera, bool captureAudio)
    {
        try
        {
            // Check permissions
            if (!await CheckPermissionsAsync())
                throw new UnauthorizedAccessException("Camera and audio recording permissions are required");

            this.cameraManager = (CameraManager)Platform.CurrentActivity?.GetSystemService(Context.CameraService)!;
            
            // Find the appropriate camera
            this.cameraId = this.GetCameraId(useFrontCamera);
            if (string.IsNullOrEmpty(this.cameraId))
                throw new InvalidOperationException($"No {(useFrontCamera ? "front" : "back")} camera found");

            // Setup MediaRecorder - order matters: sources, output format, encoders, then prepare
            // Use Context constructor (API 31+) as parameterless constructor is deprecated
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                this.mediaRecorder = new MediaRecorder(Platform.CurrentActivity!);
            }
            else
            {
#pragma warning disable CA1422 // Validate platform compatibility
                this.mediaRecorder = new MediaRecorder();
#pragma warning restore CA1422
            }

            if (captureAudio)
                this.mediaRecorder.SetAudioSource(AudioSource.Mic);
            
            this.mediaRecorder.SetVideoSource(VideoSource.Surface);
            this.mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.mediaRecorder.SetOutputFile(outputPath);
            this.mediaRecorder.SetVideoEncoder(VideoEncoder.H264);

            if (captureAudio)
                this.mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            
            this.mediaRecorder.SetVideoSize(1920, 1080);
            this.mediaRecorder.SetVideoFrameRate(30);
            this.mediaRecorder.SetVideoEncodingBitRate(10000000);
            this.mediaRecorder.Prepare();

            // Open camera
            var cameraStateCallback = new CameraStateCallback(this);
            this.cameraManager.OpenCamera(this.cameraId, cameraStateCallback, null);

            // Wait for camera to open (simplified - in production you'd want proper async handling)
            await Task.Delay(1000);

            if (this.cameraDevice == null)
                throw new InvalidOperationException("Failed to open camera");

            // Create capture session
            var surfaces = new List<Surface>();
            if (this.mediaRecorder.Surface != null)
                surfaces.Add(this.mediaRecorder.Surface);

            var captureStateCallback = new CaptureStateCallback(this);
            // this._cameraDevice.CreateCaptureSession(new SessionConfiguration());
            this.cameraDevice.CreateCaptureSession(surfaces, captureStateCallback, null);
            
            // Wait for session to be ready
            await Task.Delay(500);

            // Start recording
            this.mediaRecorder.Start();
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
            this.mediaRecorder?.Stop();
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
        if (this.cameraManager == null) return null;

        var cameraIds = this.cameraManager.GetCameraIdList();
        
        foreach (var id in cameraIds)
        {
            var characteristics = this.cameraManager.GetCameraCharacteristics(id);
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
        try { this.captureSession?.Close(); }
        catch (Exception ex) { this.OnError(ex); }
        finally { this.captureSession = null; }

        try { this.cameraDevice?.Close(); }
        catch (Exception ex) { this.OnError(ex); }
        finally { this.cameraDevice = null; }

        try { this.mediaRecorder?.Release(); }
        catch (Exception ex) { this.OnError(ex); }
        finally { this.mediaRecorder = null; }
    }

    
    private class CameraStateCallback(VideoRecorder service) : CameraDevice.StateCallback
    {
        public override void OnOpened(CameraDevice camera)
        {
            service.cameraDevice = camera;
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            camera.Close();
            service.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera.Close();
            
            service.cameraDevice = null;
            service.OnError(new InvalidOperationException($"Camera Error: {error}"));
        }
    }

    class CaptureStateCallback(VideoRecorder service) : CameraCaptureSession.StateCallback
    {
        public override void OnConfigured(CameraCaptureSession session)
        {
            service.captureSession = session;
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            // service.logger.LogError("Capture session configuration failed");
        }
    }
}