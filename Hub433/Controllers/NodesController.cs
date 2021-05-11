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
        [Route("{nodeGuid}/bytes")]
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
        [HttpPost]
        [Route("{nodeGuid}/friendlyName")]
        public IActionResult SetFriendlyName(string nodeGuid, [FromBody]string friendlyName)
        {
            //TODO: Are we allowed to update this device?
            if (!_repo.DoesNodeExist(nodeGuid))
            {
                return StatusCode(404,$"Node with id {nodeGuid} did not exist");
            }

            _repo.UpdateFriendlyName(nodeGuid, friendlyName);
            return Ok();
        }

        //[HttpGet]
        //IActionResult GenerateClaimCode()
        //{
            //Called by the application
           //Generate a random onetime use token for the authenticated user to claim a device 
        //}

        [HttpPost]
        [Route("{nodeGuid}/claim")]
        IActionResult ClaimDevice(string nodeGuid)//, string claimCode)
        {
            //Called by the device
            //authenticate device (client Certificates maybe?)
           //Validate claim code exists
           //Assign user to device
           _repo.RegisterDevice(new DeviceMetadata(){Guid = nodeGuid, IsConnected = false});
           return Ok();
        }

        //[HttpPost]
        //IActionResult UnclaimDevice(string nodeid)
        //{
        //}
    }
}