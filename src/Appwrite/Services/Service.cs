using Appwrite.Models;

namespace Appwrite.Services;

    public abstract class Service
    {
        protected readonly Client _client;

        protected Service(Client client)
        {
            _client = client;
        }

        protected async Task<HttpResponseMessage> ListObjects(
            string path,
            string[] queries = null, int? limit = 25, int? offset = 0, 
            OrderType orderType = OrderType.ASC) 
        {
            var parameters = new Dictionary<string, object>()
            {
                { "limit", limit },
                { "offset", offset },
                { "orderType", orderType.ToString() }
            };

            if (queries != null)
            {
                foreach (var query in queries)
                {
                    parameters.Add("queries[]", query);
                }
            }

            var headers = JsonHeaders();

            return await _client.Call("GET", path, headers, parameters);
        }

        protected Dictionary<string, string> JsonHeaders()
        {
            return new Dictionary<string, string>()
            {
                {"content-type", "application/json"}
            };
        }
        
    }

