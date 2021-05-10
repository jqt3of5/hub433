using Microsoft.AspNetCore.Mvc;

namespace Hub433
{
    [ApiController]
    [Route("status")]
    public class StatusController : Controller
    {
        private readonly NodeRepo _repo;

        public StatusController(NodeRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        IActionResult Status()
        {
            return Ok();
        }
        
    }
}