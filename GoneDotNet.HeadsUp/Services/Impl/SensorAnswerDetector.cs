using System.Diagnostics;
using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;


namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorAnswerDetector(ILogger<SensorAnswerDetector> logger) : IAnswerDetector
{
    private readonly FlipUpDetector _flipUpDetector = new();
    IAccelerometer Acc => Accelerometer.Default;
    IGyroscope Gyro => Gyroscope.Default;

    public event EventHandler<AnswerType>? AnswerDetected;
    
    public Task Start()
    {
        this._flipUpDetector.OnFlipped = state =>
        {
            switch (state)
            {
                case FlipUpDetector.State.FlippedUp:
                    // Flip up means pass
                    this.AnswerDetected?.Invoke(this, AnswerType.Pass);
                    break;
                case FlipUpDetector.State.FlippedDown:
                    // Flip down means correct/success
                    this.AnswerDetected?.Invoke(this, AnswerType.Success);
                    break;
            }
        };

        if (Acc is { IsSupported: true, IsMonitoring: false })
        {
            Acc.ReadingChanged += AccOnReadingChanged;
            Acc.Start(SensorSpeed.Game);
        }

        if (Gyro is { IsSupported: true, IsMonitoring: false })
        {
            Gyro.ReadingChanged += GyroOnReadingChanged;
            Gyro.Start(SensorSpeed.Game);
        }

        return Task.CompletedTask;
    }
    

    public Task Stop()
    {
        if (Acc is { IsMonitoring: true, IsSupported: true })
            Acc.Stop();
        
        if (Gyro is { IsMonitoring: true, IsSupported: true })
            Gyro.Stop();

        this._flipUpDetector.OnFlipped = null;
        Acc.ReadingChanged -= AccOnReadingChanged;
        Gyro.ReadingChanged += GyroOnReadingChanged;
        return Task.CompletedTask;
    }

    void GyroOnReadingChanged(object? sender, GyroscopeChangedEventArgs args)
    {
        this._flipUpDetector.OnGyro(args);
    }
    
    
    void AccOnReadingChanged(object? sender, AccelerometerChangedEventArgs args)
    {
        // TODO: device orientation may matter (reverse on landscape reverse?
        this._flipUpDetector.OnAccel(args);
        //logger.LogDebug($"X: {v.X} - Y: {v.Y} - Z: {v.Z}");
    }
}

public class FlipUpDetector
{
    public Action<FlipUpDetector.State>? OnFlipped;
    // Tweak these:
    const double PitchStartMaxDeg = 25;      // must start ~upright-ish
    const double PitchEndMinDeg   = 60;      // end at least this pitched up/down
    const double PitchDeltaMinDeg = 45;      // minimum change during flip
    const int    WindowMs         = 350;     // must complete within this window
    const double RollEndMaxDeg    = 35;      // keep roll reasonable at the end
    const double MinG             = 0.8;     // sanity check gravity magnitude
    const double MaxG             = 1.2;

    // If using gyro:
    const double GyroSpikeRadPerSec = 2.0;
    bool useGyro = true;

    public enum State { Idle, Primed, FlippedUp, FlippedDown }
    State state = State.Idle;

    double startPitchDeg;
    double peakPitchDeg;
    double minPitchDeg;
    long   startMs;
    bool   gyroSpike;

    internal void OnGyro(GyroscopeChangedEventArgs e)
    {
        // We care about a spike around the pitch axis.
        // With the pitch formula below, that's mostly rotation about device Y.
        var (gx, gy, gz) = (e.Reading.AngularVelocity.X,
                            e.Reading.AngularVelocity.Y,
                            e.Reading.AngularVelocity.Z);

        if (Math.Abs(gy) > GyroSpikeRadPerSec)
            gyroSpike = true;
    }

    internal void OnAccel(AccelerometerChangedEventArgs e)
    {
        var (ax, ay, az) = (e.Reading.Acceleration.X,
                            e.Reading.Acceleration.Y,
                            e.Reading.Acceleration.Z);

        // Gravity magnitude sanity
        var g = Math.Sqrt(ax*ax + ay*ay + az*az);
        if (g < MinG || g > MaxG) return;

        // Compute roll/pitch from accel
        var rollRad  = Math.Atan2(ay, az);
        var pitchRad = Math.Atan2(-ax, Math.Sqrt(ay*ay + az*az));
        var rollDeg  = rollRad  * 180.0 / Math.PI;
        var pitchDeg = pitchRad * 180.0 / Math.PI;

        var nowMs = Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency;

        switch (state)
        {
            case State.Idle:
                // Wait until we're roughly upright/neutral
                if (Math.Abs(pitchDeg) <= PitchStartMaxDeg)
                {
                    startPitchDeg = pitchDeg;
                    peakPitchDeg  = pitchDeg;
                    minPitchDeg   = pitchDeg;
                    gyroSpike     = false;
                    startMs       = (long)nowMs;
                    state = State.Primed;
                }
                break;

            case State.Primed:
                // Track both up and down movement
                peakPitchDeg = Math.Max(peakPitchDeg, pitchDeg);
                minPitchDeg = Math.Min(minPitchDeg, pitchDeg);

                var dtPrimed = nowMs - startMs;
                if (dtPrimed > WindowMs)
                {
                    state = State.Idle; // too slow; reset
                    break;
                }

                var upDelta = peakPitchDeg - startPitchDeg;
                var downDelta = startPitchDeg - minPitchDeg;
                
                // Check for flip UP (device tilted away from user, positive pitch increase)
                if (upDelta >= PitchDeltaMinDeg && peakPitchDeg >= PitchEndMinDeg)
                {
                    var goodRoll = Math.Abs(rollDeg) <= RollEndMaxDeg;
                    var fastEnough = !useGyro || gyroSpike;

                    if (goodRoll && fastEnough)
                    {
                        state = State.FlippedUp;
                        OnFlipped?.Invoke(state);
                    }
                }
                // Check for flip DOWN (device tilted toward user, negative pitch decrease)  
                else if (downDelta >= PitchDeltaMinDeg && minPitchDeg <= -PitchEndMinDeg)
                {
                    var goodRoll = Math.Abs(rollDeg) <= RollEndMaxDeg;
                    var fastEnough = !useGyro || gyroSpike;

                    if (goodRoll && fastEnough)
                    {
                        state = State.FlippedDown;
                        OnFlipped?.Invoke(state);
                    }
                }
                break;

            case State.FlippedUp:
            case State.FlippedDown:
                // Reset to idle after detection
                state = State.Idle;
                break;
        }
    }
}