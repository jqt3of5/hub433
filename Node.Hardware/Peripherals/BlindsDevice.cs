using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
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

        public enum BlindsCommand
        {
            Up,
            Down,
            Stop,
            Open,
            Close
        }

        public enum BlindsChannel
        {
            Channel1,
            Channel2,
        }

        public async Task SendCommand(BlindsChannel channel, BlindsCommand blindsCommand)
        {
            const string preamble = "11011101001101101";

            (string cmd, string crc) cmdBits = blindsCommand switch
            {
                BlindsCommand.Up =>    (cmd : "001110001000", crc : "1000"),
                BlindsCommand.Down =>  (cmd: "100010001000", crc : "0011"),
                BlindsCommand.Stop =>  (cmd: "101010000000", crc : "1001"),
                BlindsCommand.Open =>  (cmd: "001110000000", crc : "0100"),
                BlindsCommand.Close => (cmd: "100010000000", crc : "1011"),
                _ => throw new ArgumentOutOfRangeException(nameof(blindsCommand), blindsCommand, null)
            };

            (string channel , string crc) chBits = channel switch
            {
                BlindsChannel.Channel1 => (channel: "1000", crc:"0011"),
                BlindsChannel.Channel2 => (channel: "0100", crc:"1101")
            };

            var bits = preamble + chBits.channel + cmdBits.cmd + chBits.crc + cmdBits.crc + "1";
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
    }
}