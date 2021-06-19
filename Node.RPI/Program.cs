using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardwareAbstractionServiceRPI.Peripherals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using MQTTnet.Client;
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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(Pi.Gpio);
                    services.AddSingleton(Board.Pins);
                    services.AddHostedService<RadioService>();
                    
                    var mqtt = MqttClientService.Create(deviceId: "qwerty");
                    mqtt.Connect("localhost");
                    services.AddSingleton(mqtt);
                    
                });
    }
}