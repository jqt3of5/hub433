using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardwareAbstractionServiceRPI.Peripherals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using MQTTnet.Client;
using Node.Abstractions;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Pi.Init<BootstrapMock>();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(async (hostContext, services) =>
                {
                    var mqtt = MqttClientService.Create(deviceId: RadioService.DEVICE_ID);
                    mqtt.Connect("localhost").Wait();
                    services.AddSingleton(mqtt);
                    
                    services.AddSingleton(Pi.Gpio);
                    services.AddHostedService<RadioService>();
                    services.AddSingleton<CapabilityService>();
                    services.AddSingleton<InternalNodeHubApi>();
                });
    }
}