using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hub433Backend.Tests
{
    [TestFixture]
    public class ClaimCodeTests
    {
        [Test]
        public void TestGenerateClaimCode()
        {
            var claimCode = GenerateClaimCode.BuildClaimCode(new GenerateClaimCode.ClaimCodeRequest()
            {
                email = "jqt3of5@gmail.com",
                guid = "123456"
            }, "12345");
           
            Assert.That(claimCode, Is.Not.Null.Or.Empty);
        }
        [Test]
        public void TestValidateClaimCode()
        {
            var claimCode = GenerateClaimCode.BuildClaimCode(new GenerateClaimCode.ClaimCodeRequest()
            {
                email = "jqt3of5@gmail.com",
                guid = "123456"
            }, "12345");

            var result = GenerateClaimCode.ValidateClaimCode(claimCode, "12345", out var email);
            Assert.That(result);
            Assert.That(email,Is.EqualTo("jqt3of5@gmail.com"));
        }
        [Test]
        public void TestInvalidClaimCode()
        {
            var claimCode = GenerateClaimCode.BuildClaimCode(new GenerateClaimCode.ClaimCodeRequest()
            {
                email = "jqt3of5@gmail.com",
                guid = "123456"
            }, "abcdefg");

            var result = GenerateClaimCode.ValidateClaimCode(claimCode, "12345", out var email);
            Assert.That(result, Is.False);
            Assert.That(email,Is.EqualTo("jqt3of5@gmail.com"));
        }
        
        [Test]
        public async Task TestClaimCodeRequest()
        {
            var context = new Mock<ILambdaContext>();
            var generator = new GenerateClaimCode();
            var response = await generator.FunctionHandler(new APIGatewayProxyRequest()
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
                {
                    Authorizer = new APIGatewayCustomAuthorizerContext()
                    {
                        Claims = new() {{"email", "jqt3of5@gmail.com"}}
                    }
                }
            }, context.Object);

            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);
            
            Assert.That(body.ContainsKey("claimcode"));
            Assert.That(GenerateClaimCode.ValidateClaimCode(body["claimcode"], GenerateClaimCode.SignatureKey,
                out var email));
            Assert.That(email, Is.EqualTo("jqt3of5@gmail.com"));
        }

        [Test]
        public async Task TestClaimDevice()
        {
            var context = new Mock<ILambdaContext>();
            var claimCode = GenerateClaimCode.BuildClaimCode(new GenerateClaimCode.ClaimCodeRequest()
            {
                email = "jqt3of5@gmail.com",
                guid = "12345"
            }, GenerateClaimCode.SignatureKey);

            var claimer = new ClaimDevice();
            await claimer.FunctionHandler(new ClaimDevice.ClaimCodeRequest()
            {
                ClaimCode = claimCode,
                ThingName = "UnitTestThing"
            }, context.Object);
        }
    }
}