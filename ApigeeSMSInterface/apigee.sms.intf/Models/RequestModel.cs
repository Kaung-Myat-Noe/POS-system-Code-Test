namespace apigee.sms.intf.Models
{
    public class SMS_Send_Request_Model
    {

        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string ClientCode { get; set; }
        public string? Msg_type { get; set; } = "";
        public string? TelcoCode { get; set; }

    }
    public class SMS_Send_Request_GATEWAY_Model
    {

        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string ClientCode { get; set; }
        public string? Msg_type { get; set; } = "";
        public string? TelcoCode { get; set; }
        public string? Gateway { get; set; } = "";

    }
    public class Update_Request_Model
    {

        public string? TELCO_CODE { get; set; }
        public string Provider { get; set; }
        public string? CLIENTCODE { get; set; }

    }
}
