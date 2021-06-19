using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using mqtt.Notification;

namespace mqtt
{
  public class TrieNode<T> : IEnumerable<T>
    {
        /// <summary>
        /// The particular part of the path this node represents
        /// </summary>
        public string TopicPart { get; init; } = string.Empty;
        /// <summary>
        /// The actual values stored at this node
        /// </summary>
       public List<T> Values { get; } = new();
       public List<TrieNode<T>> ChildNodes { get; } = new();
       
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
        private static (X, X[]) Head<X>(this IEnumerable<X> list) => (list!.First(), list!.Skip(1).ToArray());
        
        /// <summary>
        ///  
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path">The path to the resource with each level delineated by 'delimitor'. May include wildcard characters +, * or {var}</param>
        /// <param name="value"></param>
        /// <param name="delimitor"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddValue<T>(this TrieNode<T> node, string path, T value, char delimitor = '/')
        {
            var prefixes = path
                .Split(delimitor)
                .ToArray();
            
            AddValue(node,prefixes, value);
        }
        
        private static void AddValue<T>(this TrieNode<T> node, string[] pathParts, T value)
        {
            //Once we're out of path's, then we've reached the destination node, and this should have the handle
            if (pathParts.Length == 0)
            {
                node.Values.Add(value);
                return;
            }
            
            var (head, tail) = pathParts.Head();
            
            //Search through the children to find the one that matches
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.TopicPart == head)
                {
                    childNode.AddValue(tail, value);
                    return;
                }
            }
            
            //If none match, create a new child
            var child = new TrieNode<T>() {TopicPart = head};
            node.ChildNodes.Add(child);
            child.AddValue(tail, value);
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

            var (head, tail) = path.Head();
            //Search through the childnodes to find exact matches first
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.TopicPart == head)
                {
                    if (childNode.TryGetValue<T>(tail, out values))
                    {
                        return true;
                    }
                }
            }
            
            //TODO: admittedly, not my favorite placement for this method, the trie node should probalby have this. 
            bool IsWildCard(string part) => part == "*" || part == MqttClientService.Wildcard || Regex.IsMatch(part, @"\{.*\}");
            
            //If there is a wildcard route, follow that
            if (node.ChildNodes.FirstOrDefault(node => IsWildCard(node.TopicPart)) is {} wildCardRoute)
            {
                return wildCardRoute.TryGetValue(tail, out values);
            }
            
            values = Array.Empty<T>();
            return false;
        }
    } 
}