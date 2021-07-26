using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mqtt.Notification;
using Newtonsoft.Json;

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
        public async Task<IActionResult> Claim([FromBody]ClaimCodeRequest config)
        {
            await _service.Publish($"thing/{_service.ThingName}/claim", JsonConvert.SerializeObject(config));
            return Ok();
        } 
    }
}