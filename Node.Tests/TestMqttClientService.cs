using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using mqtt.Notification;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Subscribing;
using MQTTnet.Packets;
using Node.Abstractions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RPINode;

namespace Node.Tests
{
    [TestFixture]
    public class TestMqttClientService
    {
        [Test]
        public void TestSubscribe()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);
            
            mqtt.Subscribe("a/b/c", (MqttClientService.NotificationMessage message) =>
            {
                return null;
            });
            
            client.Verify(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()));
        }
        
        [Test]
        public void TestMethodRouting()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            bool didCall = false;
            mqtt.Subscribe("a/b/c", (MqttClientService.NotificationMessage message) =>
            {
                didCall = true;                 
                return null;
            }).Wait();
            

            var payload =  Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(new[] {"test"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
           
            Assert.That(didCall);
        }
        [Test]
        public void TestMethodRouting3()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            bool shouldntCall = false;
            bool didCall = false;
            mqtt.Subscribe("a/b/c",(MqttClientService.NotificationMessage message) =>
            {
                didCall = true;                 
                return null;
            }).Wait();
            mqtt.Subscribe("a/b/d", (MqttClientService.NotificationMessage message) =>
            {
                shouldntCall = true;
                return null;
            }).Wait();
            mqtt.Subscribe("a/d/c", (MqttClientService.NotificationMessage message) =>
            {
                shouldntCall = true;
                return null;
            }).Wait();

            var payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {"test"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
           
            Assert.That(didCall);
            Assert.That(!shouldntCall);
        }
        
        [Test]
        public void TestParameterList()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            bool didCall = false;
            mqtt.Subscribe("a/b/c", strings =>
            {
                didCall = true;
                Assert.That(strings.Length, Is.EqualTo(3));
                Assert.That(strings,Has.All.EqualTo("test"));
                return null;
            }).Wait();

           var payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {"test", "test", "test"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
           
            Assert.That(didCall);
        }
        public interface IMockCapability : ICapability
        {
            object MethodA(string a, string b, string c);
            object MethodB(string a, string b, int c);
            object MethodC(int c);
            object MethodD(string c);
        }
        
        [Test]
        public void TestParameterSpread()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            var mock = new Mock<IMockCapability>();

            mock.Setup(cap =>
                cap.MethodA(It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));
            
            var methodInfo = typeof(IMockCapability).GetMethod(nameof(IMockCapability.MethodA));
            Assume.That(methodInfo, Is.Not.Null);
            
            mqtt.Subscribe("a/b/c", methodInfo, mock.Object).Wait();

            var payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {"test", "test", "test"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
            
            mock.Verify(cap => 
                cap.MethodA(It.Is<string>(s => s == "test"), 
                It.Is<string>(s => s == "test"), 
                It.Is<string>(s => s == "test")));
        }
        
        [Test]
        public void TestParameterSpreadMethodB()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            var mock = new Mock<IMockCapability>();

            mock.Setup(cap =>
                cap.MethodB(It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()));
            
            var methodInfo = typeof(IMockCapability).GetMethod(nameof(IMockCapability.MethodB));
            Assume.That(methodInfo, Is.Not.Null);
            
            mqtt.Subscribe("a/b/c", methodInfo, mock.Object).Wait();

            var payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new object[] {"test", "test", "42"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
            
            mock.Verify(cap => 
                cap.MethodB(It.Is<string>(s => s == "test"), 
                    It.Is<string>(s => s == "test"), 
                    It.Is<int>(s => s == 42)));
        }
        
        [Test]
        public void TestParameterSpreadTooManyArguments()
        {
            var client = new Mock<IMqttClient>();
            
            client.Setup(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new MqttClientSubscribeResult()
            {
                Items =
                {
                    new MqttClientSubscribeResultItem(new TopicFilter(), MqttClientSubscribeResultCode.GrantedQoS0)
                }
            }));
            
            var mqtt = new MqttClientService(client.Object);

            var mock = new Mock<IMockCapability>();

            mock.Setup(cap =>
                cap.MethodB(It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()));
            
            var methodInfo = typeof(IMockCapability).GetMethod(nameof(IMockCapability.MethodB));
            Assume.That(methodInfo, Is.Not.Null);
            
            mqtt.Subscribe("a/b/c", methodInfo, mock.Object).Wait();

            var payload =  Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new object[] {"test", "test", "42", "testytest"}));
            mqtt.HandleApplicationMessageReceivedAsync(new MqttApplicationMessageReceivedEventArgs("qwerty",
                new MqttApplicationMessage() {Payload = payload, Topic = "a/b/c"}, new MqttPublishPacket(), 
                (args, token) => Task.CompletedTask)).Wait();
            
            mock.Verify(cap => 
                cap.MethodB(It.Is<string>(s => s == "test"), 
                    It.Is<string>(s => s == "test"), 
                    It.Is<int>(s => s == 42)));
        }

        [Test]
        public void TestParameterSpreadMixedTypeInJson()
        {
            //Right now, all arguments in the json array must be strings - otherwise the json parser has issues. 
            //A future improvement could make it so we can mix the types, and the converter will handle it appropriately. 
        } 
        [Test]
        public void TestParameterSpreadTooFewArguments()
        {
            //TODO: Can't test this because errors are swalloed
            //Cannot Invoke a method if we don't have all the arguments
        }
        [Test]
        public void TestParameterSpreadWrongTypeArguments()
        {
            //TODO: Can't test this because errors are swalloed
            //Cannot Invoke a method if we can't convert the arguments
        }

    }
}