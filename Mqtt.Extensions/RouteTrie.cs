using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using mqtt.Notification;
using Pipelines.Notification;

namespace mqtt
{
  public class TrieNode<T> : IEnumerable<T>
    {
        public string TopicLevel { get; init; } = string.Empty;
       public List<T> Values { get; } = new List<T>();
       public List<TrieNode<T>> ChildNodes { get; } = new List<TrieNode<T>>();
       
       public IEnumerator<T> GetEnumerator()
       {
           foreach (var nodeValue in Values)
           {
               yield return nodeValue;
           }

           foreach (var child in ChildNodes)
           {
               foreach (var childNodeValue in child)
               {
                   yield return childNodeValue;
               }
           }
       }

       IEnumerator IEnumerable.GetEnumerator()
       {
           return GetEnumerator();
       }
    }

    public static class TrieExtensions 
    {
        
        public static void AddValue<T>(this TrieNode<T> node, string path, T value)
        {
            var prefixes = path
                .Split('/')
                .ToArray();
            AddValue(node,prefixes, value);
            
        }
        private static void AddValue<T>(this TrieNode<T> node, string[] path, T value)
        {
            //Once we're out of path's, then we're at a leaf node, and this should have the handle
            if (path.Length == 0)
            {
                node.Values.Add(value);
                return;
            }
            //Search through the children to find the one that matches
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.TopicLevel == path.First())
                {
                    childNode.AddValue(path.Skip(1).ToArray(), value);
                    return;
                }
            }
            
            //If none match, create a new child
            var child = new TrieNode<T>() {TopicLevel = path.First()};
            node.ChildNodes.Add(child);
            child.AddValue(path.Skip(1).ToArray(), value);
        }

        public static bool TryGetValue<T>(this TrieNode<T> node, string path, out T[] values)
        {
            var prefixes = path
                .Split('/')
                .ToArray();
            return TryGetValue(node, prefixes, out values); 
        }
        public static bool TryGetValue<T>(this TrieNode<T> node, string[] path, out T[] values)
        {
            if (path.Length == 0)
            {
                values = node.Values.ToArray();
                return true;
            }
            
            //Search through the childnodes to find exact matches first
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.TopicLevel == path.First())
                {
                    if (childNode.TryGetValue<T>(path.Skip(1).ToArray(), out values))
                    {
                        return true;
                    }
                }
            }

            //If there is a wildcard route, follow that
            var wildcardRoute = node.ChildNodes.FirstOrDefault(node => node.TopicLevel == MqttClientService.Wildcard);

            if (wildcardRoute != null)
            {
                return wildcardRoute.TryGetValue<T>(path.Skip(1).ToArray(), out values);
            }
            
            values = Array.Empty<T>();
            return false;
        }
    } 
}