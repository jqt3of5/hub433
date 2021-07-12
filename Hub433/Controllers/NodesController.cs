using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hub433.Busi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using mqtt.Notification;
using MQTTnet;
using Newtonsoft.Json;
using Node.Abstractions;

namespace Hub433.Controllers
{
    [ApiController]
    public class NodesController : Controller
    {
        private readonly NodeRepo _repo;
        private readonly MqttClientService _mqtt;

        public NodesController(NodeRepo repo, MqttClientService mqtt)
        {
            _repo = repo;
            _mqtt = mqtt;
        }

        [HttpGet]
        [Route("nodes/all")]
        public IActionResult GetNodes()
        {
            //TODO: Get only nodes belonging to the current user
            return Ok(_repo.Devices);
        }
        
        [HttpGet]
        [Route("node/{nodeGuid}")]
        public IActionResult GetDevice(string nodeGuid)
        {
            //TODO: Are we allowed to update this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }

            var node = _repo.GetDevice(nodeGuid)!;
            return Ok(node);
        }
        
        [HttpPost]
        [Route("node/{nodeGuid}/metadata/{propertyName}")]
        public IActionResult SetMetadataValue(string nodeGuid, string propertyName, [FromBody]string propertyValue)
        {
            //TODO: Are we allowed to update this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }

            //Should set Property based on reflection
            _repo.UpdateFriendlyName(nodeGuid, propertyValue);
            return Ok();
        }
        
        [HttpGet]
        [Route("node/{nodeGuid}/metadata/{propertyName}")]
        public IActionResult GetMetadataValue(string nodeGuid, string propertyName)
        {
            //TODO: Are we allowed to update this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }

            //TODO: Should Get property based on reflection
            var friendlyName = _repo.GetDevice(nodeGuid)!.FriendlyName;
            return Ok(friendlyName);
        }
        
        [HttpGet]
        [HttpPost]
        [Route("node/{nodeGuid}/action/{capabilityId}/{actionName}")]
        public async Task<IActionResult> InvokeAction(string nodeGuid, string capabilityId, string actionName, [FromBody]string[] parameters)
        {
            //TODO: Are we allowed to send these bytes to this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }
            
            try
            {
                var node = _repo.GetDevice(nodeGuid);
                var capability = node.DeviceCapabilities.FirstOrDefault(cap => cap.CapabilityType == capabilityId);
                if (capability != null)
                {
                    var action = capability.Actions.FirstOrDefault(act => act.Name == actionName);
                    if (action != null)
                    {
                        var body = System.Text.Json.JsonSerializer.Serialize(parameters);
                        //This will wait for a response until the client disconnects. SignalR might be a better mechanism for this possibly long running response, but this will work for now. 
                        // var result = await _mqtt.PublishWithResult(action.MqttTopic, body, HttpContext.RequestAborted);
                        // return Ok(result); 
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Is the exception caused by a disconnected client?
                return StatusCode(500, $"Could not send bytes to node. Error: {e}");
            }
            return Ok();
        }
        
        [HttpGet]
        [Route("node/claimCode")]
        IActionResult GenerateClaimCode()
        {
            //TODO: Store claim code, and associated user in DB
            return Ok(new Guid());
        }
       
        [HttpPost]
        [Route("node/{nodeGuid}/unclaim")]
        IActionResult UnclaimDevice(string nodeGuid)
        {
            //TODO: Are we allowed to send these bytes to this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }
            
            _repo.UnclaimDevice(nodeGuid);
            return Ok();
        }
    }
}