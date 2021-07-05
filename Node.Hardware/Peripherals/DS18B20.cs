using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Node.Hardware.Peripherals
{
    /// <summary>
    /// The DS18B20 is a thermistor with a Digital Dallas OneWire interface. 
    /// </summary>
    public sealed class DS18B20 
    {
        private readonly IGpioPin _pin;

        public DS18B20(IGpioPin pin)
        {
            _pin = pin;
            _pin.PinMode = GpioPinDriveMode.Input;
            _pin.InputPullMode = GpioPinResistorPullMode.PullUp;
        }

        enum RomCommands : byte
        {
            SearchRom = 0xF0,
            ReadRom = 0x33,
            MatchRom = 0x55,
            SkipRom = 0xCC,
            AlarmSearch = 0xEC,
        }

        enum FunctionCommands : byte
        {
            ConvertT = 0x44,
            WriteScratchPad = 0x4E,
            ReadScratchPad = 0xBE,
            CopyScratchPad = 0x48,
            RecallE2 = 0xB8,
            ReadPowerSupply = 0xB4,
        }

        class ScratchPadMemory
        {
            private byte[] _memory;

            public ScratchPadMemory(byte[] memory)
            {
                _memory = memory;
            }

            public Int16 TempRaw => (Int16) (_memory[0] | _memory[1] << 8);

            public float Temp
            {
                get
                {
                    switch (Configuration)
                    {
                        case Config.Resolution9Bit:
                            return TempRaw * 0.5f;
                        case Config.Resolution10Bit:
                            return TempRaw * .25f;
                        case Config.Resolution11Bit:
                            return TempRaw * .125f;
                        default:
                        case Config.Resolution12Bit:
                            return TempRaw * .0625f;
                    }
                }
            }

            public byte AlarmHigh
            {
                get => _memory[2];
                set => _memory[2] = value;
            }

            [Flags]
            public enum Config : byte
            {
                Resolution9Bit = 0b00011111,
                Resolution10Bit = 0b00111111,
                Resolution11Bit = 0b01011111,
                Resolution12Bit = 0b01111111,
            }

            public Config Configuration
            {
                get => (Config) _memory[4];
                set => _memory[4] = (byte) value;
            }

            public byte CRC
            {
                get => _memory[8];
                set => _memory[8] = value;
            }

            private int CRCMask = 0b100110001;

            public bool IsCRCValid()
            {
                //TODO:
                return true;
            }
        }
       
        bool Initialize()
        {
            _pin.PinMode = GpioPinDriveMode.Input;
            if (_pin.Read() == false)
            {
                Console.WriteLine("The bus might be active!");
                return false;
            }

            _pin.PinMode = GpioPinDriveMode.Output;
            //Resetpulse
            _pin.Write(GpioPinValue.Low);
            Pi.Timing.SleepMicroseconds(480);
            _pin.PinMode = GpioPinDriveMode.Input;
            //Wait for Presence Pulse 
            if (!_pin.WaitForValue(GpioPinValue.Low, 1))
            {
                Console.WriteLine("The DS18b20 never responded");
                return false;
            }

            Pi.Timing.SleepMicroseconds(480);

            //Wait for device to release bus
            _pin.WaitForValue(GpioPinValue.High, 1);
            return true;
        }

        void WriteByte(byte b)
        {
            for (int i = 0; i < 8; ++i)
            {
                //Write Timeslot
                _pin.PinMode = GpioPinDriveMode.Output;
                _pin.Write(GpioPinValue.Low);
                //No longer than 15 us
                Pi.Timing.SleepMicroseconds(2);
                //Write bit    
                _pin.Write((b & (1 << i)) != 0);
                Pi.Timing.SleepMicroseconds(60);
                //Release the bus
                _pin.PinMode = GpioPinDriveMode.Input;
                Pi.Timing.SleepMicroseconds(1);
            }
        }

        bool ReadByte(out byte b)
        {
            byte a = 0;
            for (int i = 0; i < 8; ++i)
            {
                _pin.PinMode = GpioPinDriveMode.Output;
                _pin.Write(GpioPinValue.Low);
                //Minimum 1us
                Pi.Timing.SleepMicroseconds(1);
                //Release the bus
                _pin.PinMode = GpioPinDriveMode.Input;
                // Pi.Timing.SleepMicroseconds(5);
                //Within 15us of start of read slot
                //Only need to set high values
                if (_pin.Read())
                {
                    a = (byte) (a | (1 << i));
                }

                Pi.Timing.SleepMicroseconds(60);
                if (!_pin.WaitForValue(GpioPinValue.High, 1))
                {
                    b = 0;
                    return false;
                }

                Pi.Timing.SleepMicroseconds(1);
            }

            b = a;
            return true;
        }

        bool TryReadConfig([MaybeNullWhen(false)] out ScratchPadMemory mem)
        {
            //Read Config
            if (Initialize())
            {
                WriteByte((byte) RomCommands.SkipRom);
                WriteByte((byte) FunctionCommands.ReadScratchPad);
                var bytes = new byte[9];
                for (int i = 0; i < 9; ++i)
                {
                    ReadByte(out bytes[i]);
                }

                mem = new ScratchPadMemory(bytes);

#if DEBUG
                Console.WriteLine($"Config: {mem.Configuration} Temp: {mem.Temp} IsValid: {mem.IsCRCValid()} ");
#endif

                return mem.IsCRCValid();
            }

            mem = default;
            return false;
        }

        bool TryWriteConfig(ScratchPadMemory.Config config, byte alarmHigh, byte alarmLow)
        {
            //Write config
            if (Initialize())
            {
                WriteByte((byte) RomCommands.SkipRom);
                WriteByte((byte) FunctionCommands.WriteScratchPad);
                WriteByte(alarmHigh);
                WriteByte(alarmLow);
                WriteByte((byte) config);
            }

            //TODO: Should check that mem is equal to the values set. 
            TryReadConfig(out var mem);

            //Save Config
            if (Initialize())
            {
                WriteByte((byte) RomCommands.SkipRom);
                WriteByte((byte) FunctionCommands.CopyScratchPad);
            }

            return true;
        }

        bool StartConversion()
        {
            if (Initialize())
            {
                WriteByte((byte) RomCommands.SkipRom);
                WriteByte((byte) FunctionCommands.ConvertT);
                //Loops till conversion has completed
                _pin.PinMode = GpioPinDriveMode.Input;
                while (_pin.Read() == false) ;
            }
            return true;
        }

        public Task<float?> GetNextValue()
        {
            lock (this)
            {
                if (StartConversion())
                {
                    if (TryReadConfig(out var mem))
                    {
                        return Task.FromResult((float?)mem.Temp);
                    }
                } 
            }

            return null;
        }
    }
}