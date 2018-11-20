using Microsoft.AspNetCore.Mvc;
using OpenView.Server.DataTransfer;

namespace OpenView.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        public void Post([FromBody] AgentRegistration data)
        {
        }
    }
}