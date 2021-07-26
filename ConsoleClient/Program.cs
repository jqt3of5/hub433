using System;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.IoT;
using Amazon.IoT.Model;
using Amazon.Runtime;
using RestSharp;

namespace ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var result = await Login(args[1]);
            var client = new RestClient("https://z7r1yyeoef.execute-api.us-west-1.amazonaws.com");
            client.AddDefaultHeader("Authorization", result.IdToken);
            switch (args[0])
            {
                case "create":
                    var request = new RestRequest("Stage/thing/create");
                    var response = client.Get(request);
                    Console.WriteLine(response.Content); 
                    break;
                case "generateclaimcode":
                    request = new RestRequest("Stage/thing/claimcode");
                    response = client.Get(request);
                    Console.WriteLine(response.Content);
                    break;
            }
        }

        static async Task<AuthenticationResultType> Login(string username)
        {
            var client = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USWest1);
            var userPool = new CognitoUserPool("us-west-1_g4JeVFuCV", "rop16j0et2ps80mshb3gutfsq", client);
            var user = new CognitoUser(username, "rop16j0et2ps80mshb3gutfsq", userPool, client);

            Console.Write("Enter Password: ");
            var pwd = GetPassword();

            try
            {
                var result = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
                {
                    Password = pwd.ToString()
                });
            
                while (result.AuthenticationResult == null)
                {
                    if (result.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                    {
                        Console.Write("New Password: ");
                        pwd = GetPassword();
                        result = await user.RespondToNewPasswordRequiredAsync(
                            new RespondToNewPasswordRequiredRequest()
                            {
                                NewPassword = pwd,
                                SessionID = result.SessionID
                            });
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized Challenge");
                        return null;
                    }
                }

                Console.WriteLine("Successful Login as: " + username);
                return result.AuthenticationResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }


            string GetPassword()
            {
                var pwd = new StringBuilder();
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (pwd.Length > 0)
                            pwd.Remove(pwd.Length - 1,pwd.Length);
                    }
                    if (key.KeyChar != '\u0000')
                        pwd.Append(key.KeyChar); 
                }

                Console.WriteLine();
                return pwd.ToString();
            }
        }
        
    }
}