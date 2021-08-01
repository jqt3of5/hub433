using System;
using System.Threading;
using System.Threading.Tasks;
using Node.Abstractions;
using Unosquare.RaspberryIO.Abstractions;

namespace Node.Hardware.Peripherals
{
    public class Relay
    {
        public Defaultable<TimeSpan> MinDutyPeriod { get; set; } = Defaultable.FromDefault(TimeSpan.Zero);

        public Defaultable<TimeSpan> PwmPeriod { get; set; } = Defaultable.FromDefault(TimeSpan.FromSeconds(2));

        public Defaultable<TimeSpan> CompressorCooldown { get; set; } = Defaultable.FromDefault(TimeSpan.Zero);
        
        private readonly IGpioPin _pin;
        private readonly Timer _pwmTimer;

        private TimeSpan _lowPeriod;
        private TimeSpan _highPeriod;
        
        const int PwmRange = 1024;
        public int PwmValue { get; set; }

        public float DutyCycle => (float)PwmValue / PwmRange;

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

        public Task SetDutyCycle(float dutyCycle)
        {
            PwmValue = (int) (dutyCycle * PwmRange);
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