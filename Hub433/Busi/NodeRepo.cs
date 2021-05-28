using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using Node.Abstractions;

namespace Hub433.Busi
{
    public record DeviceMetadata(string Guid)
    {
        //Probably better to store this as a separate table?
        public string? Owner { get; set; }
        public string? FriendlyName { get; set; }
        public bool IsConnected { get; set; }
        
        //public string OwnerId { get; set; }
        public string? NodeClientConnectionId { get; set; }
        public DateTime? LastMessageRecieved { get; set; }
        
        public DeviceCapabilities? Capabilities { get; set; }
    }

    public class NodeRepo
    {
        private object _lock = new object();
        List<DeviceMetadata> _devices  = new();
        public IReadOnlyList<DeviceMetadata> Devices => _devices;

        public void UpdateFriendlyName(string guid, string friendlyname)
        {
            lock (_lock)
            {
                if (GetDevice(guid) is { } node)
                {
                    node.FriendlyName = friendlyname;
                } 
            }
        }
        public bool DoesNodeExist(string guid)
        {
            lock (_lock)
            {
                return _devices.Any(node => node.Guid == guid);
            }
        }
        public DeviceMetadata? GetDevice(string guid)
        {
            lock (_lock)
            {
                return _devices.FirstOrDefault(node => node.Guid == guid);
            }
        }

        public void DeviceOnline(string nodeGuid, DeviceCapabilities capabilities, string connectionId)
        {
            lock (_lock)
            {
                if (GetDevice(nodeGuid) is { } node)
                {
                    node.IsConnected = true;
                    node.NodeClientConnectionId = connectionId;
                    node.Capabilities = capabilities;
                    node.LastMessageRecieved = DateTime.Now;
                }
                else
                {
                    _devices.Add(new DeviceMetadata(nodeGuid){IsConnected = true, Capabilities = capabilities, NodeClientConnectionId = connectionId}); 
                }
            }
        }
        public void DeviceOffline(string guid)
        {
            lock (_lock)
            {
                if (GetDevice(guid) is { } node)
                {
                    node.IsConnected = false;
                    node.LastMessageRecieved = DateTime.Now;
                }
            }
        }

        public void ClaimDevice(string nodeGuid, string userId)
        {
            lock (_lock)
            {
                if (GetDevice(nodeGuid) is { } node)
                {
                    _devices.Remove(node);
                    _devices.Add(node with {Owner = userId});
                }
                else
                {
                    _devices.Add(new DeviceMetadata(nodeGuid){IsConnected = false, Owner = userId}); 
                }
            }
        }
    }
}