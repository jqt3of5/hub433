using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            Pi.Init<BootstrapPiGpio>();
            
            var pin = Pi.Gpio[BcmPin.Gpio17];
            var transmitter = new Transmitter433(pin);
            var blinds = new Blinds(transmitter);

            await blinds.Broadcast(Blinds.BlindsChannel.Channel1, Blinds.BlindsCommand.Open);
           
            Setup.GpioTerminate();
        }
    }
}