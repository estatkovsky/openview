using OpenView.Server.DataTransfer;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OpenView.Agent
{
    public class ServerClient
    {
        private readonly HttpClient _client = new HttpClient();

        public ServerClient(string serverUrl)
        {
            _client.BaseAddress = new Uri(serverUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task Register(AgentRegistrationModel model)
        {
            var response = await _client.PostAsJsonAsync("api/agents", model);
            response.EnsureSuccessStatusCode();
        }
    }
}
