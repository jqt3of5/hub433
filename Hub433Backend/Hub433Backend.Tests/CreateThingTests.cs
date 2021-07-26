using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Moq;
using NUnit.Framework;

namespace Hub433Backend.Tests
{
    public class CreateThingTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var contextMock = new Mock<ILambdaContext>();
            var response = await new CreateThing().FunctionHandler(new APIGatewayProxyRequest()
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
                {
                    Authorizer = new APIGatewayCustomAuthorizerContext()
                    {
                        Claims = new (){{"email", "jqt3of5@gmail.com"}}
                    }
                }
            }, contextMock.Object);
        }
    }
}