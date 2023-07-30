namespace apigee.sms.intf.Models
{
    public class ProductConfig
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
    public class ClientCodeList
    {
        public IList<string> CLIENTCODES { get; set; }
        public IList<string> TELCOLIST { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public string? GATEWAY { get; set; }
        public filter Filter { get; set; }

    }
    public class GateWayProductInfo
    {
        public string GATEWAY { get; set; }
    }
    public class ProductUpdateConfigByTelco
    {
        public string? TELCOCODE { get; set; }
        public string? GATEWAY { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public filter Filter { get; set; }
    }
    public class filter
    {
        public int UpdateFilter { get; set; }
        public string? CreateDataWith { get; set; }
    }
    public class ProductSearchBy
    {
        public string? TELCOCODE { get; set; }
        public string? CLIENTCODE { get; set; }
    }
    public class FilterByClientCode
    {
        public int UpdateFilter { get; set; }
        public string? CreateDataWith { get; set; }
        public string CLIENTCODE { get; set; }
    }
    public class ProductDelete
    {
        public string CLIENTCODE { get; set; }
        public string? TOKENUSERNAME { get; set; }
        public filter Filter { get; set; }
    }
    public class ProductFilter
    {
        public string SearchText { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class SMSMonthlyCount
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Gateway { get; set; }
    }
    public class TransactionFilter
    {
        public string SearchText { get; set; }
        public DateTime Date { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class ProductFilterByProductID
    {
        public string PRODUCTID { get; set; }
        public string OPERATOR { get; set; }
    }
    public class Pagination
    {
        public int NEXT_INDEX { get; set; }
        public int PAGE_TOTAL { get; set; }
    }
    public class GetConfigByClientCode
    {
        public string Clietn_Code { get; set; }
    }
    public class CacheUpdateModel
    {
        public string ClientCode { get; set; }
        public string TelcoCode { get; set; }
        public string Provider { get; set; }

    }
}
