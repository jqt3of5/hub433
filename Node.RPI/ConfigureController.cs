using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mqtt.Notification;

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
        [Route("configure")]
        public IActionResult Configure([FromBody]ThingCreatedResponse config)
        {
            //TODO: Save certificate files
            //TODO: Store ThingName
            return Ok();
        } 
    }
}