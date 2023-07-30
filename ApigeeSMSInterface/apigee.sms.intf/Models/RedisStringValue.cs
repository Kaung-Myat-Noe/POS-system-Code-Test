using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apigee.sms.intf.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RedisStringValue
    {
        // [Required]
        // public string Id { get; set; } = $"redisHash:{Guid.NewGuid().ToString()}";

        [JsonProperty("Key",Required = Required.AllowNull)]
        public string Key { get; set; } = $"redisString:{Guid.NewGuid().ToString()}";

        [JsonProperty("Value",Required = Required.AllowNull)]
        public JArray Value { get; set; } = new JArray();

        [JsonProperty("Value_String")]
        public string[] Value_String { get; set; } = null;
    }
}