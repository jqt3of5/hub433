using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
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
using Node.Abstractions;
using RestSharp;

namespace ConsoleClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {

            var rootCommand = new RootCommand()
            {
                new Option("--command", 
                    "The specific command to execute. Options are: create, generateclaimcode"),
                new Option("--user",
                    "The specific user to validate as"),
                new Option("--base-url", 
                    getDefaultValue: () => "https://z7r1yyeoef.execute-api.us-west-1.amazonaws.com",
                    description: "The base URL of our api"),
                new Option("--node-api",
                    getDefaultValue: () => "http://localhost:8080",
                    description: "Node api URL")
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string>((async (command, user, baseUrl, nodeApi) =>
                    {
                        var result = await Login(user);
                        var awsClient = new RestClient(baseUrl);
                        awsClient.AddDefaultHeader("Authorization", result.IdToken);
                        switch (command)
                        {
                            case "create":
                                var request = new RestRequest("Stage/thing/create");
                                var response = awsClient.Get<ThingCreatedResponse>(request);
                                Console.WriteLine(response.Content); 
                                break;
                            case "generateclaimcode":
                                request = new RestRequest("Stage/thing/claimcode");
                                var claimCodeResponse = awsClient.Get<ClaimCodeRequest>(request);
                    
                                var nodeClient = new RestClient(nodeApi);
                                var claimRequest = new RestRequest("/claim", DataFormat.Json);
                    
                                claimRequest.AddJsonBody(claimCodeResponse.Data);
                                var claimDeviceResponse = nodeClient.Post(claimRequest);
                                Console.WriteLine(claimDeviceResponse);
                    
                                break;
                        } 
                    }
                ));

            return await rootCommand.InvokeAsync(args);
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
                            pwd.Remove(pwd.Length - 1,pwd.Length-1);
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