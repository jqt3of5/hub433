using System;
using System.Threading;
using System.Threading.Tasks;
using Swan.Parsers;
using Unosquare.RaspberryIO.Abstractions;

namespace HardwareAbstractionServiceRPI.Peripherals
{
    public static class Defaultable
    {
        public static Defaultable<T> FromDefault<T>(T @default) => new Defaultable<T>(@default); 
    }
    public record Defaultable<T> 
    {
        public T Default { get; }

        private T? _value;

        public T Value
        {
            get => _value ?? Default;
            set => _value = value;
        }
        public Defaultable(T @default)
        {
            Default = @default;
        }

        public Defaultable<T> Set(T v)
        {
            Value = v;
            return this;
        }
        public void Reset()
        {
            _value = default;
        }
        
        public static implicit operator T(Defaultable<T> d) => d.Value;
    }
    public class Relay
    {
        readonly int PwmRange = 255;
        public Defaultable<TimeSpan> MinDutyPeriod { get; set; } = Defaultable.FromDefault(TimeSpan.Zero);

        public Defaultable<TimeSpan> PwmPeriod { get; set; } = Defaultable.FromDefault(TimeSpan.FromSeconds(2));

        public Defaultable<TimeSpan> CompressorCooldown { get; set; } = Defaultable.FromDefault(TimeSpan.Zero);
        
        private readonly IGpioPin _pin;
        private readonly Timer _pwmTimer;

        private TimeSpan _lowPeriod;
        private TimeSpan _highPeriod;
        
        public int PwmValue { get; set; }

        public float Value => (float)PwmValue / PwmRange;

        public Relay(IGpioPin pin)
        {
            _pin = pin;
            _pin.PinMode = GpioPinDriveMode.Output;
            _pwmTimer = new Timer(TimerCallback, null,  Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private bool _wavePart = true;
        private void TimerCallback(object? state)
        {
            var period = _wavePart ? _highPeriod : _lowPeriod;
            if (period != TimeSpan.Zero)
            {
                _pin.Value = _wavePart;
            }
            _pwmTimer.Change(period, Timeout.InfiniteTimeSpan);
            
            _wavePart = !_wavePart;
        }

        public Task TrySetValue(float value)
        {
            PwmValue = (int) (value * PwmRange);
            _lowPeriod = PwmPeriod.Value * ((float)PwmValue / PwmRange);
            _highPeriod = PwmPeriod.Value * ((float)(PwmRange - PwmValue) / PwmRange);
            
            //It's possible that some devices will have a minimum on or off time, we need to make sure that we don't ever run afoul of that. 
            //TODO: will become obsolete
            if (_lowPeriod < MinDutyPeriod)
            {
                _highPeriod = PwmPeriod.Value - MinDutyPeriod;
                _lowPeriod = MinDutyPeriod;
            }

            if (_highPeriod < MinDutyPeriod)
            {
                _lowPeriod = PwmPeriod.Value - MinDutyPeriod;
                _highPeriod = MinDutyPeriod; 
            }
            _pwmTimer.Change(_highPeriod, Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }
    }
}