using System;
using System.Linq;
using Node.Hardware.Peripherals;

namespace Sampler433
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "transmit":
                    var transmitter = new Transmitter433();
                    var pattern = args[1];
                    while (true)
                    {
                        var symbols = pattern.SelectMany(bit => bit == '1' ? high() : low());
                        transmitter.Transmit(symbols.ToArray());
                    }
                    break;
                case "receive":
                    var receiver = new Receiver433();
                    break;
            }

            RadioSymbol[] high() => new[] {new RadioSymbol(750, true), new RadioSymbol(250, false)};
            RadioSymbol[] low() => new[] {new RadioSymbol(250, true), new RadioSymbol(750, false)};
        }
    }
}