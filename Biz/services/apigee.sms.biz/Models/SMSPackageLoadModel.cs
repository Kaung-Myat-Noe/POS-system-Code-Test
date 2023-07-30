using Newtonsoft.Json;

namespace apigee.sms.biz.Models
{
    public class SMSPackageLoadModel
    {
        public string? ClientCode { get; set; }
        public string? CheckDuplicate { get; set; }
        public string? TelcoCode { get; set; }
        public string? TokenUserName { get; set; }
        public string? UAT_SenderID { get; set; }
        public string? Pro_SenderID { get; set; }
        public string? SENDERNAME
        {
            get; set;
        }
        //public string? Department { get; set; }
        public string? DEPARTMENT{ get; set; }
        public string? Gateway { get; set; }
        public string? ServiceURL1 { get; set; }
        public string? ServiceURL2 { get; set; }
        public string? ServiceURL3 { get; set; }
        public int? Timeout { get; set; }
        public string? SecretID { get; set; }
        public string? SecretKey { get; set; }
        public string? Route_to_env { get; set; }
        public string? clientid { get; set; }
        public string? clientsecret { get; set; }
        public DateTime? ModifiedDate { get; set; } = System.DateTime.Now;
        public string? Provider { get; set; } = null;
        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string? Msg_type { get; set; }
    }
    public class SMS_Send_Request_Model
    {

        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string ClientCode { get; set; }
        public string Msg_type { get; set; }

    }
}
