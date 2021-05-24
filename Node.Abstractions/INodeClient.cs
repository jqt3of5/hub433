using System;
using System.Threading.Tasks;

namespace Node.Abstractions
{
    public interface INodeClient
    {
        Task SendBytes(string base64Bytes);
        Task StartListening(int timeoutSeconds);
        Task StopListening();
    }
}