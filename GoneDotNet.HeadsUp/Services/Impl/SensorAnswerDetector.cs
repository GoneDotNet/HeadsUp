using System.Numerics;

namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorAnswerDetector(ILogger<SensorAnswerDetector> logger) : IAnswerDetector
{
    readonly object syncLock = new();
    bool isMonitoring;
    Vector3 baselineAcceleration;
    bool hasBaseline;
    DateTime lastFlipTime = DateTime.MinValue;
    DisplayOrientation currentOrientation = DisplayOrientation.Portrait;
    const double FlipThresholdDegrees = 35.0;
    const int FlipCooldownMs = 1000; // Prevent rapid fire flips
    
    public event EventHandler<AnswerType>? AnswerDetected;
    
    public Task Start()
    {
        lock (syncLock)
        {
            if (isMonitoring) return Task.CompletedTask;
            
            logger.LogInformation("Starting sensor answer detector");
            
            // Reset state
            hasBaseline = false;
            lastFlipTime = DateTime.MinValue;
            
            // Get current device orientation
            UpdateDeviceOrientation();
            
            var accelerometer = Accelerometer.Default;
            
            if (accelerometer.IsSupported)
            {
                accelerometer.ReadingChanged += OnAccelerometerReadingChanged;
                accelerometer.Start(SensorSpeed.Game);
                isMonitoring = true;
                logger.LogInformation("Accelerometer started successfully with orientation: {Orientation}", currentOrientation);
            }
            else
            {
                logger.LogWarning("Accelerometer not supported on this device");
            }
        }
        
        return Task.CompletedTask;
    }
    
    public Task Stop()
    {
        lock (syncLock)
        {
            if (!isMonitoring) return Task.CompletedTask;
            
            logger.LogInformation("Stopping sensor answer detector");
            
            var accelerometer = Accelerometer.Default;
            if (accelerometer.IsMonitoring)
            {
                accelerometer.ReadingChanged -= OnAccelerometerReadingChanged;
                accelerometer.Stop();
            }
            
            isMonitoring = false;
            hasBaseline = false;
        }
        
        return Task.CompletedTask;
    }
    
    private void UpdateDeviceOrientation()
    {
        try
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            currentOrientation = displayInfo.Orientation;
            logger.LogDebug("Device orientation updated to: {Orientation}", currentOrientation);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not get device orientation, defaulting to Portrait");
            currentOrientation = DisplayOrientation.Portrait;
        }
    }
    
    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        try
        {
            var acceleration = e.Reading.Acceleration;
            
            // Update orientation periodically (but not too frequently)
            if (DateTime.UtcNow.Subtract(lastFlipTime).TotalSeconds > 2)
            {
                UpdateDeviceOrientation();
            }
            
            // Establish baseline when phone is held normally on head
            if (!hasBaseline)
            {
                EstablishBaseline(acceleration);
                return;
            }
            
            // Calculate the current flip angle relative to baseline based on device orientation
            var currentAngle = CalculateFlipAngle(acceleration);
            var baselineAngle = CalculateFlipAngle(baselineAcceleration);
            var angleDifference = currentAngle - baselineAngle;
            
            // Log angle data for debugging
            logger.LogDebug("Orientation: {Orientation}, Current: {Current:F1}°, Baseline: {Baseline:F1}°, Diff: {Diff:F1}°", 
                           currentOrientation, currentAngle, baselineAngle, angleDifference);
            
            // Check for flip gestures with cooldown
            var now = DateTime.UtcNow;
            if ((now - lastFlipTime).TotalMilliseconds < FlipCooldownMs)
                return;
                
            if (Math.Abs(angleDifference) >= FlipThresholdDegrees)
            {
                // Determine flip type based on orientation and angle change
                var flipType = DetermineFlipType(angleDifference);
                
                logger.LogInformation("Flip detected: {FlipType}, Angle difference: {AngleDifference:F1}°, Orientation: {Orientation}", 
                                    flipType, angleDifference, currentOrientation);
                
                lastFlipTime = now;
                AnswerDetected?.Invoke(this, flipType);
                
                // Reset baseline after flip to allow for continuous play
                Task.Delay(800).ContinueWith(_ => hasBaseline = false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing accelerometer reading");
        }
    }
    
    private AnswerType DetermineFlipType(double angleDifference)
    {
        // Determine flip type based on device orientation
        return currentOrientation switch
        {
            DisplayOrientation.Portrait => angleDifference > 0 ? AnswerType.Pass : AnswerType.Success,
            DisplayOrientation.Landscape => angleDifference < 0 ? AnswerType.Pass : AnswerType.Success,
            _ => angleDifference > 0 ? AnswerType.Pass : AnswerType.Success
        };
    }
    
    private void EstablishBaseline(Vector3 acceleration)
    {
        // Only establish baseline when device is relatively stable
        var magnitude = Math.Sqrt(acceleration.X * acceleration.X + 
                                 acceleration.Y * acceleration.Y + 
                                 acceleration.Z * acceleration.Z);
        
        // Check if we're close to 1G (device is stable)
        if (magnitude >= 0.8 && magnitude <= 1.2)
        {
            baselineAcceleration = acceleration;
            hasBaseline = true;
            logger.LogDebug("Baseline established for {Orientation}: X={X:F2}, Y={Y:F2}, Z={Z:F2}", 
                           currentOrientation, acceleration.X, acceleration.Y, acceleration.Z);
        }
    }
    
    private double CalculateFlipAngle(Vector3 acceleration)
    {
        // Calculate the appropriate flip angle based on device orientation
        // This determines which axis represents the "flip" motion
        
        var magnitude = Math.Sqrt(acceleration.X * acceleration.X + 
                                 acceleration.Y * acceleration.Y + 
                                 acceleration.Z * acceleration.Z);
        
        if (magnitude < 0.1) return 0; // Avoid division by zero
        
        var normalizedX = acceleration.X / magnitude;
        var normalizedY = acceleration.Y / magnitude;
        var normalizedZ = acceleration.Z / magnitude;
        
        double angle = currentOrientation switch
        {
            DisplayOrientation.Portrait => 
                // In portrait, flip is around X-axis (pitch)
                Math.Atan2(-normalizedX, Math.Sqrt(normalizedY * normalizedY + normalizedZ * normalizedZ)),
                
            DisplayOrientation.Landscape => 
                // In landscape, flip is around Y-axis (roll)
                Math.Atan2(-normalizedY, Math.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ)),
                
            _ => // Default to portrait behavior
                Math.Atan2(-normalizedX, Math.Sqrt(normalizedY * normalizedY + normalizedZ * normalizedZ))
        };
        
        var angleDegrees = angle * 180.0 / Math.PI;
        return Math.Max(-90, Math.Min(90, angleDegrees));
    }
}