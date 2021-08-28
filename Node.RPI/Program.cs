using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using Node.Abstractions;
using Unosquare.PiGpio;
using Unosquare.RaspberryIO;
using BootstrapPiGpio = Node.Hardware.BootstrapPiGpio;

namespace RPINode
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
                        
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            Pi.Init<BootstrapPiGpio>();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    //TODO: Should only use this config file when building for raspberry pi
                    config.AddJsonFile("appsettings.RaspberryPi.json", optional: false);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:8080");
                })
                .ConfigureServices(async (hostContext, services) =>
                {
                    var certificate = hostContext.Configuration["certificate"];
                    var cert = new X509Certificate2(certificate, "", X509KeyStorageFlags.Exportable);
                    
                    var privateKey = hostContext.Configuration["privateKey"];
                    string privateKeyText = File.ReadAllText(privateKey);
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(privateKeyText);
                    
                    var name = hostContext.Configuration["thingName"];
                    var host = hostContext.Configuration["mqttHost"];
                    
                    var mqtt = MqttClientService.Create();
                    mqtt.Connect(name, host, cert, rsa).Wait();
                    
                    services.AddSingleton<IMqttClientService>(mqtt);

                    services.AddSingleton(Pi.Gpio);
                    services.AddSingleton<CapabilityService>();
                    services.AddHostedService<CommunicationService>();
                });
    }
}