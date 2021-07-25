using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Node.Hardware.Capability;
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
            var blinds = new BlindsCapability(transmitter);

            await blinds.Broadcast(BlindsCapability.BlindsChannel.Channel1, BlindsCapability.BlindsCommand.Open);
           
            Setup.GpioTerminate();
        }
    }
}