using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace Hub433Backend 
{
    public class GenerateClaimCode
    {
        private static readonly HttpClient client = new HttpClient();

        public class ClaimCodeRequest
        {
            public string email { get; set; }
            public string guid { get; set; } 
        }

        public static string BuildClaimCode(ClaimCodeRequest request, string sharedKey)
        {
            var signature = SignRequestCode(request, sharedKey); 
            var claimCodeObject = new Dictionary<string, string>()
            {
                {"email", request.email},
                {"guid", request.guid},
                {"signature", signature} 
            };
            
            //Encode the object into a claim code
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(claimCodeObject))); 
        }
        
        public static string SignRequestCode(ClaimCodeRequest request, string sharedKey)
        {
            //TODO: This uses a shared key to perform the signature. Not the best way to do this
            var hmac = HMAC.Create();
            hmac.Initialize();
            hmac.Key = Encoding.UTF8.GetBytes(sharedKey);
           
            //Compute the signature hash of the object
            var objStr = JsonConvert.SerializeObject(request);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(objStr));

            return Convert.ToBase64String(hash);
        }

        public static bool ValidateClaimCode(string claimCode, string sharedKey, out string email)
        {
            var claimCodeObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(Convert.FromBase64String(claimCode)));
            if (!claimCodeObject.ContainsKey("email") || !claimCodeObject.ContainsKey("guid"))
            {
                email = null;
                return false;
            }

            email = claimCodeObject["email"];
            
            if (!claimCodeObject.ContainsKey("signature"))
            {
                return false;
            }
            
            var signature = SignRequestCode(new ClaimCodeRequest()
            {
                email = claimCodeObject["email"],
                guid = claimCodeObject["guid"],
            }, sharedKey);

            return signature == claimCodeObject["signature"];
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var email = apigProxyEvent.RequestContext.Authorizer.Claims["email"];

            //TODO: Secret in code! Bad!!
            var claimCode = BuildClaimCode(new ClaimCodeRequest()
            {
                email = email,
                guid = Guid.NewGuid().ToString()
            }, ";lk@#$asdf@daf#");
            
            var body = new Dictionary<string, string>
            {
                { "claimcode", claimCode},
            };
            
            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(apigProxyEvent.RequestContext.Authorizer.Claims),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
