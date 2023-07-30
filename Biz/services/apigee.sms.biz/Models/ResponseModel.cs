namespace apigee.sms.biz.Models
{
    public class ResponseModel
    {
        public string? KBZRefNo { get; set; }
        public object? Data { get; set; }
        public BaseRespError Error { get; set; }
    }
    public class ResponseModelForUtil {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class BaseResp
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
    public static class SuccessCode
    {
        public static BaseResp Success { get { return new BaseResp { Message = "Success." }; } }
    }
    public class BaseRespError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public IList<BaseRespErrorDetail> Details { get; set; }

        public BaseRespError()
        {
            Details = new List<BaseRespErrorDetail>();
        }
    }

    public class BaseRespErrorDetail
    {
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
    }

    public static class ErrorCode
    {
        public static BaseRespError Unauthorized { get { return new BaseRespError { Code = "1000", Message = "Unauthorized." }; } }
        public static BaseRespError UnknownException { get { return new BaseRespError { Code = "1004", Message = "Unknown Error" }; } }        
        public static BaseRespErrorDetail OperationError { get { return new BaseRespErrorDetail { ErrorCode = "SYS001", ErrorDescription = "Indicate unknown exception" }; } }
    }
    public class ErrorResponseModel
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
    public static class ReturnMessage
    {
        public static ResponseModelForUtil Success { get { return new ResponseModelForUtil { Code = "103002000", Description = "Success" }; } }
        public static ResponseModelForUtil Pending { get { return new ResponseModelForUtil { Code = "1003", Description = "Pending." }; } }
        public static ErrorResponseModel SenderNameNotFound { get { return new ErrorResponseModel { Code = "1001", Message = "Sender Name is not found." }; } }
        public static ErrorResponseModel CheckMandatory { get { return new ErrorResponseModel { Code = "1001", Message = "[FIELD] is should not be blank." }; } }
        public static ErrorResponseModel DuplicateTranRefNo { get { return new ErrorResponseModel { Code = "1011", Message = "TrxnRefNum is Duplicated." }; } }
        public static ErrorResponseModel NoRoute { get { return new ErrorResponseModel { Code = "1004", Message = "Route not found for current client." }; } }
        public static ErrorResponseModel InvalidMobileNo { get { return new ErrorResponseModel { Code = "1001", Message = "Invalid MobileNo." }; } }
        public static ErrorResponseModel NeedtoSubscribe { get { return new ErrorResponseModel { Code = "1001", Message = "Need to subscribe operator." }; } }
        public static ErrorResponseModel TelcoTimeOut { get { return new ErrorResponseModel { Code = "1002", Message = "Transaction Timeout in Telco." }; } }
        public static ErrorResponseModel MicroException { get { return new ErrorResponseModel { Code = "1004", Message = "Indicate unknown exception in System Layer." }; } }
        public static ErrorResponseModel BusinessException { get { return new ErrorResponseModel { Code = "1004", Message = "Indicate unknown exception in Business Layer." }; } }
        public static ErrorResponseModel MicroTimeOut { get { return new ErrorResponseModel { Code = "1002", Message = "System layer Timeout." }; } }
        public static ErrorResponseModel BusinessTimeOut { get { return new ErrorResponseModel { Code = "1002", Message = "Business layer Timeout." }; } }
        public static ErrorResponseModel InvalidUserName_ClientCode { get { return new ErrorResponseModel { Code = "1001", Message = "Invalid UserName or ClientCode" }; } }
        public static ResponseModelForUtil KBZ_REFERENCE { get { return new ResponseModelForUtil { Code = "KBZREF", Description = "{0}" }; } }
        public static ErrorResponseModel Unauthorized { get { return new ErrorResponseModel { Code = "1000", Message = "Unauthorized" }; } }
        public static ErrorResponseModel ExceedMobile { get { return new ErrorResponseModel { Code = "103002008", Message = "Total mobile no exceed. [FIELD]" }; } }
    }
}
