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
using static Node.Abstractions.DeviceCapability;

namespace RPINode
{
    public class RadioService : BackgroundService
    {
        private readonly ILogger<RadioService> _logger;
        private readonly InternalNodeHubApi _hubApi;
        private Transmitter433 _transmitter433;
        private Receiver433 _receiver433;

        private readonly DeviceCapability[] Capabilities;
        public RadioService(ILogger<RadioService> logger, IGpioController controller, InternalNodeHubApi hubApi)
        {
            _logger = logger;
            _hubApi = hubApi;
            _transmitter433 = new Transmitter433(controller[BcmPin.Gpio17]);
            _receiver433 = new Receiver433(controller[BcmPin.Gpio23]);
            
            var transmitterCapability = new DeviceCapability(
                "Transmitter17",
                nameof(Transmitter433),
                new ValueDescriptor[] { },
                new[]
                {
                    new ActionDescriptor(
                        "Transmit",
                        new[] {new ValueDescriptor("bitString", ValueDescriptor.TypeEnum.String) }, 
                        ValueDescriptor.TypeEnum.Void),
                });
            
            var blindsCapability = new DeviceCapability(
                "Blinds17",
                nameof(Blinds),
                new ValueDescriptor[] { },
                new[]
                {
                    new ActionDescriptor(
                        "Open",
                        new ValueDescriptor[] { }, 
                        ValueDescriptor.TypeEnum.Void),
                    new ActionDescriptor(
                        "Close",
                        new ValueDescriptor[] { }, 
                        ValueDescriptor.TypeEnum.Void),
                    new ActionDescriptor(
                        "Stop",
                        new ValueDescriptor[] { }, 
                        ValueDescriptor.TypeEnum.Void)
                });
            Capabilities = new[] {transmitterCapability, blindsCapability};
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _hubApi.DeviceOnline("MyOnlyDevice", Capabilities);
                await Task.Delay(10000, stoppingToken);
            }

            await _hubApi.DeviceOffline("MyOnlyDevice");
        }
    }
}