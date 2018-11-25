using log4net;
using OpenView.Server.DataTransfer;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OpenView.Agent
{
    public class ServerConnection : IDisposable
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ServerConnection));
        private ServerClient _serverClient;
        private string _sessionId;
        private ServerConnectionStateEnum _connectionState = ServerConnectionStateEnum.Closed;

        public void Open()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                throw new ApplicationException("Connection already opened");
            }
            _serverClient = new ServerClient(AgentConfiguration.ServerUrl);
            _sessionId = Guid.NewGuid().ToString();
            RegisterAgent();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        private async Task RegisterAgent()
        {
            var model = new AgentRegistrationModel();
            model.SessionId = _sessionId;
            model.HostName = Dns.GetHostName();
            await _serverClient.Register(model);
            _connectionState = ServerConnectionStateEnum.Opened;
        }
    }
}
