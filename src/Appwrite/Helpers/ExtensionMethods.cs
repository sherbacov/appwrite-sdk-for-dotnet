using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Appwrite.Helpers;


    public enum JsonAction
    {
        Get,
        Create,
        Update,
        Delete
    }
    
    
    public static class ExtensionMethods
    {
        public static string ToJson(this Dictionary<string, object> dict, JsonAction action = JsonAction.Create)
        {
            var settings = new JsonSerializerSettings
            {
                //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //ContractResolver = new DefaultContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter(), new WeirdNameSerializer(action) }
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
    
    
    public class WeirdNameSerializer : JsonConverter
    {

        private JsonAction _action;
        public WeirdNameSerializer(JsonAction action)
        {
            _action = action;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            Type type = value.GetType();
            //jo.Add("type", type.Name);

            foreach (var prop in type.GetProperties().ToList())
            {
                if (prop.CanRead)
                {
                    if (_action == JsonAction.Create && prop.Name.ToLowerInvariant() == "id")
                        continue;
                    
                    
                    var propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                    }
                }
            }
            jo.WriteTo(writer);
            
            //serializer.Serialize(writer, value);
        }

        public override bool CanRead { get; } = false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //return base.ReadJson(reader, objectType, existingValue, serializer);

            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(string))
                return false;

            if (objectType == typeof(int))
                return false;

            if (objectType == typeof(decimal))
                return false;

            if (objectType == typeof(float))
                return false;

            //if (objectType.IsTypeDefinition)
            //    return false;
            
            if (objectType.IsGenericType)
                return false;
            
            return true;
        }
    }
    
