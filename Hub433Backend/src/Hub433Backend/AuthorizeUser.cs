using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Internal;

namespace Hub433Backend
{
    public class AuthorizeUser
    {
        public async Task<bool> CanUserInvokeCapability(string email, string thingname, string capability)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.USWest1);
            var response = await client.GetItemAsync(Hub433ThingsTableSchema.TableName,
                new Dictionary<string, AttributeValue>()
                {
                    {Hub433ThingsTableSchema.PrimaryKey, new AttributeValue(thingname)}
                });
            
            if (response.Item.TryGetValue(Hub433ThingsTableSchema.OwnerEmail, out var value))
            {
                if (value.S == email)
                {
                   return true; 
                }
            }

            return false;
        }

        public async Task<bool> CanUserGetThingShadow(string email, string thingname)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.USWest1);
            var response = await client.GetItemAsync(Hub433ThingsTableSchema.TableName,
                new Dictionary<string, AttributeValue>()
                {
                    {Hub433ThingsTableSchema.PrimaryKey, new AttributeValue(thingname)}
                }); 
            
            if (response.Item.TryGetValue(Hub433ThingsTableSchema.OwnerEmail, out var value))
            {
                if (value.S == email)
                {
                    return true; 
                }
            }

            return false;
        }
    }
}