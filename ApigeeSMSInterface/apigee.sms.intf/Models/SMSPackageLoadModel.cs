using Newtonsoft.Json;

namespace apigee.sms.intf.Models
{
    public class SMSPackageLoadModel
    {
        public string? ClientCode { get; set; }
        public string? CheckDuplicate { get; set; }
        public string? TelcoCode { get; set; }
        public string? TokenUserName { get; set; }
        public string? UAT_SenderID { get; set; }
        public string? Pro_SenderID { get; set; }
        public string? Department { get; set; }
        public string? Gateway { get; set; }
        public string? ServiceURL1 { get; set; }
        public string? ServiceURL2 { get; set; }
        public string? ServiceURL3 { get; set; }
        public int? Timeout { get; set; }
        public string? SecretID { get; set; }
        public string? SecretKey { get; set; }
        public string? Route_to_env { get; set; }
        public string? ClientID { get; set; }
        public string? ClientSecret{ get; set; }
        public DateTime? ModifiedDate { get; set; } = System.DateTime.Now;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Provider { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriberNum { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TrxnRefNum { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Msg_type { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DEPARTMENT { get { return Department; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? SENDERNAME
        {
            get; set;
        }
    }

    public class Product
    {
        public string? PRODUCTID { get; set; }
        public string? CLIENTCODE { get; set; }
        public string? TOKENUSERNAME { get; set; }
        public string? UAT_SENDERID { get; set; }
        public string? PRO_SENDERID { get; set; }
        public string? DEPARTMENT { get; set; }
        public string? DESCRIPTION { get; set; }
        public string? REQUESTER { get; set; }
        public DateTime? REQUESTDATETIME { get; set; }
        public DateTime? TARGETDATETIME { get; set; }
        public DateTime? APPROVEDDATETIME { get; set; }
        public string? APPROVEDBY { get; set; }
        public string? CHECKDUPLICATE { get; set; }
        public string? SECRETID { get; set; }
        public string? SECRETKEY { get; set; }
        public string? ROUTE_TO_ENV { get; set; }
        public string? CLIENTID { get; set; }
        public string? CLIENTSECRET { get; set; }
        public string GATEWAY{ get; set; }
    }
}
