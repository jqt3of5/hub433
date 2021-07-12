using System;
using HardwareAbstractionServiceRPI.Peripherals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using Unosquare.RaspberryIO;

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
                    //TODO: Configure the client to connect to the aws IoT core service
                    //Device ID should probably come from there
                    var mqtt = MqttClientService.Create();
                    mqtt.Connect("a2z3jvbqhyj6iu-ats.iot.us-west-1.amazonaws.com").Wait();
                    
                    services.AddSingleton(mqtt);

                    services.AddSingleton(Pi.Gpio);
                    services.AddSingleton<CapabilityService>();
                    services.AddHostedService<CommunicationService>();
                });
    }
}