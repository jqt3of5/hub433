using Microsoft.VisualBasic;
using Node.Abstractions;

namespace RPINode
{
    public class Blinds
    {
        private readonly Transmitter433 _transmitter433;

        public Blinds(string capabilityId, Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        public void Open()
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