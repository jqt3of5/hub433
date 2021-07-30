﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Node.Abstractions
{
    public class CapabilityDescriber
    {
        public record DeviceCapabilityDescriptor (string CapabilityType, string CapabilityVersion,
            DeviceCapabilityDescriptor.PropertyDescriptor[] Properties,
            DeviceCapabilityDescriptor.ActionDescriptor[] Actions)
        {
            public record ActionDescriptor(string Name, ValueDescriptor[] Parameters, string TypeName)
            {
                [JsonIgnore] public MethodInfo Method { get; init; }
            }

            public record PropertyDescriptor(string Name, string TypeName, bool ReadOnly)
            {
                [JsonIgnore] public PropertyInfo Property { get; init; }
            }

            public record ValueDescriptor(string Name, string TypeName);
        }
         public static DeviceCapabilityDescriptor Describe(ICapability capability)
        {
            
            var actionMethods = capability.GetType().GetMethods().Where(info =>
                info.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(CapabilityActionAttribute)));
            
            var actions = new List<DeviceCapabilityDescriptor.ActionDescriptor>();
            foreach (var actionMethod in actionMethods)
            {
                var parameters = actionMethod
                    .GetParameters()
                    .Select(param =>
                        new DeviceCapabilityDescriptor.ValueDescriptor(
                            param.Name,
                            param.ParameterType.Name)
                    ).ToArray();
                
                var descriptor = new DeviceCapabilityDescriptor.ActionDescriptor(
                    actionMethod.Name,
                    parameters,
                    actionMethod.ReturnType.Name
                ){Method = actionMethod};
                
                actions.Add(descriptor);
            }
            
            var valueProperties = capability
                .GetType()
                .GetProperties()
                .Where(info =>
                    info
                        .CustomAttributes
                        .Any(attribute => 
                                attribute.AttributeType == typeof(ValueAttribute)
                            )
                    ); 
            
            var values = new List<DeviceCapabilityDescriptor.PropertyDescriptor>();
            foreach (var valueProperty in valueProperties)
            {
                var descriptor = new DeviceCapabilityDescriptor.PropertyDescriptor(
                    valueProperty.Name,
                    valueProperty.PropertyType.Name, 
                    true)
                    {Property = valueProperty};

                values.Add(descriptor); 
            }
            
            //TODO: Register events

            var capabilityAttribute = capability.GetType().GetCustomAttribute(typeof(CapabilityAttribute)) as CapabilityAttribute;
            return new DeviceCapabilityDescriptor(capabilityAttribute?.Name ?? capability.GetType().Name, capabilityAttribute?.Version ?? "0.0.0", values.ToArray(), actions.ToArray()); 
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public sealed class CapabilityAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Version;

        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public CapabilityAttribute(string name, string version)
        {
            Name = name;
            Version = version;
        }
    } 
    
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class CapabilityActionAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public CapabilityActionAttribute()
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class ValueAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public ValueAttribute()
        {
        }
    } 
    
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public EventAttribute()
        {
        }
    }  
}