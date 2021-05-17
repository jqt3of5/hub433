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

namespace RPINode
{
    public class RadioService : BackgroundService
    {
        private readonly ILogger<RadioService> _logger;

        private HubConnection SignalRConnection { get; }
        public RadioService(ILogger<RadioService> logger)
        {
            _logger = logger;
            SignalRConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/nodeHub")
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
            SignalRConnection.On(nameof(INodeClient.SendBytes), (string base64Bytes) =>
            {
                var bytes = Convert.FromBase64String(base64Bytes);
                
                //TODO: Write bytes
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
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}