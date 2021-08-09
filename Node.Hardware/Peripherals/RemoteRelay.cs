using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Swan;

namespace Node.Hardware.Peripherals
{
    public class RemoteRelay
    {
       readonly RadioSymbol[] _startPattern = new[] {new RadioSymbol(4000, true), new RadioSymbol(2500, false), new RadioSymbol(1000, true)};
       private const byte StartData = 0xAA;
        enum RelayCommand
        {
            Set = 0xC4, 
            Pair = 0x9D, 
        }

        private readonly List<byte> _transmitterId = new() {0x12, 0x34, 0x56, 0x78};
        private readonly Transmitter433 _transmitter433;
       
       public RemoteRelay(Transmitter433 transmitter433)
       {
           _transmitter433 = transmitter433;
       }

       public async Task Pair(int channel)
       {
           //Data Star
           List<byte> message = new List<byte>(){StartData};

           //Add the transmitter id - 32 bits + 8bits
           message.AddRange(_transmitterId);
           message.Add(_transmitterId.crc8());
           
           //Assemble the channel package - 8 bits + 8bits
           var b = (byte) channel;
           message.Add(b);
           message.Add(b.crc8()); 
           
           message.Add((byte)RelayCommand.Pair);
           message.Add(((byte)RelayCommand.Pair).crc8());
           
#if DEBUG
           foreach (var b1 in message)
           {
               Console.Write($"{b1:X} ");
           } 
#endif
           
           //Long signals to indicate we're about to transmit
           var radioMessage = _startPattern.Concat(message.ToSymbols()).ToArray();

           await _transmitter433.Transmit(radioMessage);
       }

       public async Task Pwm(int channel, (int port, float dutyCycle, float cyclesPerSecond) [] ports)
       {
           //Data Start
           List<byte> message = new List<byte>(StartData);

           //Add the transmitter id - 32 bits + 8 crc
           message.AddRange(_transmitterId);
           message.Add(_transmitterId.crc8());
           
           //Assemble the channel package - 8 bits + 8 crc
           var b = (byte) channel;
           message.Add(b);
           message.Add(b.crc8());
           message.Add((byte)RelayCommand.Set);
           message.Add(((byte)RelayCommand.Set).crc8());
           
           foreach (var value in ports)
           {
               //Assemble the port state, 56 bits + 8 crc
               var p = (byte) value.port;
               var portBytes = new List<byte>(){p};
               
               portBytes.Add(p);

               var dutyCycle = (ushort)(ushort.MaxValue * value.dutyCycle);
               portBytes.AddRange(BitConverter.GetBytes(dutyCycle));
               
               portBytes.AddRange(BitConverter.GetBytes(value.cyclesPerSecond));

               message.AddRange(portBytes);
               message.Add(portBytes.crc8());
           }
           
#if DEBUG
           foreach (var b1 in message)
           {
               Console.Write($"{b1:X} ");
           } 
#endif
           
           //Long signals to indicate we're about to transmit
           var radioMessage = _startPattern.Concat(message.ToSymbols()).ToArray();

           await _transmitter433.Transmit(radioMessage);
       }
       
       public Task On(int channel, int [] ports)
       {
           return Pwm(channel, ports.Select(p => (port: p, dutyCycle: 1f, cyclesPerSecond: 1024f)).ToArray());
       }

       public Task Off(int channel, int [] ports)
       {
           return Pwm(channel, ports.Select(p => (port: p, dutyCycle: 0f, cyclesPerSecond: 1024f)).ToArray());
       }
    }
}