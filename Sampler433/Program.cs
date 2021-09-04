using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware;
using Node.Hardware.Peripherals;
using Swan.Diagnostics;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.RaspberryIO.Peripherals;

namespace Sampler433
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Pi.Init<BootstrapPiGpio>();
            // Pi.Init<BootstrapMock>();
            switch (args[0])
            {
                case "blinds":
                    var trans= new Transmitter433(Pi.Gpio[BcmPin.Gpio17]);
                    var blinds = new Blinds(trans);
                    await blinds.Broadcast(Blinds.BlindsChannel.Channel2, Blinds.BlindsCommand.Up);
                    break;
                case "transmit":
                    var transmitter = new Transmitter433(Pi.Gpio[BcmPin.Gpio17]);
                    var pattern = args[1];
                    while (true)
                    {
                        var symbols = pattern.SelectMany(bit => bit == '1' ? high() : low());
                        await transmitter.Transmit(symbols.ToArray());
                    }
                    break;
                case "receive":
                    var receiver = new Receiver433(Pi.Gpio[BcmPin.Gpio27]);
                    var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var receivedSymbols = await receiver.Receive(source.Token);

                    Console.WriteLine($"total symbol ticks:{receivedSymbols.Sum(s => s.DurationUS)}");

                    foreach (var radioSymbol in receivedSymbols)
                    {
                        Console.Write($"{(radioSymbol.Value? "1" : "0")}({radioSymbol.DurationUS}) ");
                    }
                    
                    break;
            }
            
            Setup.GpioTerminate();

            RadioSymbol[] high() => new[] {new RadioSymbol(750, true), new RadioSymbol(250, false)};
            RadioSymbol[] low() => new[] {new RadioSymbol(250, true), new RadioSymbol(750, false)};
        }
    }
}