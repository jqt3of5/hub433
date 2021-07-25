using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.IoT;
using Amazon.IoT.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace Hub433Backend
{
    public class CreateThing
    {
        public class ThingCreatedResponse
        {
            public string ThingName { get; set; }
            public string CertificatePem { get; set; }
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var client = new AmazonIoTClient(RegionEndpoint.USWest1);
           
            //Create Certificate
            var certificateResponse = await client.CreateKeysAndCertificateAsync();
            
            //Attach Policy to Certificate
            var attachPolicyResponse = await client.AttachPolicyAsync(new AttachPolicyRequest()
            {
                PolicyName = "TestingClient-Policy",
                Target = certificateResponse.CertificateArn
            });
            
            var thingName = ThingNameGenerator();
            //Create Thing
            var createThingResponse = await client.CreateThingAsync(new CreateThingRequest()
            {
                ThingName = thingName
            });

            //Attach Certificate to thing
            var attachThingPrincipalResponse = await client.AttachThingPrincipalAsync(new AttachThingPrincipalRequest()
            {
                Principal = certificateResponse.CertificateArn,
                ThingName = thingName
            });
            
            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(new ThingCreatedResponse()
                {
                    ThingName = thingName,
                    CertificatePem = certificateResponse.CertificatePem,
                    PublicKey = certificateResponse.KeyPair.PublicKey,
                    PrivateKey = certificateResponse.KeyPair.PrivateKey
                }),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private string ThingNameGenerator()
        {
            string[] words = {"razzle", "dazzle", "dog", "cat", "round", "square"};

            var rand = new Random();
            return
                $"{words[rand.Next(0, words.Length)]}.{words[rand.Next(0, words.Length)]}.{words[rand.Next(0, words.Length)]}";
        }
    }
}