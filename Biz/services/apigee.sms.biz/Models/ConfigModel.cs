using System.ComponentModel.DataAnnotations.Schema;

namespace apigee.sms.biz.Models
{
    public class RespConfigModel
    {
        public string KBZRefNo { get; set; }
        public List<ConfigModel> Data { get; set; }
        public BaseRespError Error { get; set; }
    }
    public class ConfigModel
    {
        public string? Operator { get; set; }
        public string? ClientCode { get; set; }
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
        public RoutesModel ROUTES { get; set; }
        public LogModel LOG { get; set; }
        public SMS_ServicesModel SMS_SERVICE { get; set; }
        public PREFIXModel PREFIX { get; set; }
        public SettingModel SETTING { get; set; }
        public string SMS_ON { get; set; }
    }
    public class SettingModel
    {
        public string CHECKDUPLICATE { get; set; }
        public string FILTER { get; set; }
        public string BUS_IP { get; set; }
        public string BUS_SERVER { get; set; }
        public int TOTAL_MOBILE { get; set; }
    }
    public class RoutesModel
    {
        public LayerModel Business { get; set; }
    }

    public class LayerModel
    {
        public int TIMEOUT { get; set; }
        public List<RouteModel> ROUTE { get; set; }
    }

    public class RouteModel
    {
        public string CHANNEL { get; set; }
        public string SYSURL { get; set; }
        public string CHECKVALIDATE { get; set; }
        public string ROUTE_TO_ENV { get; set; }
    }

    public class LogModel
    {
        public ProjectLogModel Business { get; set; }
        public ProjectLogModel System { get; set; }
    }
    public class ProjectLogModel
    {
        public string TEXTLOG { get; set; }
        public string APILOG { get; set; }
        public string TEXT_URL { get; set; }
        public string API_URL { get; set; }
    }

    public class SubscribersModel
    {
        public string TS { get; set; }
        public List<SubscriberModel> Data { get; set; }
    }
    public class SubscriberModel
    {
        public string TELCO_CODE { get; set; }
        public string CLIENTCODE { get; set; }
        public string SENDERNAME { get; set; }
        public string SERVICE { get; set; }
        public string API_USER { get; set; }
        public string DEPARTMENT { get; set; }
        public string DESCRIPTION { get; set; }
    }

    public class SMS_ServicesModel
    {
        public MGATE_SERVICEModel MGATE { get; set; }
    }
    public class MGATE_SERVICEModel
    {
        public List<ENVIRONMENTModel> ENVIRONMENT { get; set; }
        public string REQ_FORMAT_VALUE { get; set; }
        public string HTTP_METHOD { get; set; }
        public int TIMEOUT { get; set; }
    }
    public class ENVIRONMENTModel
    {
        public string ENV { get; set; }
        public string SERVICE_URL { get; set; }
        public string BULKSMS_SERVICEURL { get; set; }
    }
    public class PREFIXModel
    {
        public string MPT { get; set; }
        public string TELENOR { get; set; }
        public string OOREDOO { get; set; }
        public string MYTEL { get; set; }
        public string MEC { get; set; }
    }
    public class ProductSearchBy
    {
        public string? TELCOCODE { get; set; }
        public string? CLIENTCODE { get; set; }
    }
    public class SMSMonthlyCount
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Gateway { get; set; }
    }
    public class ProductConfig
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
    public class ProductConfigDelete
    {
        public string? CLIENTCODE { get; set; }
    }
    public class ProductFilter
    {
        public string SearchText { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class TransactionFilter
    {
        public string SearchText { get; set; }
        public DateTime Date { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class GateWayProductInfo
    {
        public string GATEWAY { get; set; }
    }
    public class ClientCodeList
    {
        public IList<string> CLIENTCODES { get; set; }
        public IList<string>? TELCOLIST { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public string? GATEWAY { get; set; }
        public filter Filter { get; set; }

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
    public class GateWay
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
}
