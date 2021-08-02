﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;
using RPINode.Capability;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class CapabilityService
    {
        private readonly Transmitter433 _transmitter433;
        private readonly Receiver433 _receiver433;
        private readonly DhtCore _dht;

        private List<(string Name, ICapability capability)> _capabilities= new();
        public CapabilityService(IGpioController pins)
        {
            //Create Connected Peripheral Instances
            _transmitter433 = new Transmitter433(pins[BcmPin.Gpio17]);
            _receiver433 = new Receiver433(pins[BcmPin.Gpio23]);
            // _dht = new DhtCore(pins[BcmPin.Gpio22]);

            //Register the available capabilities 
            RegisterCapability(new BlindsCapability(_transmitter433));
            RegisterCapability(new RemoteRelayCapability(_transmitter433));
            RegisterCapability(new ConnectedRelayCapability(
                new Relay(pins[BcmPin.Gpio18]
                )));
        }

        public bool RegisterCapability<T>(T capability) where T : ICapability
        {
            var attribute = (CapabilityAttribute)typeof(T).GetCustomAttribute(typeof(CapabilityAttribute));
            if (attribute == null)
            {
                return false;
            }

            _capabilities.Add((attribute.Name, capability));
            return true;
        }

        public async Task<object?> InvokeCapabilityAction(DeviceCapabilityActionRequest request)
        {
            var capability = _capabilities.FirstOrDefault(cap=> cap.Name == request.CapabilityType);
            if (capability == default)
            {
                return null;
            }
            
            //Next find the method to perform the action
            var methodInfo = capability.capability.GetType().GetMethod(request.CapabilityAction);
            if (methodInfo != null)
            {
                //Create the capability handler
                var result = methodInfo.Invoke(capability.capability, new object?[] {request});
                
                if (result is Task<object?> taskReturn)
                {
                    return await taskReturn;
                }
                if (result is Task task)
                {
                    await task;
                    return Task.FromResult<object?>(null);
                }
        
                return Task.FromResult(result);  
            }

            return Task.FromResult<object?>(null);
        }
        public async Task<object?> InvokeCapability(string capabilityName, JsonElement actionRequest)
        {
            //First find the matching capabilty
            var capability = _capabilities.FirstOrDefault(cap=> cap.Name == capabilityName);
            if (capability == default)
            {
                return null;
            }

            //TODO: it would technically be possible to deserialize the payload as a specific type and pass that as an argument instead of the whole request
            return await capability.capability.Invoke(actionRequest);
        }
        
        private object? InvokeWithMappedParameters(string [] stringArguments, MethodInfo method, object instance)
        {
            //TODO: This can be improved in a couple of ways....
            //since this is deserializing as a string array, each element must be a string in the JSON. Then we type convert later
            //If we deserialized as an object array, we would get JsonElements, which we can then convert/type check into the method parameters. 
            //allowing a bit more flexibility in the JSON format
            //OR we can convert to a dictionary and mapped named parameters.  
            var parameters = method.GetParameters();
            if (parameters.Length > stringArguments.Length)
            {
                //we cannot invoke the method if we don't have enough arguments
                return null;
            }

            List<object> arguments = new();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var argument = TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFromString(stringArguments[i]);
                arguments.Add(argument);
            }

            return method.Invoke(instance, arguments.ToArray());
        }

    }
}