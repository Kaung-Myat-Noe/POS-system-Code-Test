namespace apigee.sms.intf.Models
{
    public class ResponseModel
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string KBZRefNo { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public object Data { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public BaseRespError Error { get; set; }
    }

    public class BaseRespError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public IList<BaseRespErrorDetail> Details { get; set; }

        public BaseRespError()
        {
            Details = new List<BaseRespErrorDetail>();
        }
    }

    public class BaseRespErrorDetail
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class BaseResp
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public static class SuccessCode
    {
        public static BaseResp Success { get { return new BaseResp { Code = "1000", Message = "Success." }; } }
        public static BaseResp RefreshSuccess{ get { return new BaseResp { Code = "200", Message = "Config Refresh Successfully." }; } }
        public static BaseResp RefreshFail{ get { return new BaseResp { Code = "1004", Message = "Config Refresh Fail!" }; } }
    }

    public static class ErrorCode
    {
        public static BaseRespError UnknownException { get { return new BaseRespError { Code = "1004", Message = "Unknown error." }; } }
        public static BaseRespError DependencyError { get { return new BaseRespError { Code = "1004", Message = "Depedency error. Please check the depedency configuration in app setting file." }; } }
        public static BaseRespError NoRecordFound { get { return new BaseRespError { Code = "400", Message = "No Records Found!" }; } }
        public static BaseRespError Unauthorized { get { return new BaseRespError { Code = "1000", Message = "Unauthorized." }; } }
        public static BaseRespError DatabaseError { get { return new BaseRespError { Code = "1015", Message = "Database Error." }; } }
        public static BaseRespError CachingError { get { return new BaseRespError { Code = "500", Message = "Caching Error." }; } }
        public static BaseRespError FilterOutOfRange { get { return new BaseRespError { Code = "400", Message = "Filter parameter is not acceptable" }; } }
        public static BaseRespError FilterEmpty { get { return new BaseRespError { Code = "400", Message = "Please Add Filter Parameter" }; } }
        public static BaseRespErrorDetail OperationError { get { return new BaseRespErrorDetail { ErrorCode = "B001", ErrorDescription = "Indicate unknown exception in System Layer" }; } }
        public static BaseRespErrorDetail PhonePrefixEmpty { get { return new BaseRespErrorDetail { ErrorCode = "400", ErrorDescription = "No Record Found for Phone prefix" }; } }
        public static BaseRespErrorDetail ProductEmpty { get { return new BaseRespErrorDetail { ErrorCode = "400", ErrorDescription = "No Record Found for Product" }; } }
        public static BaseRespError ClientCodeError { get { return new BaseRespError { Code = "400", Message = "Product with current subscriber Doesn't Exist or token is not valid!" }; } }
        public static BaseRespError ProviderError { get { return new BaseRespError { Code = "400", Message = "Provider Empty" }; } }

        public static BaseRespError CacheError { get { return new BaseRespError { Code = "400", Message = "Please enter correct phone number" }; } }
        public static BaseRespError RefreshConfigFailed{ get { return new BaseRespError { Code = "400", Message = "Error While Refreshing Config" }; } }

        public static BaseRespError InvalidSubscriberError { get { return new BaseRespError { Code = "400", Message = "Invalid Subscriber" }; } }
        public static BaseRespError InvalidPhoneNumberError{ get { return new BaseRespError { Code = "400", Message = "Phone Number/s Invalid!" }; } }

        public static BaseRespError InvalidGateway { get { return new BaseRespError { Code = "400", Message = "Gateway is incorrect for current client" }; } }
        public static BaseRespError NoClientCode { get { return new BaseRespError { Code = "400", Message = "Client Code must not be null or empty" }; } }
        public static BaseRespError NoTelcoOrClient { get { return new BaseRespError { Code = "400", Message = "Telco Code and Client Code is null" }; } }
        public static BaseRespError BothTelcoOrClient { get { return new BaseRespError { Code = "400", Message = "Please choose either Telco Code or Client Code" }; } }
        public static BaseRespError NoParameter{ get { return new BaseRespError { Code = "400", Message = "Parameter is empty. Please check!" }; } }
        public static BaseRespError PhoneNumberNullError { get { return new BaseRespError { Code = "400", Message = "Phone Number is null" }; } }
        public static BaseRespError WrongEnv { get { return new BaseRespError { Code = "400", Message = "Wrong Environment" }; } }
        public static BaseRespError ProvierAreDifferent { get { return new BaseRespError { Code = "400", Message = "Subscriber providers are different" }; } }


    }

    public class BaseRespModel
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string KBZRefNo { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public List<BaseResp> Data { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public List<BaseRespError> Error { get; set; }
    }

    public class ServiceModel
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public bool? ScheduleTime { get; set; }
    }
}
