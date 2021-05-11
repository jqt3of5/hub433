using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;

namespace Hub433.Busi
{
    public interface INodeClient
    {
        void SendBytes(string base64Bytes);
    }
    
    public record DeviceMetadata
    {
        public string? FriendlyName { get; set; }
        public string Guid { get; set; }
        
        public bool IsConnected { get; set; }
        
        //public string OwnerId { get; set; }
        public string? NodeClientConnectionId { get; set; }
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

        public void DeviceOnline(string guid, string connectionId)
        {
            lock (_lock)
            {
                if (GetDevice(guid) is { } node)
                {
                    node.IsConnected = true;
                    node.NodeClientConnectionId = connectionId;
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
                }
            }
        }

        public void RegisterDevice(DeviceMetadata device)
        {
            lock (_lock)
            {
                UnregisterDevice(device.Guid);
                _devices.Add(device); 
            }
        }
        public void UnregisterDevice(DeviceMetadata device)
        {
            UnregisterDevice(device.Guid);
        }
        
        public void UnregisterDevice(string guid)
        {
            lock (_lock)
            {
                if (_devices.FirstOrDefault(node => node.Guid == guid) is { } node)
                {
                    UnregisterDevice(node);
                }
            }
        }
    }
}