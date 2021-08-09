using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using BootstrapPiGpio = Node.Hardware.BootstrapPiGpio;

namespace Node.Tests.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Pi.Init<BootstrapPiGpio>();
            Pi.Init<BootstrapMock>(); 
            var pin = Pi.Gpio[BcmPin.Gpio17];
            var transmitter = new Transmitter433(pin);
            var relay = new RemoteRelay(transmitter);
            await relay.On(0, new[] {0, 1, 2, 3});
            
            await Task.Delay(500);
            
            await relay.Off(0, new[] {0, 1, 2, 3});
           
            // Setup.GpioTerminate();
        }
    }
}