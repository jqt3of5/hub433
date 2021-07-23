using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using Node.Abstractions;
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
                    var cert = new X509Certificate2("2d7dd678d5-certificate.pem.crt", "", X509KeyStorageFlags.Exportable);
                    
                    string privateKey = File.ReadAllText(@"2d7dd678d5-private.pem.key");
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