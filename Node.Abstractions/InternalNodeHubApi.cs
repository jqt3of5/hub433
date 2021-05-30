using System.Threading.Tasks;
using RestSharp;

namespace Node.Abstractions
{
    public class InternalNodeHubApi
    {
        private RestClient _httpClient = new RestClient("http://localhost");

        public async Task DeviceOnline(string deviceGuid, DeviceCapabilityDescriptor[] capabilities)
        {
            var request = new RestRequest($"/node/{deviceGuid}/online", Method.POST, DataFormat.Json);
            request.AddJsonBody(capabilities);
            var response = await _httpClient.ExecuteAsync(request);
        }

        public async Task DeviceOffline(string deviceGuid)
        {
            var request = new RestRequest($"/node/{deviceGuid}/offline", Method.POST);
            var response = await _httpClient.ExecuteAsync(request);
        }
    }
}