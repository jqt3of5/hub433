using System;
using System.Linq;
using Node.Hardware;
using Node.Hardware.Peripherals;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Sampler433
{
    class Program
    {
        static void Main(string[] args)
        {
            Pi.Init<BootstrapPiGpio>();
            switch (args[0])
            {
                case "transmit":
                    var transmitter = new Transmitter433(Pi.Gpio[BcmPin.Gpio17]);
                    var pattern = args[1];
                    while (true)
                    {
                        var symbols = pattern.SelectMany(bit => bit == '1' ? high() : low());
                        transmitter.Transmit(symbols.ToArray());
                    }
                    break;
                case "receive":
                    var receiver = new Receiver433(Pi.Gpio[BcmPin.Gpio27]);
                    var symboles = receiver.Receive(TimeSpan.FromSeconds(10));
                    Console.WriteLine($"total symbol ticks:{symboles.Sum(s => s.DurationUS)} average symbol length: {symboles.Average(s => s.DurationUS)}");
                    break;
            }

            RadioSymbol[] high() => new[] {new RadioSymbol(750, true), new RadioSymbol(250, false)};
            RadioSymbol[] low() => new[] {new RadioSymbol(250, true), new RadioSymbol(750, false)};
        }
    }
}