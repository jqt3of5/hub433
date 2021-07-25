using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
        private static readonly HttpClient client = new HttpClient();
        
        public async Task FunctionHandler(string claimCode, string thingName, ILambdaContext context)
        {
            //TODO: Secret in code! Bad!!
            if (GenerateClaimCode.ValidateClaimCode(claimCode, ";lk@#$asdf@daf#", out var email))
            {
                //TODO: Assign the thing to the user specified in the claim code                
            }
        }
    }
}
