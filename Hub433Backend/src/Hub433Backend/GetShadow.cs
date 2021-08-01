using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.IoT;
using Amazon.IoT.Model;
using Amazon.IotData;
using Amazon.IotData.Model;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace Hub433Backend 
{
    public class GetShadow
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var authorizeUser = new AuthorizeUser();

                if (request.PathParameters.TryGetValue("thingName", out var thingName) &&
                    request.RequestContext.Authorizer.TryGetValue("claims", out var o) && o is JObject claims)
                {
                    var email = claims["email"].ToString();
                    if (await authorizeUser.CanUserGetThingShadow(email, thingName))
                    {
                        var client = new AmazonIoTClient(RegionEndpoint.USWest1);
                        var endpoint = await client.DescribeEndpointAsync(new DescribeEndpointRequest()
                        {
                            EndpointType = "iot:Data-ATS"
                        });

                        var dataClient = new AmazonIotDataClient($"https://{endpoint.EndpointAddress}");
                        var shadow = await dataClient.GetThingShadowAsync(new GetThingShadowRequest()
                        {
                            ThingName = thingName,
                            ShadowName = string.Empty
                        });

                        using (var reader = new StreamReader(shadow.Payload))
                        {
                            return new APIGatewayProxyResponse()
                            {
                                Body = await reader.ReadToEndAsync(),
                                StatusCode = 200
                            }; 
                        }
                    }
                    return new APIGatewayProxyResponse()
                    {
                        Body = $"Not authorized to access {thingName ?? "<no thing name>"}",
                        StatusCode = 401
                    };
                }
                return new APIGatewayProxyResponse()
                {
                    Body = $"Not authorized",
                    StatusCode = 401
                };
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse()
                {
                    Body = e.ToString(),
                    StatusCode = 200 
                }; 
            }
        }
    }
}
