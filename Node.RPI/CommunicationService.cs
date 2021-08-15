﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using Node.Abstractions;

namespace RPINode
{
    public class CommunicationService : BackgroundService 
    {
        private readonly IMqttClientService _mqttClientService;
        private readonly IConfiguration _configuration;
        private readonly CapabilityService _capabilityService;

        public CommunicationService(IConfiguration configuration, IMqttClientService mqttClientService, CapabilityService capabilityService)
        {
            _configuration = configuration;
            _capabilityService = capabilityService;
            _mqttClientService = mqttClientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqttClientService.Publish($"$aws/things/{_mqttClientService.ThingName}/shadow/get", string.Empty);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var version = _configuration["version"];
                var doc = new ShadowUpdate()
                {
                    state = new ShadowUpdate.ShadowUpdateState()
                    {
                        reported = new ShadowState()
                        {
                            connected = true,
                            version = version
                        }
                    }
                };
                
                await _mqttClientService.Publish($"$aws/things/{_mqttClientService.ThingName}/shadow/update", 
                    JsonSerializer.Serialize(doc, 
                        new JsonSerializerOptions()
                        {
                            //Mostly to ignore the timestamp, and version properties which don't need to be written
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        }));
                await Task.Delay(10000, stoppingToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mqttClientService.Subscribe($"capability/{_mqttClientService.ThingName}", HandleCapabilityRequest);
            
            await _mqttClientService.Subscribe(
                $"$aws/things/{_mqttClientService.ThingName}/shadow/update/delta",
                HandleShadowDelta);
            
            await _mqttClientService.Subscribe(
                $"$aws/things/{_mqttClientService.ThingName}/shadow/update/documents",
                HandleShadowDocument);

            await _mqttClientService.Subscribe(
                $"$aws/things/{_mqttClientService.ThingName}/shadow/get/accepted", 
                HandleGetShadow);
            
            await _mqttClientService.Subscribe(
                $"$aws/things/{_mqttClientService.ThingName}/tunnel/notify", 
                HandleTunnelNotify); 
            
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var d = new ShadowUpdate()
            {
                state = new ShadowUpdate.ShadowUpdateState()
                {
                    reported = new ShadowState()
                    {
                        connected = false,
                    }
                }
            };
                
            await _mqttClientService.Publish($"$aws/things/{_mqttClientService.ThingName}/shadow/update", 
                JsonSerializer.Serialize(d, 
                    new JsonSerializerOptions()
                    {
                        //Mostly to ignore the timestamp, and version properties which don't need to be written
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    })); 
            
            await _mqttClientService.Unsubscribe($"capability/{_mqttClientService.ThingName}");
            await _mqttClientService.Unsubscribe(
                $"$aws/things/{_mqttClientService.ThingName}/shadow/update/delta");
            
            await _mqttClientService.Unsubscribe(
                $"$aws/things/{_mqttClientService.ThingName}/shadow/update/documents");

            await base.StopAsync(cancellationToken);
        }

        public class StartTunnelRequest
        {
            public string clientAccessToken { get; set; }
            public string clientMode { get; set; }
            public string region { get; set; }
            public string [] services { get; set; } 
        }
        
        public class ShadowState
        {
            public string? version { get; set; }
            public bool? connected { get; set; }
            public Dictionary<string, JsonElement> capabilities { get; set; }
        }
        
        public class ShadowDelta
        {
            public int version { get; set; }
            public int timestamp { get; set; } 
            public ShadowState state { get; set; }
            public Dictionary<string, object> metadata { get; set; }
        }
        
        public class ShadowUpdate 
        {
            public int? version { get; set; } 
            public int? timestamp { get; set; }
            public class ShadowUpdateState
            {
                public ShadowState reported { get; set; }
                public ShadowState desired { get; set; }
            }
            public class ShadowUpdateMetadata
            {
                public Dictionary<string, object> reported { get; set; }
                public Dictionary<string, object> desired { get; set; }
            } 
            
            public ShadowUpdateState state { get; set; }
            public ShadowUpdateMetadata metadata { get; set; }
        }

        private async Task HandleTunnelNotify(MqttClientService.NotificationMessage message)
        {
            var request = message.GetPayload<StartTunnelRequest>();

            if (request.clientMode != "destination")
            {
                return;
            }

            if (request.services.Length > 1)
            {
                return;
            }

            if (request.services.First() != "SSH")
            {
                return;
            }
            
            Logger.Log("Attempting to start tunnel");
            // Start the destination local proxy in a separate process to connect to the SSH Daemon listening port 22
            //final ProcessBuilder pb = new ProcessBuilder("localproxy",
            //    "-t", accessToken,
            //    "-r", region,
            //    "-d", "localhost:22"); 
            Logger.Log("Starting Tunnel failed - not implemented");

        }
        /// <summary>
        /// the handler for getting the current shadow. Should only be called once after the device connects to the server
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task HandleGetShadow(MqttClientService.NotificationMessage message)
        {
            //Handle this message only once after startup
            await _mqttClientService.Unsubscribe($"$aws/things/{_mqttClientService.ThingName}/shadow/get/accepted");
            
            var doc = message.GetPayload<ShadowUpdate>();

            if (doc?.state?.desired?.capabilities == null)
            {
                Logger.Log("No capabilities to update in HandleGetShadow");
                return;
            }
            
            foreach (var request in doc.state.desired.capabilities)
            {
                Logger.Log($"Updating capability state: {request.Key} with body: \n {request.Value.GetRawText()}");
                await _capabilityService.UpdateCapabilityState(request.Key, request.Value);
            }
            
            doc.version = null;
            doc.timestamp = null;
            doc.state.reported = doc.state.desired;

            await _mqttClientService.Publish($"$aws/things/{_mqttClientService.ThingName}/shadow/update", 
                JsonSerializer.Serialize(doc, 
                     new JsonSerializerOptions()
                    {
                        //Mostly to ignore the timestamp, and version properties which don't need to be written
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    }));
        } 
        
        private async Task HandleShadowDelta(MqttClientService.NotificationMessage message)
        {
            var delta = message.GetPayload<ShadowDelta>();
           
            foreach (var request in delta.state.capabilities)
            {
                Logger.Log($"Updating capability state: {request.Key} with body: \n {request.Value.GetRawText()}");
                await _capabilityService.UpdateCapabilityState(request.Key, request.Value);
            }

            //Our state should be updated by now, publish back to aws
            await _mqttClientService.Publish($"$aws/things/{_mqttClientService.ThingName}/shadow/update",
                JsonSerializer.Serialize(new ShadowUpdate()
                {
                    state = new ShadowUpdate.ShadowUpdateState()
                    {
                        reported = delta.state,
                        //I thought this didn't need to be sent too, but it seems to be missing if we don't send it up. 
                        desired = delta.state
                    }
                }, new JsonSerializerOptions()
                {
                    //Mostly to ignore the timestamp, and version properties which don't need to be written
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }));
        }

        private async Task HandleShadowDocument(MqttClientService.NotificationMessage message)
        {
            Console.WriteLine("ShadowDocument");
        }
        
        private async Task HandleCapabilityRequest(MqttClientService.NotificationMessage message)
        {
            try
            {
                var capabilityRequest = message.GetPayload<DeviceCapabilityActionRequest>();
                Logger.Log($"Invoking capability action: {capabilityRequest.CapabilityType}.{capabilityRequest.CapabilityAction}");
                var response = await _capabilityService.InvokeCapabilityAction(capabilityRequest);
                
                if (!string.IsNullOrEmpty(message.InternalMessage.ResponseTopic))
                {
                    await _mqttClientService.Publish(message.InternalMessage.ResponseTopic,
                        JsonSerializer.Serialize(response));
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                await _mqttClientService.Publish($"error/{_mqttClientService.ThingName}", e.ToString());
            } 
        }
    }
}