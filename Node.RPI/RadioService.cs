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
using RPINode.Capabilities;
using RPINode.Peripherals;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;
using static Node.Abstractions.DeviceCapabilityDescriptor;

namespace RPINode
{
    public class RadioService : BackgroundService
    {
        public const string DEVICE_ID = "MyOnlyDevice";
        private readonly ILogger<RadioService> _logger;
        private readonly InternalNodeHubApi _hubApi;
        private readonly CapabilityService _capabilityService;
        private Transmitter433 _transmitter433;
        private Receiver433 _receiver433;
        private BlindsCapability _blinds; 
        public RadioService(ILogger<RadioService> logger, IGpioController pins, InternalNodeHubApi hubApi, CapabilityService capabilityService)
        {
            _logger = logger;
            _hubApi = hubApi;
            _capabilityService = capabilityService;
            
            _transmitter433 = new Transmitter433(pins[BcmPin.Gpio17]);
            _receiver433 = new Receiver433(pins[BcmPin.Gpio23]);
            _blinds = new BlindsCapability(new BlindsDevice(_transmitter433));
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var descriptor = await _capabilityService.RegisterCapabilities(_blinds).ToArrayAsync(stoppingToken); //, _receiver433, _transmitter433);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await _hubApi.DeviceOnline(DEVICE_ID, descriptor);
                await Task.Delay(10000, stoppingToken);
            }

            //TODO: Unregister device
            await _hubApi.DeviceOffline(DEVICE_ID);
        }
    }
}