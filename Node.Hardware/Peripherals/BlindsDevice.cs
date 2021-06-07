using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode.Peripherals
{
    //TODO: This is an abstraction on a device, not a device itself. Should it be combined with BlindsCapability? 
    public class BlindsDevice
    {
        private readonly Transmitter433 _transmitter433;
        private readonly RadioSymbol[] _startPattern;
        private readonly RadioSymbol[] _repeatPattern;
        public int BcmPin => _transmitter433.BcmPin;
        
        public BlindsDevice(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
            _startPattern = new[] {new RadioSymbol(4000, true), new RadioSymbol(2500, false), new RadioSymbol(1000, true)}; 
            _repeatPattern = new[] {new RadioSymbol(5000, true), new RadioSymbol(2500, false), new RadioSymbol(1000, true)}; 
        }

        public async Task OpenOneStep()
        {

            //                 something    channel            ch crc  co crc 
            //                     |           |      command      |   |
            var upCh1   = "11011101001101101 1000 0011 1000 1000 0011 1000 1";
            var stopCh1 = "11011101001101101 1000 1010 1000 0000 0011 1001 1";
            var downCh1 = "11011101001101101 1000 1000 1000 1000 0011 0011 1";
            var upCh2   = "11011101001101101 0100 0011 1000 1000 1101 1000 1";
            var stopCh2 = "11011101001101101 0100 1010 1000 0000 1101 1001 1";
            var downCh2 = "11011101001101101 0100 1000 1000 1000 1101 0011 1";

            var fullUp  = "11011101001101101 0100 0011 1000 0000 1101 0100 1";
            var fullDon = "11011101001101101 0100 1000 1000 0000 1101 1011 1";
            
            
            var data = bits.SelectMany(bit => 
                bit == '1' ? 
                    new [] {new RadioSymbol(725, true), new RadioSymbol(350, false)} : 
                    new [] {new RadioSymbol(350, true), new RadioSymbol(725, false)}
            ).ToList();

            var buffer = _startPattern.Concat(data);
            var buffer2 = _repeatPattern.Concat(data).ToArray();

            for (int i = 0; i < 10; ++i)
            {
                buffer = buffer.Concat(buffer2);
            }

            await _transmitter433.Transmit(buffer.ToArray());
        }

        public async Task OpenFull()
        {
            
        }

        public void Close()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}