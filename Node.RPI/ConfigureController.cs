using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mqtt.Notification;
using Node.Abstractions;

namespace RPINode
{
    [ApiController]
    public class ConfigureController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMqttClientService _service;

        public ConfigureController(IConfiguration configuration, IMqttClientService service)
        {
            _configuration = configuration;
            _service = service;
        }
        
        [HttpPost]
        [Route("claim")]
        public async Task<IActionResult> Claim([FromBody]ClaimDeviceRequest config)
        {
            await _service.Publish($"thing/{_service.ThingName}/claim", JsonSerializer.Serialize(config));
            return Ok();
        } 
    }
}