using System;
using System.Net;
using Hub433.Busi;
using Microsoft.AspNetCore.SignalR;

namespace Hub433.Controllers
{
    public class NodeHub : Hub<INodeClient>
    {
        private readonly NodeRepo _repo;

        public NodeHub(NodeRepo repo)
        {
            _repo = repo;
        }
        
        public void DeviceOnline(string guid)
        {
            _repo.DeviceOnline(guid, Context.ConnectionId);
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