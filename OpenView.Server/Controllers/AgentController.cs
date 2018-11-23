using Microsoft.AspNetCore.Mvc;
using OpenView.Server.Agents;
using OpenView.Server.Agents.Domain;
using OpenView.Server.DataTransfer;

namespace OpenView.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly AgentService _agentService;

        public AgentController(AgentService agentService)
        {
            _agentService = agentService;
        }

        public void Post([FromBody] AgentRegistration model)
        {
            var p = new AgentCreateParameter();
            var data = _agentService.Create(p);
        }
    }
}