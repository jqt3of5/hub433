using System.Threading;
using System.Threading.Tasks;
using Moq;
using mqtt.Notification;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Subscribing;
using NUnit.Framework;
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
            
            var mqtt = new MqttClientService("qwerty", client.Object);
            
            mqtt.Subscribe("a/b/c", message =>
            {
                return null;
            });
            
            client.Verify(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()));
        }
        
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
            
            var mqtt = new MqttClientService("qwerty", client.Object);
            
            mqtt.Subscribe("a/b/c", message =>
            {
                return null;
            });
            
            client.Verify(client => client.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()));
        }
    }
}