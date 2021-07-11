using System;
using System.Collections.Immutable;
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
        [Capability(nameof(IPropertyMockCapability), "1.0.0")]
        public interface IPropertyMockCapability : ICapability
        {
            [Value]
            public string PropertyString { get; }
            [Value]
            public int PropertyInt{ get; }
        }
        [Capability(nameof(IMixedMockCapability), "1.0.0")]
        public interface IMixedMockCapability : ICapability
        {
            [CapabilityAction]
            public void ActionNoParams();
            [CapabilityAction]
            public void ActionStringParam(string a); 
            [CapabilityAction]
            public void ActionMixedParam(string a, int b);
            
            [Value]
            public string PropertyString { get; }
            [Value]
            public int PropertyInt{ get; }
        }
        [Capability(nameof(IActionMockCapability), "1.0.0")]
        public interface IActionMockCapability : ICapability
        {
            [CapabilityAction]
            public void ActionNoParams();
            [CapabilityAction]
            public void ActionStringParam(string a);
            [CapabilityAction]
            public void ActionIntParam(int a);
            [CapabilityAction]
            public void ActionMixedParam(string a, int b);
            [CapabilityAction]
            public string FuncStringReturn();
            [CapabilityAction]
            public string FuncMixed(string a, int b);
        }
        [Capability(nameof(IActionAMockCapability), "1.0.0")]
        public interface IActionAMockCapability : ICapability
        {
            [CapabilityAction]
            void Foo();
        }
        [Capability(nameof(IActionBMockCapability), "1.0.0")]
        public interface IActionBMockCapability : ICapability
        {
            [CapabilityAction]
            string Foo();
        }
        [Capability(nameof(IActionCMockCapability), "1.0.0")]
        public interface IActionCMockCapability : ICapability
        {
            [CapabilityAction]
            void Foo(string a);
        } 
        [Capability(nameof(IActionDMockCapability), "1.0.0")]
        public interface IActionDMockCapability : ICapability
        {
            [CapabilityAction]
            string Foo(string a);
        } 
        [Test]
        public void TestEmptyCapabilityDescriber()
        {
            var capability = new Mock<ICapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);

            Assert.That(descriptor.CapabilityType, Is.Not.Empty);
            Assert.That(descriptor.CapabilityVersion, Is.EqualTo("0.0.0"));
            Assert.That(descriptor.Actions, Is.Empty);
            Assert.That(descriptor.Properties, Is.Empty);
        }
        [Test]
        public void TestCapabilityAttributeValues()
        {
            var capability = new Mock<IActionMockCapability>();
            var descriptor = CapabilityDescriber.Describe(capability.Object);
            
            Assert.That(descriptor.CapabilityType, Is.EqualTo(nameof(IActionMockCapability)));
            Assert.That(descriptor.CapabilityVersion, Is.EqualTo("1.0.0"));
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