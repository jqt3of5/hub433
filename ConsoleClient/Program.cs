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
using Microsoft.VisualBasic;
using Node.Abstractions;
using RestSharp;

namespace ConsoleClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            const string defaultBaseUrl = "https://z7r1yyeoef.execute-api.us-west-1.amazonaws.com";
            var createCommand = new Command("create", "Create a new thing associated to the logged-in user")
            {
                new Option("--user",
                    "The specific user to validate as"),
                new Option("--base-url", 
                    getDefaultValue: () => defaultBaseUrl,
                    description: "The base URL of our api"),
            };
            var claimCommand = new Command("claim", "Generates a claim code for an unclaimed thing, and sends it to the thing server for validation")
            {
                new Option("--user",
                    getDefaultValue: () => "", 
                    description: "The specific user to validate as"),
                new Option("--base-url", 
                    getDefaultValue: () => defaultBaseUrl, 
                    description: "The base URL of our api"),
                new Option("--thing-host",
                    getDefaultValue: () => "",
                    description: "Node api URL"),
            }; 
            var invokeCapability = new Command("capability", "Invoke a capability for a thing")
            {
                new Argument("thingName"),
                new Argument("capability"),
                new Option("--user",
                    getDefaultValue: () => "", 
                    description: "The specific user to validate as"),
                new Option("--capability-type",
                    getDefaultValue: () => "", 
                    description: "The capability type"),
                new Option("--capability-action",
                    getDefaultValue: () => "", 
                    description: "The capability action belonging to the type"),
                new Option("--capability-version",
                    getDefaultValue: () => "latest", 
                    description: "The capability version to invoke"), 
                new Option("--base-url", 
                    getDefaultValue: () => defaultBaseUrl, 
                    description: "The base URL of our api"),
            }; 
            var getThingShadow = new Command("shadow", "Generates a claim code for an unclaimed thing, and sends it to the thing server for validation")
            {
                new Argument("thingName"),
                new Option("--user",
                    getDefaultValue: () => "", 
                    description: "The specific user to validate as"),
                new Option("--base-url", 
                    getDefaultValue: () => defaultBaseUrl, 
                    description: "The base URL of our api"),
            }; 
            var rootCommand = new RootCommand()
            {
                createCommand, 
                claimCommand,
                invokeCapability,
                getThingShadow
            };
            
            invokeCapability.Handler = CommandHandler.Create<string,string, string, string, string, string>(async (thingName, capabilityType, capabilityAction, capabilityVersion, user, baseUrl) =>
            {
                if (string.IsNullOrEmpty(capabilityAction) || string.IsNullOrEmpty(capabilityType))
                {
                    Console.WriteLine("Must supply a capability type and action");
                    return;
                }
                
                if (string.IsNullOrEmpty(user))
                {
                    Console.Write("Username: ");
                    user = Console.ReadLine();
                }
                
                var result = await Login(user);
                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return;
                }
                
                var awsClient = new RestClient(baseUrl);
                awsClient.AddDefaultHeader("Authorization", result.IdToken); 
                
                var request = new RestRequest($"Stage/thing/capability/{thingName}");
                request.AddJsonBody(new DeviceCapabilityRequest()
                {
                   CapabilityAction = capabilityAction,
                   CapabilityType = capabilityType,
                   CapabilityVersion = capabilityVersion,
                });
                var response = awsClient.Get<ThingCreatedResponse>(request);
                
                Console.WriteLine(response.Content); 
            });
            
            getThingShadow.Handler = CommandHandler.Create<string, string, string>(async (thingName, user, baseUrl) =>
            {
                if (string.IsNullOrEmpty(user))
                {
                    Console.Write("Username: ");
                    user = Console.ReadLine();
                }
                
                var result = await Login(user);
                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return;
                }
                
                var awsClient = new RestClient(baseUrl);
                awsClient.AddDefaultHeader("Authorization", result.IdToken); 
                
                var request = new RestRequest($"Stage/thing/shadow/{thingName}");
                //request.AddJsonBody()
                var response = awsClient.Get<ThingCreatedResponse>(request);
                
                Console.WriteLine(response.Content); 
            });

            createCommand.Handler = CommandHandler.Create<string, string>(async (user, baseUrl) =>
            {
                if (string.IsNullOrEmpty(user))
                {
                    Console.Write("Username: ");
                    user = Console.ReadLine();
                }
                
                var result = await Login(user);
                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return;
                }
                
                var awsClient = new RestClient(baseUrl);
                awsClient.AddDefaultHeader("Authorization", result.IdToken); 
                
                var request = new RestRequest("Stage/thing/create");
                var response = awsClient.Get<ThingCreatedResponse>(request);
                
                Console.WriteLine(response.Content); 
            });
            
            claimCommand.Handler = CommandHandler.Create<string, string, string>(async (user, baseUrl, thingHost) =>
            {
                if (string.IsNullOrEmpty(user))
                {
                    Console.Write("Username: ");
                    user = Console.ReadLine();
                }
                
                var result = await Login(user);
                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return;
                }
                
                var awsClient = new RestClient(baseUrl);
                awsClient.AddDefaultHeader("Authorization", result.IdToken); 
                
                var request = new RestRequest("Stage/thing/claimcode");
                var claimCodeResponse = awsClient.Get<ClaimCodeRequest>(request);

                if (!claimCodeResponse.IsSuccessful)
                {
                    Console.WriteLine($"Failed to generate claim code. Error: HTTP {claimCodeResponse.StatusCode} {claimCodeResponse.Content}");
                    return;
                }

                if (string.IsNullOrEmpty(thingHost))
                {
                    Console.WriteLine(claimCodeResponse.Content);
                    return;
                }
                
                var nodeClient = new RestClient(thingHost);
                var claimRequest = new RestRequest("/claim", DataFormat.Json);
                claimRequest.AddJsonBody(claimCodeResponse.Data);
                
                var claimDeviceResponse = nodeClient.Post(claimRequest);
                
                Console.WriteLine(claimDeviceResponse);

            });
   
            return await rootCommand.InvokeAsync(args);
        }

        static async Task<AuthenticationResultType> Login(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            
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