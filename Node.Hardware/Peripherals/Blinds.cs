using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPINode;

namespace Node.Hardware.Peripherals
{
    public class Blinds
    { 
        private readonly Transmitter433 _transmitter433;
       private readonly RadioSymbol[] _startPattern = new[] {new RadioSymbol(4000, true), new RadioSymbol(2500, false), new RadioSymbol(1000, true)};
       private readonly RadioSymbol[] _repeatPattern = new[] {new RadioSymbol(5000, true), new RadioSymbol(2500, false), new RadioSymbol(1000, true)};
       public int BcmPin => _transmitter433.BcmPin;
       
       public Blinds(Transmitter433 transmitter433)
       {
           _transmitter433 = transmitter433;
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
           Channel1 = 1,
           Channel2,
           Channel3,
           Channel4,
           Channel5,
           Channel6,
       }

       public async Task Pair(BlindsChannel channel)
       {
           
       }
       
       public async Task Broadcast(BlindsChannel channel, BlindsCommand blindsCommand)
       {
           const string preamble = "11011101001101101";

           (string cmd, string crc) cmdBits = blindsCommand switch
           {
               BlindsCommand.Up =>    (cmd: "001110001000", crc : "1000"),
               BlindsCommand.Down =>  (cmd: "100010001000", crc : "0011"),
               BlindsCommand.Stop =>  (cmd: "101010000000", crc : "1001"),
               BlindsCommand.Open =>  (cmd: "001110000000", crc : "0100"),
               BlindsCommand.Close => (cmd: "100010000000", crc : "1011"),
               _ => throw new ArgumentOutOfRangeException(nameof(blindsCommand), blindsCommand, null)
           };

           (string channel , string crc) chBits = channel switch
           {
               BlindsChannel.Channel1 => (channel: "1000", crc:"0011"),
               BlindsChannel.Channel2 => (channel: "0100", crc:"1101"),
               BlindsChannel.Channel3 => (channel: "1100", crc:""),
               BlindsChannel.Channel4 => (channel: "0010", crc:""),
               BlindsChannel.Channel5 => (channel: "1010", crc:""),
               BlindsChannel.Channel6 => (channel: "0110", crc:"")
           };

           var bits = preamble + chBits.channel + cmdBits.cmd + chBits.crc + cmdBits.crc + "1";
#if DEBUG
           Logger.Log($"Transmitting bits: {bits}");
#endif
           var message = ToRawMessage(bits);
           await _transmitter433.Transmit(message);
       }

       private RadioSymbol [] ToRawMessage(string bits)
       {
           //TODO: Update to use new ToSymbols() method and use bits instead of strings
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

           return buffer.ToArray();
       } 
    }
}