using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Appwrite.Helpers
{
    public static class ExtensionMethods
    {
        public static string ToJson(this Dictionary<string, object> dict)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            return JsonConvert.SerializeObject(dict, settings);
        }

        public static async Task<T> ToObject<T>(this HttpResponseMessage message)
        {
            //ITraceWriter traceWriter = new MemoryTraceWriter();

            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                //TraceWriter = traceWriter, 
                Converters = new List<JsonConverter> {new StringEnumConverter()}
            };
            
            var content = await message.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(content, settings);

            //Console.WriteLine(traceWriter);

            return result;
        }

        public static string ToQueryString(this Dictionary<string, object> parameters)
        {
            List<string> query = new List<string>();

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                if (parameter.Value != null)
                {
                    if (parameter.Value is List<object>)
                    {
                        foreach(object entry in (dynamic) parameter.Value) 
                        {
                            query.Add(parameter.Key + "[]=" + Uri.EscapeUriString(entry.ToString()));
                        }
                    } 
                    else 
                    {
                        query.Add(parameter.Key + "=" + Uri.EscapeUriString(parameter.Value.ToString()));
                    }
                }
            }
            return string.Join("&", query);
        }
    }
}