using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Server;

namespace Hub433.Controllers
{
    public class MqttHostedService : IHostedService
    {
        private IMqttServer _mqttServer;

        public MqttHostedService()
        {
           _mqttServer = new MqttFactory().CreateMqttServer();
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
           var options = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
           return _mqttServer.StartAsync(options); 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _mqttServer.StopAsync();
        }
    }
}