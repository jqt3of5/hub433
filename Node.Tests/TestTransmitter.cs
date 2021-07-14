using System.Linq;
using System.Threading.Tasks;
using HardwareAbstractionServiceRPI.Peripherals;
using Node.Hardware.Peripherals;
using NUnit.Framework;
using RPINode;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using BootstrapPiGpio = Node.Hardware.BootstrapPiGpio;

namespace Node.Tests
{
    public class TestTransmitter    
    {
        [SetUp]
        public void Setup()
        {
            Assert.That(Unosquare.PiGpio.NativeMethods.Setup.GpioInitialise(), Is.EqualTo(ResultCode.Ok));
        }

        [Test]
        public async Task TransmitBitPatterns()
        {
            var pin = Board.Pins[UserGpio.Bcm17];
            var transmitter = new Transmitter433(new BootstrapPiGpio.PiGpioController.PiGpioPin(pin));

            var bits = "10101010";
            var symbols = bits.SelectMany(bit => 
                bit == '1' ? 
                    new [] {new RadioSymbol(650, true), new RadioSymbol(300, false)} : 
                        new [] {new RadioSymbol(300, true), new RadioSymbol(650, false)}
                ).ToArray();
            await transmitter.Transmit(symbols);
        }
    }
}