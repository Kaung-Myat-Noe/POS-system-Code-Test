﻿namespace apigee.sms.biz.Models
{
    public class AuditLogModel
    {
        public string AuditLogID { get; set; }

        public DateTime LoggedDate { get; set; }

        public object PayLoad { get; set; }

        public string LoggedBy { get; set; }

        public PayLoadType PayLoadType { get; set; }

        public string TransactionRefNo { get; set; }

        public string KBZMessageID { get; set; }

        public string SourceUrl { get; set; }

        public string CurrentUrl { get; set; }

        public HttpVerbs HttpVerb { get; set; }

        public HttpStatusCode HttpCode { get; set; }

        public string Message { get; set; }

        public LogLevel LogLevel { get; set; }

        public string Exception { get; set; }

        public string ServiceCategory { get; set; }

        public string ServiceName { get; set; }

        public string ServiceStatus { get; set; }

        public string SupportKey { get; set; }

        public string ResponseCode { get; set; }
    }
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }
    public enum HttpVerbs
    {
        POST,
        GET,
        PUT,
        PATCH,
        DELETE
    }
    public enum PayLoadType
    {
        REQUEST,
        RESPONSE
    }
    public enum HttpStatusCode
    {

        Continue = 100,

        SwitchingProtocols = 101,

        OK = 200,

        Created = 201,

        Accepted = 202,
        
        NonAuthoritativeInformation = 203,
        
        NoContent = 204,
        
        ResetContent = 205,
        
        PartialContent = 206,
        
        MultipleChoices = 300,
        
        Ambiguous = 300,
        
        MovedPermanently = 301,
        
        Moved = 301,
        
        Found = 302,
        
        Redirect = 302,
        
        SeeOther = 303,
        
        RedirectMethod = 303,
        
        NotModified = 304,
        
        UseProxy = 305,
        
        Unused = 306,

        TemporaryRedirect = 307,

        RedirectKeepVerb = 307,

        BadRequest = 400,

        Unauthorized = 401,

        PaymentRequired = 402,

        Forbidden = 403,

        NotFound = 404,

        MethodNotAllowed = 405,

        NotAcceptable = 406,

        ProxyAuthenticationRequired = 407,

        RequestTimeout = 408,

        Conflict = 409,

        Gone = 410,

        LengthRequired = 411,

        PreconditionFailed = 412,

        RequestEntityTooLarge = 413,

        RequestUriTooLong = 414,

        UnsupportedMediaType = 415,

        RequestedRangeNotSatisfiable = 416,

        ExpectationFailed = 417,

        UpgradeRequired = 426,

        InternalServerError = 500,

        NotImplemented = 501,

        BadGateway = 502,

        ServiceUnavailable = 503,

        GatewayTimeout = 504,

        HttpVersionNotSupported = 505
    }
}