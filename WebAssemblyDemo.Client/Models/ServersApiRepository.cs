using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WebAssemblyDemo.Client.Models
{
    public class ServersApiRepository : IServersRepository
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ServersApiRepository(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<List<Server>> GetServersAsync()
        {
            var httpClient = httpClientFactory.CreateClient("ServersApi");
            var response = await httpClient.GetAsync("servers.json");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content) && content != "null")
            {
                return JsonConvert.DeserializeObject<List<Server>>(content) ?? new List<Server>();
            }
            else
            {
                return new List<Server>();
            }
        }

        public async Task AddServerAsync(Server server)
        {
            server.ServerId = await GetNextServerIdAsync();

            var httpClient = httpClientFactory.CreateClient("ServersApi");
            var content = new StringContent(JsonConvert.SerializeObject(server), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"servers/{server.ServerId}.json", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<int> GetNextServerIdAsync()
        {
            var servers = await GetServersAsync();
            if(servers is not null && servers.Any())
                return servers.Where(s => s is not null).Max(s => s.ServerId) + 1;

            return 1;
        }
    }
}
