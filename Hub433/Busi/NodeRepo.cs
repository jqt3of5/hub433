using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Hub433
{
    public record Node
    {
        public IPAddress Ip;
        public string FriendlyName;
        public Guid Guid;
    }
    
    public class NodeRepo
    {
        List<Node> Nodes { get; } = new List<Node>();

        public Node? GetNode(Guid guid)
        {
            return Nodes.FirstOrDefault(node => node.Guid == guid);
        }
        public void AddNode(Node node)
        {
            if (!Nodes.Contains(node))
            {
               Nodes.Add(node); 
            }
        }
        public void DeleteNode(Node node)
        {
            Nodes.Remove(node);
        }
        public void DeleteNode(Guid guid)
        {
            if (Nodes.FirstOrDefault(node => node.Guid == guid) is { } node)
            {
                DeleteNode(node);
            }
        }
        
    }
}