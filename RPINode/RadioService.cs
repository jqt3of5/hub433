using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Node.Abstractions;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class RadioService : BackgroundService
    {
        private readonly ILogger<RadioService> _logger;

        private HubConnection SignalRConnection { get; }
        private Transmitter433 _transmitter433;
        public RadioService(ILogger<RadioService> logger, IGpioController controller)
        {
            _logger = logger;
            _transmitter433 = new Transmitter433(controller[BcmPin.Gpio17]);
            
            SignalRConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8080/nodeHub")
                .Build();
            
            SignalRConnection.Closed += async exception =>
            {
                Console.WriteLine(exception);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await SignalRConnection.StartAsync();
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SignalRConnection.On(nameof(INodeClient.SendBytes), async (string bitstring) =>
            {
               await _transmitter433.Transmit(bitstring.Select(bit => 
                    new RadioSymbol(
                        bit == '1' ? TimeSpan.FromMilliseconds(10) : TimeSpan.Zero, 
                        bit =='0' ? TimeSpan.FromMilliseconds(10) : TimeSpan.Zero
                        )
                ).ToArray());
            });
            
            SignalRConnection.On(nameof(INodeClient.StartListening), (int timeoutSeconds) =>
            {
            });
            
            SignalRConnection.On(nameof(INodeClient.StopListening), () =>
            {
            });
            
            try
            {
                await SignalRConnection.StartAsync(stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await SignalRConnection.InvokeAsync("DeviceOnline", "12345", stoppingToken);
                await Task.Delay(10000, stoppingToken);
            }

            await SignalRConnection.InvokeAsync("DeviceOffline", "12345", stoppingToken);
        }
    }
}