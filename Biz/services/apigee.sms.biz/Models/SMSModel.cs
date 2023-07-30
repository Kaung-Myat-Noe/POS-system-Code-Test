using apigee.sms.biz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace apigee.sms.biz.Models
{
    public class RequestSMSModel
    {
        public string TelcoCode { get; set; }
        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string ClientCode { get; set; }
        public string Msg_type { get; set; }
    }    
    public class ResponseTelcoSMSModel
    {
        public string TrxnRefNum { get; set; }
        public List<ResponseModel> Response { get; set; }
    }    
    public class TelcoSMSModel
    {
        public string TELCO_CODE { get; set; }
        public string MERCHANT_ID { get; set; }
        public string CLIENT_CODE { get; set; }
        public string SMS_MESSAGE { get; set; }
        public string PROCESS_STAGES { get; set; }
        public DateTime REQUEST_DATETIME { get; set; }
        public DateTime RESPONSE_DATETIME { get; set; }
        public string TRN_REF_NO { get; set; }
        public string REQUEST { get; set; }
        public string RESPONSE { get; set; }
        public string MERCHANT_TRN_NO { get; set; }
        public string MERCHANT_REQUEST { get; set; }
        public string MERCHANT_RESPONSE { get; set; }
        public string SUBSCRIBER_NO { get; set; }
        public string SMS_COUNT { get; set; }
        public string TELCO_REF_NO { get; set; }
        public string MESSAGE_ID { get; set; }
        public string CHECKVALIDATE { get; set; }
        public string BUS_IP { get; set; }
        public string BUS_SERVER { get; set; }
    }    
    public class ResponseQueryLogModel
    {
        public List<Object> Data { get; set; }
        public List<ResponseModel> Response { get; set; }
    }

    public class RequestMGateModel
    {
        public string scheme { get; set; }
        public string para { get; set; }
        public string header { get; set; }
        public string checkvalidate { get; set; }
        public string loggedby { get; set; }
        public string url { get; set; }
        public string bus_ip { get; set; }
        public string bus_server { get; set; }
        public SMSPackageLoadModel request_obj { get; set; }
        public dynamic json_data { get; set; }        

        public string SecretID { get; set; }
        public string SecretKey { get; set; }
    }

    public class RequestMITModel
    {
        public int PRIORITY { get; set; }
        public RequestSMSModel request_obj { get; set; }
    }



    public class ReqCallBackModel
    {
        public string message_id { get; set; }
        public string phone { get; set; }
        public string sender { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public DateTime timestamp { get; set; }
        public decimal sms_count { get; set; }

    }
    public class ReqBulkSMSModel
    {
        public string SubscriberNum { get; set; }
        public string Message { get; set; }
        public string TrxnRefNum { get; set; }
        public string ClientCode { get; set; }
    }

    public class Telco_Bulk_SMS_Tran
    {
        public string TELCO_REF_NO { get; set; }
        public string TELCO_CODE { get; set; }
        public string MERCHANT_ID { get; set; }
        public string CLIENT_CODE { get; set; }
        public string TRN_REF_NO { get; set; }
        public DateTime REQUEST_DATETIME { get; set; }
        public DateTime RESPONSE_DATETIME { get; set; }
        public string MESSAGE { get; set; }
        public string REQUEST { get; set; }
        public string SMS_MESSAGE { get; set; }
        public string PROCESS_STAGES { get; set; }
        public string RESPONSE { get; set; }
        public string MERCHANT_REQUEST { get; set; }
        public string MERCHANT_RESPONSE { get; set; }
        public string BUS_IP { get; set; }
        public string BUS_SERVER { get; set; }
        public string PROCESS_STAGE { get; set; }
        public string SUBSCRIBER_NO { get; set; }
        public DateTime M_REQ_DATETIME { get; set; }
        public DateTime M_RESP_DATETIME { get; set; }
        public string CHECKVALIDATE { get; set; }
    }

}