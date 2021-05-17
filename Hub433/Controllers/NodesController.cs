using System;
using System.Text;
using Hub433.Busi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Hub433.Controllers
{
    [ApiController]
    [Route("nodes")]
    public class NodesController : Controller
    {
        private readonly NodeRepo _repo;
        private readonly IHubContext<NodeHub, INodeClient> _hubContext;

        public NodesController(NodeRepo repo, IHubContext<NodeHub, INodeClient> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetNodes()
        {
            //TODO: Get only nodes belonging to the current user
            return Ok(_repo.Devices);
        }

        [HttpGet]
        [HttpPost]
        [Route("transmit/{nodeGuid}")]
        public IActionResult SendBytes(string nodeGuid, [FromBody]string base64)
        {
            //TODO: Are we allowed to send these bytes to this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }

            try
            {
                var node = _repo.GetDevice(nodeGuid);
                _hubContext.Clients.Client(node.NodeClientConnectionId).SendBytes(base64);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Could not send bytes to node. Error: {e}");
            }
            return Ok();
        }
        
        [HttpGet]
        [Route("{nodeGuid}")]
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
        [Route("{nodeGuid}/{propertyName}")]
        public IActionResult SetPropertyValue(string nodeGuid, string propertyName, [FromBody]string propertyValue)
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
        [Route("{nodeGuid}/{propertyName}")]
        public IActionResult GetPropertyValue(string nodeGuid, string propertyName)
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

        //[HttpGet]
        //IActionResult GenerateClaimCode()
        //{
            //Called by the application
            //Generate a random onetime use token for the authenticated user to claim a device 
        //}

        [HttpPost]
        [Route("claim/{nodeGuid}")]
        IActionResult ClaimDevice(string nodeGuid)//, [FromBody] string claimCode)
        {
            //Called by the device
            //authenticate device (client Certificates maybe?)
           //Validate claim code exists
           //Assign user associated with claimcode to device
           var user = "GeneralUser";
           _repo.ClaimDevice(nodeGuid, user);
           return Ok();
        }

        //[HttpPost]
        //IActionResult UnclaimDevice(string nodeid)
        //{
        //}
    }
}