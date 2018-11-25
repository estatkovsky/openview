using System.Collections.Generic;
using System.Configuration;

namespace OpenView.Agent
{
    public static class AgentConfiguration
    {
        private static Dictionary<string, string> _cachedSettings = new Dictionary<string, string>();

        public static int WebSocketPort { get { return int.Parse(GetConfigValue("web_socket_port")); } }

        public static int WebSocketConcurrentConnectionsLimit { get { return int.Parse(GetConfigValue("web_socket_concurrent_connections_limit")); } }

        public static string ServerUrl { get { return GetConfigValue("server_url"); } }

        private static string GetConfigValue(string key)
        {
            if (_cachedSettings.ContainsKey(key))
            {
                return _cachedSettings[key];
            }
            string value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(value))
            {
                _cachedSettings.Add(key, value);
            }
            return value;
        }
    }
}
