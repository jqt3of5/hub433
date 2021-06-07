using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RPINode;
using RPINode.Peripherals;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace Node.Tests.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (Setup.GpioInitialise() < 0)
            {
                System.Console.WriteLine("Failed to initialize. was your libpigpio.so built from a raspberrypi 4?");
                Setup.GpioTerminate();
                return;
            }

            var pin = Board.Pins[UserGpio.Bcm17];
            var transmitter = new Transmitter433(pin);
            var blinds = new BlindsDevice(transmitter);

            await blinds.OpenOneStep();
           
            
            Setup.GpioTerminate();
        }
    }
}