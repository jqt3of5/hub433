using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class Receiver433
    {
        private readonly BcmPin _pin;

        public Receiver433(BcmPin pin)
        {
            _pin = pin;
        }
        
        
    }
}