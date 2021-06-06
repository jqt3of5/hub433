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
            var bits = "110111010011011010100001110001000110110001";
            var data = bits.SelectMany(bit => 
                bit == '1' ? 
                    new [] {new RadioSymbol(725, true), new RadioSymbol(350, false)} : 
                    new [] {new RadioSymbol(350, true), new RadioSymbol(725, false)}
            ).ToList();

            var buffer = _startPattern.Concat(data);
            var buffer2 = _repeatPattern.Concat(data);
            
            await _transmitter433.Transmit(buffer.Concat(buffer2).Concat(buffer2).Concat(buffer2).Concat(buffer2).Concat(buffer2).Concat(buffer2).ToArray()); 
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