using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("SMSGATEWAY")]
    public class Gateway
    {
        public string? GATEWAYID { get; set; }
        public string? GATEWAY { get; set; }
        public string? SERVICEURL1 { get; set; }
        public string? SERVICEURL2 { get; set; }
        public string? SERVICEURL3 { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TIMEOUT { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MAXSUBSCRIBERS { get; set; }
    }

    [Table("SMSPRODUCT")]
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
    }

    [Table("SMSGATEWAY_PRODUCT_MAPPING")]
    public class MAPPING
    {
        public string? MAPPINGID { get; set; }
        public string? OPERATOR { get; set; }
        public string? GATEWAYID { get; set; }
        public string? PRODUCTID { get; set; }
        public DateTime? CREATEDON { get; set; }
        public string? CREATEDBY { get; set; }
        public string? LAST_MODIFIEDON { get; set; }
        public DateTime? LAST_MODIFIEDBY { get; set; }
    }
}
