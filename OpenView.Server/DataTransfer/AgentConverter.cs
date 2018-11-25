using OpenView.Server.Agents.Domain;

namespace OpenView.Server.DataTransfer
{
    public static class AgentConverter
    {
        public static AgentCreateParameter Convert(AgentRegistrationModel self)
        {
            var data = new AgentCreateParameter();
            data.SessionId = self.SessionId;
            data.HostName = self.HostName;
            data.RemoteAddress = self.RemoteAddress;
            data.User = self.User;
            data.WebSockerUrl = self.WebSockerUrl;
            data.PrimaryScreenPreview = self.PrimaryScreenPreview;
            return data;
        }

        public static AgentModel Convert(Agent self)
        {
            var model = new AgentModel();
            model.SessionId = self.SessionId;
            model.HostName = self.HostName;
            model.RemoteAddress = self.RemoteAddress;
            model.User = self.User;
            model.WebSockerUrl = self.WebSockerUrl;
            return model;
        }
    }
}
