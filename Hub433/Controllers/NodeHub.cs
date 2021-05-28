using System;
using System.Net;
using Hub433.Busi;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Node.Abstractions;
using Swan.Formatters;
using DeviceCapabilities = Node.Abstractions.DeviceCapabilities;

namespace Hub433.Controllers
{
    public class NodeHub : Hub
    {
        private readonly NodeRepo _repo;

        public NodeHub(NodeRepo repo)
        {
            _repo = repo;
        }
        
        public void DeviceOnline(string guid, string deviceCapabilities)
        {
            var capabilities = Json.Deserialize<DeviceCapabilities>(deviceCapabilities);
            _repo.DeviceOnline(guid, capabilities,  Context.ConnectionId);
        }

        public void DeviceOffline(string guid)
        {
            _repo.DeviceOffline(guid);
        }
        
        //called when a 433Mhz receiver receives a newbyte string
        //public void PublishBytes(string bytes)
        //{
            //???            
        //}
    }
}