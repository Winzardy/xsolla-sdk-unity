using System;
using Newtonsoft.Json;

namespace Xsolla.SDK.Common
{
    internal static class XsollaClientHelpers
    {
        private const string Tag = "XsollaClientHelpers";
        
        private static readonly JsonSerializerSettings serializerSettings;

        public static string EmptyJson => "{}";
        
        static XsollaClientHelpers()
        {
            serializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };
        }
        
        public static T FromJson<T>(string json) where T : class {
            T result;
            try { result = JsonConvert.DeserializeObject<T>(json, serializerSettings); } 
            catch (Exception e) {
                XsollaLogger.Error(Tag, $"Deserialization failed for {typeof(T)} {e}");
                result = null;
            }
            return result;
        }
        
        public static string ToJson<T>(T data) where T : class {
            return JsonConvert.SerializeObject(data, serializerSettings);
        }
        
        public static string ConfigurationToJson(XsollaClientConfiguration configuration) => ToJson(configuration);
    }
}
