using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Hub433Backend.Tests
{
    [TestFixture]
    public class ShadowTests
    {
        [Test]
        public async Task TestGetShadow()
        {
            var context = new Mock<ILambdaContext>();
            var client = new GetShadow();
            var response = await client.FunctionHandler(new APIGatewayProxyRequest()
            {
                PathParameters = new Dictionary<string, string>()
                {
                    {"thingName", "TestingThing"}
                },
               RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
               {
                   Authorizer = new APIGatewayCustomAuthorizerContext()
                   {
                       {"claims", new JObject()
                       {
                           {"email", "jqt3of5@gmail.com"}
                       }}
                   }
               }
                
            }, context.Object);
        } 
    }
}