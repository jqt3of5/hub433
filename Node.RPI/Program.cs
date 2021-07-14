using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
                    var cert = new X509Certificate2(
                            @"ce7cd39126f2e0150f7653e43a99d0c167822de002861060628c72666b20935b-certificate.pem.crt", "", 
                            X509KeyStorageFlags.Exportable);
            
                    string privateKey = File.ReadAllText(@"ce7cd39126f2e0150f7653e43a99d0c167822de002861060628c72666b20935b-private.pem.key");

                    var rsa = RSA.Create();
                    rsa.ImportFromPem(privateKey);
                    
                    var mqtt = MqttClientService.Create();
                    mqtt.Connect("TestingThing", "a2z3jvbqhyj6iu-ats.iot.us-west-1.amazonaws.com", cert, rsa).Wait();
                    
                    services.AddSingleton<IMqttClientService>(mqtt);

                    services.AddSingleton(Pi.Gpio);
                    services.AddSingleton<CapabilityService>();
                    services.AddHostedService<CommunicationService>();
                });
    }
}