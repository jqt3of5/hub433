using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO.Peripherals;

namespace Node.Hardware.Peripherals
{
    public class DhtCore
    {
        private (double humidity, double temperature)? _lastValue;
        private DhtSensor InternalDevice { get; } 

        public DhtCore(IGpioPin pin)
        {
            InternalDevice = DhtSensor.Create(DhtType.Dht11, pin);
            InternalDevice.OnDataAvailable += DeviceOnOnDataAvailable;
        }

        private TaskCompletionSource<(double humidity, double temperature)>? _readSource;
        private void DeviceOnOnDataAvailable(object? sender, DhtReadEventArgs e)
        {
            if (e.IsValid)
            {

                _lastValue = (e.HumidityPercentage, e.Temperature);
                _readSource?.SetResult(_lastValue.Value);
            }
            else
            {
                _readSource?.SetResult((double.NaN, double.NaN));
            }
        }

        public (double humidity, double temperature)? GetLastValue()
        {
            return _lastValue;
        }
        
        public Task<(double humidity, double temperature)> GetNextValue()
        {
            lock (this)
            {
                if (_readSource == null)
                {
                    _readSource = new TaskCompletionSource<(double, double)>();
                }
            }
           
            return _readSource.Task;
        }
    }
}