using Newtonsoft.Json;

namespace apigee.sms.intf.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SMSClientModel
    {
        /// <summary>
        /// Teleco Code
        /// </summary>
        [JsonProperty("TelcoCode")]
        public string TelcoCode { get; set; } = null;

        /// <summary>
        /// Subscriber Number
        /// </summary>
        [JsonProperty("SubscriberNum")]
        public string SubscriberNum { get; set; } = null;

        /// <summary>
        /// Provider
        /// </summary>
        [JsonProperty("Provider")]
        public string Provider { get; set; } = null;

        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty("Message")]
        public string Message { get; set; } = null;

        /// <summary>
        /// Transaction Reference Number
        /// </summary>
        [JsonProperty("TrxnRefNum")]
        public string TrxnRefNum { get; set; } = null;

        /// <summary>
        /// Client Code (Product)
        /// </summary>
        [JsonProperty("ClientCode")]
        public string ClientCode { get; set; } = null; // by product

        /// <summary>
        /// Message Type (Optional)
        /// </summary>
        [JsonProperty("Msg_type")]
        public string Msg_type { get; set; } = null;

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; } = null;

        /// <summary>
        /// Teleco Code
        /// </summary>
        [JsonProperty("TELCO_CODE")]
        private string TelcoCode2 { set { TelcoCode = value; } }

        /// <summary>
        /// Client Code (Product)
        /// </summary>
        [JsonProperty("CLIENTCODE")]
        private string ClientCode2 { set { ClientCode = value; } }// by product

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("SENDERNAME")]
        private string? SenderName { get; set; } = null;

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("SERVICE")]
        private string? SERVICE { get; set; } = null;

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("API_USER")]
        public string? API_USER { get; set; } = null;

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("DEPARTMENT")]
        private string? DEPARTMENT { get; set; } = null;

        /// <summary>
        /// Modified Date (Optional)
        /// </summary>
        [JsonProperty("DESCRIPTION")]
        private string? DESCRIPTION { get; set; } = null;

        /// <summary>
        /// Phone Operator
        /// </summary>
        [JsonProperty("Operator")]
        private string? Operator { get; set; } = null;
        
        /// <summary>
        /// UAT Sender ID
        /// </summary>
        [JsonProperty("UAT_SenderID")]
        private string? UAT_SenderID { get; set; } = null;

        /// <summary>
        /// Production Sender ID
        /// </summary>
        [JsonProperty("Pro_SenderID")]
        private string? Pro_SenderID { get; set; } = null;

        /// <summary>
        /// Gateway
        /// </summary>
        [JsonProperty("Gateway")]
        private string? Gateway { get; set; } = null;

        /// <summary>
        /// ServiceURL1
        /// </summary>
        [JsonProperty("ServiceURL1")]
        private string? ServiceURL1 { get; set; } = null;

        /// <summary>
        /// ServiceURL2
        /// </summary>
        [JsonProperty("ServiceURL2")]
        private string? ServiceURL2 { get; set; } = null;

        /// <summary>
        /// ServiceURL3
        /// </summary>
        [JsonProperty("ServiceURL3")]
        private string? ServiceURL3 { get; set; } = null;

        /// <summary>
        /// Timeout
        /// </summary>
        [JsonProperty("Timeout")]
        private string? Timeout { get; set; } = null;

        /// <summary>
        /// SecretID
        /// </summary>
        [JsonProperty("SecretID")]
        private string? SecretID { get; set; } = null;

        /// <summary>
        /// SecretKey
        /// </summary>
        [JsonProperty("SecretKey")]
        private string? SecretKey { get; set; } = null;

        /// <summary>
        /// SecretKey
        /// </summary>
        [JsonProperty("Route_to_env")]
        public string? ROUTE_TO_ENV { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        [JsonProperty("ClientID")]
        public string? CLIENTID { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        [JsonProperty("ClientSecret")]
        public string? CLIENTSECRET { get; set; }
        /// <summary>
        /// SecretKey
        /// </summary>
        [JsonProperty("CheckDuplicate")]
        public string? CHECKDUPLICATE { get; set; }
    }
}
