using pos.sys.Entities;
using System.Runtime.Serialization;

namespace pos.sys.Models
{
    public class ConfigModel
    {
        public string? ProductID { get; set; }
        public string? TelcoCode { get; set; }
        public string? ClientCode { get; set; }
        public string? Description { get; set; }
        public string? TokenUserName { get; set; }
        public string? UAT_SenderID { get; set; }
        public string? Pro_SenderID { get; set; }
        public string? Requester { get; set; }
        public DateTime? RequestDateTime { get; set; }
        public DateTime? TargetDateTime { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Department { get; set; }
        public string? Gateway { get; set; }
        public string? ServiceURL1 { get; set; }
        public string? ServiceURL2 { get; set; }
        public string? ServiceURL3 { get; set; }
        public int? Timeout { get; set; }
        public string? SecretID { get; set; }
        public string? CheckDuplicate { get; set; }
        public string? SecretKey { get; set; }
        public string? Route_to_env { get; set; }
        public string? clientid { get; set; }
        public string? clientsecret{ get; set; }
    }
    public class ConfigModelChecker
    {
        public string? ProductID { get; set; }
        public string? ClientCode { get; set; }
        public string? TokenUserName { get; set; }
        public string? UAT_SenderID { get; set; }
        public string? Pro_SenderID { get; set; }
        public string? Description{ get; set; }
        public string? Requester { get; set; }
        public DateTime? RequestDateTime { get; set; }
        public DateTime? TargetDateTime { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Department { get; set; }
        public string? Gateway { get; set; }
        public string? ServiceURL1 { get; set; }
        public string? ServiceURL2 { get; set; }
        public string? ServiceURL3 { get; set; }
        public string? SecretID { get; set; }
        public string? CheckDuplicate { get; set; }
        public string? SecretKey { get; set; }
        public string? Route_to_env { get; set; }
        public string? clientid { get; set; }
        public string? clientsecret { get; set; }
    }
    //ProductID = p.PRODUCTID,
    //                                         ClientCode = p.CLIENTCODE,
    //                                         CheckDuplicate = p.CHECKDUPLICATE,
    //                                         TokenUserName = p.TOKENUSERNAME,
    //                                         UAT_SenderID = p.UAT_SENDERID,
    //                                         Pro_SenderID = p.PRO_SENDERID,
    //                                         Department = p.DEPARTMENT,
    //                                         Gateway = g.GATEWAY,
    //                                         ServiceURL1 = g.SERVICEURL1,
    //                                         ServiceURL2 = g.SERVICEURL2,
    //                                         ServiceURL3 = g.SERVICEURL3,
    //                                         SecretID = p.SECRETID,
    //                                         SecretKey = p.SECRETKEY
    public class ProductConfig
    {
        public string? PRODUCTID { get; set; }
        public string CLIENTCODE { get; set; }
        public string TOKENUSERNAME { get; set; }
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
        public string GateWay { get; set; }
        public string? OPERATOR { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public filter Filter { get; set; }
    }
    public class ProductUpdateConfig
    {
        public string? PRODUCTID { get; set; }
        public string CLIENTCODE { get; set; }
        public string TOKENUSERNAME { get; set; }
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
        public string GateWay { get; set; } = null;
        public string? OPERATOR { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public filter Filter { get; set; }
    }

    public class ProductInfoUpdateConfig
    {
        public string? PRODUCTID { get; set; }
        public string CLIENTCODE { get; set; }
        public string TOKENUSERNAME { get; set; }
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
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public filter Filter { get; set; }
    }
    public class ProductSearchBy 
    {
        public string? TELCOCODE { get; set; }
        public string? CLIENTCODE{ get; set; }
    }
    public class ProductUpdateConfigByTelco
    {
        public string? TELCOCODE { get; set; }
        public string? GATEWAY { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public filter Filter { get; set; }
    }
    public class ClientCodeList
    {
        public IList<string> CLIENTCODES{ get; set; }
        public IList<string>? TELCOLIST { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public string? GATEWAY { get; set; }
        public filter Filter { get; set; }

    }

    public class filter
    {
        public int UpdateFilter { get; set; }
        public string? CreateDataWith { get; set; }
    }

    public class GatewayData
    {
        public string GATEWAY { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<string> CLIENTCODE { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int COUNT { get; set; }
    }

    public class ProductDelete
    {
        public string CLIENTCODE { get; set; }
        public string TOKENUSERNAME { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
    }

    public class ProductFilter
    {
        public string SearchText { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class TransactionFilter
    {
        public string SearchText { get; set; }
        public DateTime Date{ get; set; }
        public Pagination Pagination { get; set; }
    }
    public class SMSMonthlyCountRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Gateway{ get; set; }
    }

    public class Gates
    {
        public string ClientCode { get; set; }
        public string Gateway { get; set; }
        public int Count { get; set; }
    }

    public class SMSMonthlyCountResponse
    {
        public string ClientCode { get; set; }
        public List<Gates> Gateways { get; set; }
    }

    public class ProductFilterByProductID
    {
        public string PRODUCTID { get; set; }
        public string OPERATOR { get; set; }
    }
    public class GateWayProductInfo
    {
        public string GATEWAY { get; set; }
    }
    public class Pagination
    {
        public int NEXT_INDEX { get; set; }
        public int PAGE_TOTAL { get; set; }
    }
    public class MetaData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
    public class ListData
    {
        public IList<ProductConfigSearch> Products;
        public int ProductCount;
    }
    public class ListDataByClientCode
    {
        public string ClientCode { get; set; }
        public IList<ProductConfigSearch> Product { get; set; }
    }
    public class ListSMSRecordData
    {
        public int ProductCount;
        public IList<SMS_CONFIG_RECORDS> Records { get; set; }
    }
    public class SMSCountListModel
    {
        public string? ClientCode { get; set; }
        public GatewayCount? Gateway{ get; set; }
    }
    public class SMSFinalCountListModel
    {
        public string? ClientCode { get; set; }
        public IList<GatewayCount>? Gateway { get; set; }
    }
    public class GatewayCount
    {
        public string? Gateway { get; set; }
        public int? SMSCount { get; set; }
    }
    public class ListConfigByClientCode
    {
        public string ClientCode { get; set; }
        public IList<Product> Product { get; set; }
    }
    public class ListDataByClientCodeOneProduct
    {
        public string ClientCode { get; set; }
        public ProductConfigSearch Product { get; set; }
        public int ProductCount;
    }
    public class ProductConfigSearch
    {
        public string? PRODUCTID { get; set; }
        public string CLIENTCODE { get; set; }
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
        public string? GateWay { get; set; }
        public string? OPERATOR { get; set; }
    }

    public class SMS_Records
    {
        public string CONFIG_RECORD_ID { get; set; }
        public string? RECORD_TYPE { get; set; }
        public string? GATWEWAY { get; set; }
        public string? CLIENTCODE { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? NEW_CONFIG { get; set; }
        public string? OLD_CONFIG { get; set; }
        public DateTime? RECORD_DATE { get; set; }
    }
}
