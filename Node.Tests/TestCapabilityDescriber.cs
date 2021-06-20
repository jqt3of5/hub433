using System;
using System.Linq;
using Moq;
using mqtt.Notification;
using Node.Abstractions;
using NUnit.Framework;
using RPINode;

namespace Node.Tests
{
    [TestFixture]
    public class TestCapabilityDescriber
    {
        public interface IPropertyMockCapability : ICapability
        {
            [Value]
            public string PropertyString { get; }
            [Value]
            public int PropertyInt{ get; }
        }
        public interface IMixedMockCapability : ICapability
        {
            [Action]
            public void ActionNoParams();
            [Action]
            public void ActionStringParam(string a); 
            [Action]
            public void ActionMixedParam(string a, int b);
            
            [Value]
            public string PropertyString { get; }
            [Value]
            public int PropertyInt{ get; }
        }
        public interface IActionMockCapability : ICapability
        {
            [Action]
            public void ActionNoParams();
            [Action]
            public void ActionStringParam(string a);
            [Action]
            public void ActionIntParam(int a);
            [Action]
            public void ActionMixedParam(string a, int b);
            [Action]
            public string FuncStringReturn();
            [Action]
            public string FuncMixed(string a, int b);
        }
        public interface IActionAMockCapability : ICapability
        {
            [Action]
            void Foo();
        }
        public interface IActionBMockCapability : ICapability
        {
            [Action]
            string Foo();
        }
        public interface IActionCMockCapability : ICapability
        {
            [Action]
            void Foo(string a);
        } 
        public interface IActionDMockCapability : ICapability
        {
            [Action]
            string Foo(string a);
        } 
        [Test]
        public void TestEmptyCapabilityDescriber()
        {
            var capability = new Mock<ICapability>();
            capability.SetupGet(capability => capability.CapabilityId).Returns("MyCapability");
            capability.SetupGet(capability => capability.CapabilityTypeId).Returns("MyCapabilityType");
            
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assert.That(descriptor.CapabilityId, Is.EqualTo("MyCapability")); 
            Assert.That(descriptor.CapabilityTypeId, Is.EqualTo("MyCapabilityType")); 
            Assert.That(descriptor.Actions, Is.Empty);
            Assert.That(descriptor.Properties, Is.Empty);
        }
        [Test]
        public void TestActionCapabilityDescriber()
        {
            var capability = new Mock<IActionMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            
            Assert.That(descriptor.Actions, Has.Length.EqualTo(6));
            Assert.That(descriptor.Properties, Is.Empty);
        }
        [Test]
        public void TestMixedCapabilityDescriber()
        {
            var capability = new Mock<IMixedMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assert.That(descriptor.Actions, Has.Length.EqualTo(3));
            Assert.That(descriptor.Properties, Has.Length.EqualTo(2));
        }
        [Test]
        public void TestPropertyCapabilityDescriber()
        {
            var capability = new Mock<IPropertyMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assert.That(descriptor.Actions, Is.Empty);
            Assert.That(descriptor.Properties, Has.Length.EqualTo(2));
        }
        
        [Test]
        public void TestCapabilityATypesDescriber()
        {
            var capability = new Mock<IActionAMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assume.That(descriptor.Actions, Has.Length.EqualTo(1));
            
            Assert.That(descriptor.Actions.First().Method, Is.Not.Null);
            Assert.That(descriptor.Actions.First().Name, Is.EqualTo(nameof(IActionAMockCapability.Foo)));
            Assert.That(descriptor.Actions.First().TypeName, Is.EqualTo("Void"));
            Assert.That(descriptor.Actions.First().Parameters, Is.Empty);
        }
        [Test]
        public void TestCapabilityBTypesDescriber()
        {
            var capability = new Mock<IActionBMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assume.That(descriptor.Actions, Has.Length.EqualTo(1));
            
            Assert.That(descriptor.Actions.First().Method, Is.Not.Null);
            Assert.That(descriptor.Actions.First().Name, Is.EqualTo(nameof(IActionBMockCapability.Foo)));
            Assert.That(descriptor.Actions.First().TypeName, Is.EqualTo("String"));
            Assert.That(descriptor.Actions.First().Parameters, Is.Empty);
        }
        [Test]
        public void TestCapabilityCTypesDescriber()
        {
            var capability = new Mock<IActionCMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assume.That(descriptor.Actions, Has.Length.EqualTo(1));
            
            Assert.That(descriptor.Actions.First().Method, Is.Not.Null);
            Assert.That(descriptor.Actions.First().Name, Is.EqualTo(nameof(IActionCMockCapability.Foo)));
            Assert.That(descriptor.Actions.First().TypeName, Is.EqualTo("Void"));
            Assert.That(descriptor.Actions.First().Parameters, Has.Length.EqualTo(1));
            Assert.That(descriptor.Actions.First().Parameters.First().Name, Is.EqualTo("a"));
            Assert.That(descriptor.Actions.First().Parameters.First().TypeName, Is.EqualTo("String"));
        }
        [Test]
        public void TestCapabilityDTypesDescriber()
        {
            var capability = new Mock<IActionDMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assume.That(descriptor.Actions, Has.Length.EqualTo(1));
            
            Assert.That(descriptor.Actions.First().Method, Is.Not.Null);
            Assert.That(descriptor.Actions.First().Name, Is.EqualTo(nameof(IActionDMockCapability.Foo)));
            Assert.That(descriptor.Actions.First().TypeName, Is.EqualTo("String"));
            Assert.That(descriptor.Actions.First().Parameters, Has.Length.EqualTo(1));
            Assert.That(descriptor.Actions.First().Parameters.First().Name, Is.EqualTo("a"));
            Assert.That(descriptor.Actions.First().Parameters.First().TypeName, Is.EqualTo("String"));
        }
    }
}