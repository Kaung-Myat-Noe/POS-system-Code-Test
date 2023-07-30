namespace pos.sys.Models
{

    public class ResponseModel
    {
        public int HttpStatusCode { get; set; }
        public string RefNo { get; set; }
        public dynamic Data { get; set; }
        public BaseRespError Error { get; set; }
    }

    public class BaseResp
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
    public static class SuccessCode
    {
        public static BaseResp Success { get { return new BaseResp { Message = "Success." }; } }
        public static BaseResp ValidCoupon { get { return new BaseResp {Code ="200",  Message = "Valid." }; } }
        public static BaseResp InValidCoupon { get { return new BaseResp { Code = "400", Message = "InValid." }; } }
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
        public static BaseRespError NoRecordFound { get { return new BaseRespError { Code = "1013", Message = "No Record Found." }; } }
        public static BaseRespError WrongUserNameAndPass{ get { return new BaseRespError { Code = "1000", Message = "Email or Password is incorrect." }; } }
        public static BaseRespError Duplicate { get { return new BaseRespError { Code = "1011", Message = "Duplicate Data." }; } }
        public static BaseRespError NoRowsAffected { get { return new BaseRespError { Code = "1012", Message = "No Rows Affected." }; } }
        public static BaseRespError ClientCodeNotFound { get { return new BaseRespError { Code = "1012", Message = "Client code was not found." }; } }
        public static BaseRespError GateNotExist { get { return new BaseRespError { Code = "1012", Message = "Gateway does not exist." }; } }
        public static BaseRespError ProductNotExist { get { return new BaseRespError { Code = "1012", Message = "Product does not exist" }; } }
        public static BaseRespError TelcoCodeListNull{ get { return new BaseRespError { Code = "1012", Message = "Telco Code should not be null" }; } }
        public static BaseRespErrorDetail OperationError { get { return new BaseRespErrorDetail { ErrorCode = "SYS001", ErrorDescription = "Indicate unknown exception" }; } }
    }
}

