using Hub433.Busi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Node.Abstractions;

namespace Hub433.Controllers
{
    //Endpoints used by the nodes themselves, to publish certain status/etc
    [Route("internalnode")]
    [ApiController]
    public class InternalNodeController : Controller
    {
        private readonly NodeRepo _repo;

        public InternalNodeController(NodeRepo repo)
        {
            _repo = repo;
        } 
        
        [HttpPost]
        [Route("{nodeGuid}/claim")]
        public IActionResult ClaimDevice(string nodeGuid, [FromBody] string claimCode)
        {
            //Called by the device
            //authenticate device (client Certificates maybe?)
           //Validate claim code exists
           //Assign user associated with claimcode to device
           var userId = "GeneralUser";
           _repo.ClaimDevice(nodeGuid, userId);
           return Ok();
        }

        [HttpPost]
        [HttpGet]
        [Route("{nodeGuid}/online")]
        public IActionResult DeviceOnline(string nodeGuid, [FromBody] DeviceCapabilityDescriptor[]? deviceCapabilities)
        {
            _repo.DeviceOnline(nodeGuid, deviceCapabilities);
            return Ok();
        }
        
        [HttpPost]
        [Route("{nodeGuid}/offline")]
        public IActionResult DeviceOffline(string nodeGuid)
        {
            _repo.DeviceOffline(nodeGuid);
            return Ok();
        }
    }
}