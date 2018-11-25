using Microsoft.AspNetCore.Mvc;
using OpenView.Server.Agents;
using OpenView.Server.Agents.Domain;
using OpenView.Server.DataTransfer;
using System.Linq;

namespace OpenView.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly AgentsService _agentService;

        public AgentsController(AgentsService agentService)
        {
            _agentService = agentService;
        }

        [HttpPost]
        public AgentModel Post([FromBody] AgentRegistrationModel model)
        {
            var data = _agentService.Create(AgentConverter.Convert(model));
            return AgentConverter.Convert(data);
        }

        [HttpGet]
        public AgentModel[] Get()
        {
            var data = _agentService.Get();
            return data.Select(AgentConverter.Convert).ToArray();
        }
    }
}