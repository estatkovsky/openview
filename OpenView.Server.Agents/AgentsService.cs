using OpenView.Server.Agents.Domain;
using System.Collections.Concurrent;
using System.Linq;

namespace OpenView.Server.Agents
{
    public class AgentsService
    {
        private readonly ConcurrentDictionary<string, Agent> _activeAgents = new ConcurrentDictionary<string, Agent>();

        public Agent Create(AgentCreateParameter p)
        {
            var data = new Agent();
            data.SessionId = p.SessionId;
            data.HostName = p.HostName;
            data.RemoteAddress = p.RemoteAddress;
            data.User = p.User;
            data.WebSockerUrl = p.WebSockerUrl;
            data.PrimaryScreenPreview = p.PrimaryScreenPreview;
            _activeAgents.AddOrUpdate(data.SessionId, data, (k, v) => data);
            return data;
        }

        public Agent[] Get()
        {
            return _activeAgents.Values.ToArray();
        }
    }
}
