using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("TELCO_SMS_BULK_TRAN")]
    public class TransactionBulk
    {
        public string? MERCHANT_ID { get; set; }
        public string CLIENT_CODE { get; set; }
        public string TRN_REF_NO { get; set; }
        public DateTime? REQUEST_DATETIME { get; set; }
        public DateTime? RESPONSE_DATETIME { get; set; }
        public string? MESSAGE { get; set; }
        public string? REQUEST { get; set; }
        public string? RESPONSE { get; set; }
        public string? MERCHANT_REQUEST { get; set; }
        public string? MERCHANT_RESPONSE { get; set; }
        public string? BUS_IP { get; set; }
        public string? BUS_SERVER { get; set; }
        public string? PROCESS_STAGE { get; set; }
        public string? SUBSCRIBER_NO { get; set; }
        public DateTime? M_RESP_DATETIME { get; set; }
        public DateTime? M_REQ_DATETIME { get; set; }

    }
}
