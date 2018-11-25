namespace OpenView.Server.Agents.Domain
{
    public class AgentCreateParameter
    {
        public string SessionId { get; set; }

        public string HostName { get; set; }

        public string RemoteAddress { get; set; }

        public string User { get; set; }

        public string WebSockerUrl { get; set; }

        public string PrimaryScreenPreview { get; set; }
    }
}
