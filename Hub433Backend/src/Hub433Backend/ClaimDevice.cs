using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.IoT.Model;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Hub433Backend 
{
    public class ClaimDevice
    {
        public class ClaimCodeRequest
        {
            public string ClaimCode { get; set; }
            public string ThingName { get; set; }
        }
        public async Task FunctionHandler(ClaimCodeRequest request, ILambdaContext context)
        {
            if (GenerateClaimCode.ValidateClaimCode(request.ClaimCode, GenerateClaimCode.SignatureKey, out var email))
            {
                var client = new AmazonDynamoDBClient(RegionEndpoint.USWest1);
                await client.UpdateItemAsync(Hub433ThingsTableSchema.TableName, 
                    new()
                    {
                        {Hub433ThingsTableSchema.PrimaryKey, new AttributeValue(request.ThingName)}
                    }, 
                    new ()
                    {
                        {Hub433ThingsTableSchema.OwnerEmail, new AttributeValueUpdate(new AttributeValue(email), AttributeAction.PUT)}
                    });
            }
        }
    }
}
