using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using apigee.sms.biz.Models;
using Newtonsoft.Json;
using apigee.sms.biz.Utilities;
using System.Net.Http.Headers;
using System.Text;
using System.Collections;
using System.Threading;
using System.Web;
using System.Configuration;
using apigee.sms.biz.Services;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using apigee.sms.biz.Common;
using Security = apigee.sms.biz.Utilities.Security;
using NLog;
using NLog.Web;
using Microsoft.Extensions.Options;
using apigee.sms.biz.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace apigee.sms.biz.Controllers
{
    [Route("api/")]
    [ApiController]
    //[TypeFilter(typeof(CustomAuthorizeFilter))]
    public class SMSController : BaseController
    {
        private ILogger<SMSController> logger;
        private AuditLogModel logs = null;
        internal string BaseAddress = null, vendors = null, service;
        internal HttpRequestMessage requestMsg = null;
        internal HttpResponseMessage responseMsg = null;
        internal readonly AppSettings _settings;
        internal ErrorResponseModel err_responseModel;
        internal readonly ISMSBrixServices _smsbrixService;
        internal readonly IServices _services;
        internal readonly IConfigService _cfg;
        const string secret = "t@1c0@p!";
        public SMSController(ILogger<SMSController> nlog, IConfigService cfg, IServices services, ISMSBrixServices smsbrixService, IOptionsMonitor<AppSettings> setting)
        {
            logger = nlog;
            _services = services;
            _settings = setting.CurrentValue;
            _smsbrixService = smsbrixService;
            _cfg = cfg;
            
            //claimuserName = GetClaimUsername(_settings.JwtConfig.ClaimUserNameKey);
            try
            {
                
            }
            catch (Exception ex)
            {
                logger.LogError("CACHE:" + ex.ToString());
            }
        }
        [HttpPost("WrapperSendSMS/", Name = "WrapperSendSMS")]
        public async Task<IActionResult> WrapperSendSMS(SMSPackageLoadModel param)
        {
            SMSPackageLoadModel obj = new SMSPackageLoadModel();
            try
            {
                obj = param;
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                obj.Message = Security.Encrypt(obj.Message, true, secret);
                obj.TrxnRefNum = obj.TrxnRefNum.Trim();
                scheme = authHeader.Scheme;
                parameter = authHeader.Parameter;
                GetClaimUsername();
                AssignLogID();
                string url;
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogInformation("RefID:" + KBZ_REF_NO + ",Request Json:" + JsonConvert.SerializeObject(obj));
                if (obj.Route_to_env != null && obj.Route_to_env.Equals("UAT"))
                {
                    url = obj.ServiceURL3;
                }
                else if (obj.Route_to_env != null && obj.Route_to_env.Equals("PRODUCTION"))
                {
                    if (_settings.BUS_SERVER.Contains("dc"))
                    {
                        url = obj.ServiceURL1;
                    }
                    else
                    {
                        url = obj.ServiceURL2;
                    }
                }
                else
                {
                    url = obj.ServiceURL3;
                }

                //var handler = new JwtSecurityTokenHandler();
                //var jwtSecurityToken = handler.ReadJwtToken(parameter);
                //var Name = jwtSecurityToken.Claims.First(v => v.Type == "user_name").Value;
                if (claimUser == null) {
                    logger.LogError("Ref_ID: " + KBZ_REF_NO + ",TrxnRefNum: " + obj.TrxnRefNum + "Error: CLaim user is null");
                    HttpResponseMessage unauthorizeHttpResponseMessage = new System.Net.Http.HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Content = new StringContent(JsonConvert.SerializeObject(new
                        {
                            KBZRefNo = KBZ_REF_NO,
                            Error = ErrorCode.Unauthorized
                        }),
                        Encoding.UTF8, "application/json")
                    };
                    return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(unauthorizeHttpResponseMessage.Content.ReadAsStringAsync().Result)))
                    { StatusCode = ((int)unauthorizeHttpResponseMessage.StatusCode) };
                }

                RequestMGateModel mgate = new RequestMGateModel
                {
                    scheme = scheme,
                    para = parameter,
                    header = KBZ_REF_NO,
                    loggedby = claimUser, // Token user
                    url = url + "/" + _settings.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE,
                    bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                    bus_server = _settings.BUS_SERVER, // DC or DR
                    request_obj = obj,
                    json_data = new { obj },
                    checkvalidate = obj.CheckDuplicate
                };
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                if (obj.Gateway.Equals("MGate"))
                {
                    if (!obj.SubscriberNum.StartsWith("959"))
                    {
                        if (obj.SubscriberNum.StartsWith("0"))
                        {
                            string cutPhoneNumber = obj.SubscriberNum.Substring(1, obj.SubscriberNum.Length - 1);
                            obj.SubscriberNum = "95" + cutPhoneNumber;
                        }
                        else
                        {
                            obj.SubscriberNum = "95" + obj.SubscriberNum;
                        }
                    }
                    mgate.checkvalidate = obj.CheckDuplicate;

                    httpResponseMessage = _services.mgatesendsms(mgate);
                }
                else if (obj.Gateway.Equals("MGateBulk")) {

                    string[] array = obj.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length > _settings.SETTING.TOTAL_MOBILE)
                    {
                        err_responseModel = ReturnMessage.ExceedMobile;
                        err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", Convert.ToString(_settings.SETTING.TOTAL_MOBILE));
                        return new ObjectResult(JsonConvert.SerializeObject(new { KBZRefNo = KBZ_REF_NO, Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message } }))
                        { StatusCode = ((int)Models.HttpStatusCode.BadRequest) };
                        //return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!array[i].StartsWith("959"))
                        {
                            if (array[i].StartsWith("0"))
                            {
                                string cutPhoneNumber = array[i].Substring(1, array[i].Length-1);
                                array[i] = "95" + cutPhoneNumber;
                            }
                            else
                            {
                                array[i] = "95" + array[i];
                            }
                        }
                    }

                    obj.SubscriberNum = string.Join(",", array);
                    mgate = new RequestMGateModel
                    {
                        scheme = scheme,
                        para = parameter,
                        header = KBZ_REF_NO,
                        loggedby = claimUser, // Token user
                        url = url + "/" + _settings.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE,
                        bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                        bus_server = _settings.BUS_SERVER, // DC or DR
                        request_obj = obj,
                        json_data = new { obj },
                        checkvalidate = obj.CheckDuplicate,
                        SecretID = obj.SecretID,
                        SecretKey = obj.SecretKey
                    };

                    //httpResponseMessage = await Task.FromResult(_services.mgate_bulksms(mgate)).Result;
                }
                else if (obj.Gateway.Equals("SMSBrix"))
                {
                    
                    if (obj.SubscriberNum.StartsWith("959"))
                    {
                        obj.SubscriberNum = "09" + obj.SubscriberNum.Substring(3, obj.SubscriberNum.Length-3);
                    }
                    else
                    {
                        if (!obj.SubscriberNum.StartsWith("09"))
                        {
                            obj.SubscriberNum = "09" + obj.SubscriberNum.Substring(1, obj.SubscriberNum.Length - 1);
                        }
                       
                    }
                    //url = obj.ServiceURL1;

                    mgate = new RequestMGateModel
                    {
                        scheme = scheme,
                        para = parameter,
                        header = KBZ_REF_NO,
                        loggedby = claimUser, // Token user
                        url = url,
                        bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                        bus_server = _settings.BUS_SERVER, // DC or DR
                        request_obj = obj,
                        json_data = new { obj },
                        checkvalidate = obj.CheckDuplicate,
                        SecretID = obj.SecretID,
                        SecretKey = obj.SecretKey
                    };

                    httpResponseMessage = _smsbrixService.smsbrixsendsms(mgate);
                }

                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
                { StatusCode = ((int)httpResponseMessage.StatusCode) };
            }
            catch (Exception ex)
            {
                logger.LogError("Ref_ID: " + KBZ_REF_NO + ",TrxnRefNum: " + obj.TrxnRefNum + "Exception: " + ex.ToString());
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = HttpContext.Request.PathBase,
                    CurrentUrl = HttpContext.Request.Path,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = KBZ_REF_NO,
                    PayLoad = JsonConvert.SerializeObject(obj),
                    TransactionRefNo = obj.TrxnRefNum,
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    ServiceName = "mgatesendsms",
                    ServiceCategory = "sms",
                    LoggedBy = claimUser
                });

                HttpResponseMessage httpResponseMessage = new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = KBZ_REF_NO,
                        Error =  new BaseRespError { Code = "1004", Message = ex.Message }
                    }),
                        Encoding.UTF8, "application/json")
                };
                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
                { StatusCode = ((int)httpResponseMessage.StatusCode) };
            }
        }

        //[HttpPost("wrapperbulksms/", Name = "wrapperbulksms")]
        //public async Task<IActionResult> wrapperbulksms(SMSPackageLoadModel obj)
        //{
        //    #region Old Code
        //    //obj.Message = Security.Encrypt(obj.Message, true, secret);
        //    //obj.TrxnRefNum = obj.TrxnRefNum.Trim();
        //    //var scheme = RequestContext.Url.Request.Headers.Authorization.Scheme;
        //    //var parameter = RequestContext.Url.Request.Headers.Authorization.Parameter;
        //    //if (System.Web.HttpContext.Current.Request.Headers["KBZ_REF_NO"] != null)
        //    //    KBZ_REF_NO = System.Web.HttpContext.Current.Request.Headers["KBZ_REF_NO"];
        //    //logger.Info("RefID:" + KBZ_REF_NO + ",Request Json:" + JsonConvert.SerializeObject(obj));
        //    //#region CheckValidate
        //    //if (string.IsNullOrEmpty(obj.TrxnRefNum) || obj.TrxnRefNum == "null")
        //    //{
        //    //    err_responseModel = ReturnMessage.CheckMandatory;
        //    //    err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", "TrxnRefNum");
        //    //    return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    //}

        //    //if (string.IsNullOrEmpty(obj.SubscriberNum))
        //    //{
        //    //    err_responseModel = ReturnMessage.CheckMandatory;
        //    //    err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", "SubscriberNum");
        //    //    return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    //}

        //    //if (String.IsNullOrEmpty(obj.ClientCode))
        //    //{
        //    //    err_responseModel = ReturnMessage.CheckMandatory;
        //    //    err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", "ClientCode");
        //    //    return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    //}

        //    //string[] array = obj.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //    ////if (array.Length > CONFIG.SETTING.TOTAL_MOBILE)
        //    ////{
        //    ////    err_responseModel = ReturnMessage.ExceedMobile;
        //    ////    err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", Convert.ToString(CONFIG.SETTING.TOTAL_MOBILE));
        //    ////    return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    ////}
        //    //string[] array = obj.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //    //for (int i = 0; i < array.Length; i++)
        //    //{
        //    //    if (!array[i].All(char.IsDigit) ||
        //    //            ContainsUnicodeCharacter(array[i]) || obj.SubscriberNum.Length < 6 || !(ValidateSubscriberNum(array[i], Convert.ToString(CONFIG.SETTING.FILTER))))
        //    //    {
        //    //        err_responseModel = ReturnMessage.InvalidMobileNo;
        //    //        return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    //    }

        //    //    if (!array[i].StartsWith("95"))
        //    //    {
        //    //        double dblno = Convert.ToDouble(array[i]);
        //    //        array[i] = "95" + dblno.ToString();
        //    //    }
        //    //}
        //    //obj.SubscriberNum = string.Join(",", array);

        //    //var configs = SUBSCRIBERS.Data.Where(o => o.API_USER == claimuserName && o.CLIENTCODE == obj.ClientCode);
        //    //if (configs.FirstOrDefault() == null)
        //    //{
        //    //    err_responseModel = ReturnMessage.InvalidUserName_ClientCode;
        //    //    return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //    //}
        //    //else
        //    //{
        //    //    RequestMGateModel mgate = new RequestMGateModel
        //    //    {
        //    //        scheme = scheme,
        //    //        para = parameter,
        //    //        header = KBZ_REF_NO,
        //    //        loggedby = claimuserName,
        //    //        url = Convert.ToString(CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == obj.ClientCode).FirstOrDefault().ROUTE_TO_ENV == "UAT" ? CONFIG.SMS_SERVICE.MGATE.ENVIRONMENT.Where(o => o.ENV == "UAT").FirstOrDefault().BULKSMS_SERVICEURL : CONFIG.SMS_SERVICE.MGATE.ENVIRONMENT.Where(o => o.ENV == "PROD").FirstOrDefault().BULKSMS_SERVICEURL + CONFIG.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE),
        //    //        bus_ip = CONFIG.SETTING.BUS_IP,
        //    //        bus_server = CONFIG.SETTING.BUS_SERVER,
        //    //        request_obj = obj,
        //    //        json_data = configs.Where(o => o.CLIENTCODE == obj.ClientCode && o.API_USER == claimuserName).FirstOrDefault(),
        //    //        checkvalidate = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == obj.ClientCode).FirstOrDefault().CHECKVALIDATE
        //    //    };
        //    //    return await Task.FromResult(_services.mgate_bulksms(mgate)).Result;
        //    //}
        //    #endregion
        //    try
        //    {
        //        var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
        //        obj.Message = Security.Encrypt(obj.Message, true, secret);
        //        obj.TrxnRefNum = obj.TrxnRefNum.Trim();
        //        var scheme = authHeader.Scheme;
        //        var parameter = authHeader.Parameter;
        //        KBZ_REF_NO = KBZRefNo;


        //        string[] array = obj.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //        if (array.Length > CONFIG.SETTING.TOTAL_MOBILE)
        //        {
        //            err_responseModel = ReturnMessage.ExceedMobile;
        //            err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", Convert.ToString(CONFIG.SETTING.TOTAL_MOBILE));
        //            return new ObjectResult(JsonConvert.SerializeObject(new { KBZRefNo = KBZ_REF_NO, Error = new BaseRespError {Code = err_responseModel.Code, Message=err_responseModel.Message } }))
        //            { StatusCode = ((int)Models.HttpStatusCode.BadRequest) };
        //            //return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
        //        }
        //        for (int i = 0; i < array.Length; i++)
        //        {
        //            if (!array[i].All(char.IsDigit) ||
        //                    ContainsUnicodeCharacter(array[i]) || obj.SubscriberNum.Length < 6 || !(ValidateSubscriberNum(array[i], Convert.ToString(CONFIG.SETTING.FILTER))))
        //            {
        //                err_responseModel = ReturnMessage.InvalidMobileNo;
        //                return new ObjectResult(JsonConvert.SerializeObject(new { KBZRefNo = KBZ_REF_NO, Error = new BaseRespError { Code =err_responseModel.Code, Message=err_responseModel.Message } }))
        //                { StatusCode = ((int)Models.HttpStatusCode.BadRequest) };
        //            }

        //            if (!array[i].StartsWith("95"))
        //            {
        //                double dblno = Convert.ToDouble(array[i]);
        //                array[i] = "95" + dblno.ToString();
        //            }
        //        }
        //        obj.SubscriberNum = string.Join(",", array);


        //        RequestMGateModel mgate = new RequestMGateModel
        //        {
        //            scheme = scheme,
        //            para = parameter,
        //            header = KBZ_REF_NO,
        //            loggedby = claimUser,
        //            url = String.Concat(CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == obj.ClientCode).FirstOrDefault().ROUTE_TO_ENV == "UAT" ? CONFIG.SMS_SERVICE.MGATE.ENVIRONMENT.Where(o => o.ENV == "UAT").FirstOrDefault().SERVICE_URL : CONFIG.SMS_SERVICE.MGATE.ENVIRONMENT.Where(o => o.ENV == "PROD").FirstOrDefault().SERVICE_URL, CONFIG.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE),
        //            bus_ip = CONFIG.SETTING.BUS_IP,
        //            bus_server = CONFIG.SETTING.BUS_SERVER,
        //            request_obj = obj,
        //            //json_data = configs.Where(o => o.TELCO_CODE == vendors && o.CLIENTCODE == obj.ClientCode && o.API_USER == claimuserName).FirstOrDefault(),
        //            checkvalidate = CONFIG.ROUTES.Business.ROUTE.Wherfe(o => o.CHANNEL == obj.ClientCode).FirstOrDefault().CHECKVALIDATE
        //        };
        //        HttpResponseMessage httpResponseMessage = await Task.FromResult(_services.mgate_bulksms(mgate)).Result;

        //        return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
        //        { StatusCode = ((int)httpResponseMessage.StatusCode) };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError("Ref_ID: " + KBZ_REF_NO + ",TrxnRefNum: " + obj.TrxnRefNum + "Exception: " + ex.ToString());
        //        new BaseController().logsAudit(new AuditLogModel
        //        {
        //            HttpCode = Models.HttpStatusCode.InternalServerError,
        //            HttpVerb = HttpVerbs.POST,
        //            SourceUrl = BaseAddress,
        //            PayLoadType = PayLoadType.RESPONSE,
        //            LogLevel = Models.LogLevel.ERROR,
        //            KBZMessageID = KBZ_REF_NO,
        //            PayLoad = JsonConvert.SerializeObject(obj),
        //            TransactionRefNo = obj.TrxnRefNum,
        //            Message = ex.Message,
        //            Exception = ex.ToString(),
        //            ServiceName = "mgatesendsms",
        //            ServiceCategory = "sms",
        //            LoggedBy = claimUser
        //        });

        //        HttpResponseMessage httpResponseMessage = new System.Net.Http.HttpResponseMessage
        //        {
        //            StatusCode = System.Net.HttpStatusCode.InternalServerError,
        //            Content = new StringContent(JsonConvert.SerializeObject(new
        //            {
        //                KBZRefNo = KBZ_REF_NO,
        //                Error = new BaseRespError { Code = "1004", Message = ex.Message }
        //            }),
        //                Encoding.UTF8, "application/json")
        //        };
        //        return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
        //        { StatusCode = ((int)httpResponseMessage.StatusCode) };
        //    }
        //}
        [HttpGet("HealthCheck/", Name = "HealthCheck")]
        public IActionResult HealthCheck()
        {
            requestMsg = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(_settings.ROUTES.Business.ROUTE.FirstOrDefault().SYSURL + "healthcheck?server=" + CONFIG.SETTING.BUS_SERVER) };
            responseMsg = client.SendAsync(requestMsg).Result;
            return StatusCode(StatusCodes.Status200OK, JObject.Parse(responseMsg.Content.ReadAsStringAsync().Result)["data"].ToString());
        }

        //[HttpPost("CallBackSave/", Name = "CallBackSave")]
        //public IActionResult CallBackSave(ReqCallBackModel model)
        //{
        //    try
        //    {
        //        AssignLogID();
        //        logger.LogInformation("KBZ_REF_NO:" + KBZ_REF_NO + " ,Request Json:" + JsonConvert.SerializeObject(model));
        //        var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
        //        scheme = authHeader.Scheme;
        //        parameter = authHeader.Parameter;


        //        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        //        requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(CONFIG.ROUTES.Business.ROUTE.FirstOrDefault().SYSURL + "callbacksave") };
        //        requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
        //        requestMsg.Headers.Add("KBZ_REF_NO", KBZ_REF_NO);
        //        logger.LogInformation("Request CallBackSave URL: " + requestMsg.RequestUri.ToString() + ", Payload: " + JsonConvert.SerializeObject(model));
        //        responseMsg = client.SendAsync(requestMsg).Result;
        //        logger.LogInformation("Response CallBackSave: " + responseMsg.Content.ReadAsStringAsync().Result);

        //        //if (CONFIG.LOG.System.APILOG == "Y")
        //        //{
        //        //    if (responseMsg.StatusCode == HttpStatusCode.OK)
        //        //    {
        //        //        logs = new AuditLogModel { HttpCode = responseMsg.StatusCode, HttpVerb = HttpVerbs.POST, SourceUrl = HttpContext.Current.Request.Url.AbsoluteUri, PayLoadType = PayLoadType.RESPONSE, LogLevel = LogLevel.INFO, KBZMessageID = KBZ_REF_NO, PayLoad = responseMsg.Content.ReadAsStringAsync().Result, TransactionRefNo = model.message_id, Message = "Business" };
        //        //    }
        //        //    else
        //        //    {
        //        //        logs = new AuditLogModel { HttpCode = responseMsg.StatusCode, HttpVerb = HttpVerbs.POST, SourceUrl = HttpContext.Current.Request.Url.AbsoluteUri, PayLoadType = PayLoadType.RESPONSE, LogLevel = LogLevel.ERROR, KBZMessageID = KBZ_REF_NO, PayLoad = responseMsg.Content.ReadAsStringAsync().Result, TransactionRefNo = model.message_id, Message = "Business" };
        //        //    }                
        //        //    logsAudit(logs);
        //        //}
        //        return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMsg.Content.ReadAsStringAsync().Result)))
        //        { StatusCode = ((int)responseMsg.StatusCode) };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError("Ref_ID: " + KBZ_REF_NO + ",Message ID: " + model.message_id + "Exception: " + ex.ToString());

        //        HttpResponseMessage httpResponseMessage = new System.Net.Http.HttpResponseMessage
        //        {
        //            StatusCode = System.Net.HttpStatusCode.InternalServerError,
        //            Content = new StringContent(JsonConvert.SerializeObject(new
        //            {
        //                KBZRefNo = KBZ_REF_NO,
        //                Error = new BaseRespError { Code = "1004", Message = ex.Message } 
        //            }),
        //                Encoding.UTF8, "application/json")
        //        };
        //        return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
        //        { StatusCode = ((int)httpResponseMessage.StatusCode) };
        //    }

        //    //requestMsg = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(CONFIG.ROUTES.Business.ROUTE.FirstOrDefault().SYSURL + "healthcheck?server=" + CONFIG.SETTING.BUS_SERVER) };
        //    //responseMsg = Global.client.SendAsync(requestMsg).Result;
        //    //return Request.CreateResponse(HttpStatusCode.OK, JObject.Parse(responseMsg.Content.ReadAsStringAsync().Result)["data"].ToString(), "application/json");
        //}

        [HttpPost("TestSendSMS/", Name = "TestSendSMS")]
        public async Task<IActionResult> TestSendSMS(SMSPackageLoadModel param)
        {
            SMSPackageLoadModel obj = new SMSPackageLoadModel();
            try
            {
                obj = param;
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                obj.Message = Security.Encrypt(obj.Message, true, secret);
                obj.TrxnRefNum = obj.TrxnRefNum.Trim();
                scheme = authHeader.Scheme;
                parameter = authHeader.Parameter;
                GetClaimUsername();
                AssignLogID();
                string url;

                HttpResponseMessage result = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, $"GetGateway?gateWay={param.Gateway}", null).Result;
                if (result.IsSuccessStatusCode)
                {
                    ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(result.Content.ReadAsStringAsync().Result);

                    GateWay gateWay = JsonConvert.DeserializeObject<GateWay>(JsonConvert.SerializeObject(response.Data));
                    if (gateWay != null) {
                        obj.ServiceURL1 = gateWay.SERVICEURL1;
                        obj.ServiceURL2 = gateWay.SERVICEURL2;
                        obj.ServiceURL3 = gateWay.SERVICEURL3;
                    }
                }

                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogInformation("RefID:" + KBZ_REF_NO + ",Request Json:" + JsonConvert.SerializeObject(obj));
                if (obj.Route_to_env != null && obj.Route_to_env.Equals("UAT"))
                {
                    url = obj.ServiceURL3;
                }
                else if (obj.Route_to_env != null && obj.Route_to_env.Equals("PRODUCTION"))
                {
                    if (_settings.BUS_SERVER.Contains("dc"))
                    {
                        url = obj.ServiceURL1;
                    }
                    else
                    {
                        url = obj.ServiceURL2;
                    }
                }
                else
                {
                    url = obj.ServiceURL3;
                }

                //var handler = new JwtSecurityTokenHandler();
                //var jwtSecurityToken = handler.ReadJwtToken(parameter);
                //var Name = jwtSecurityToken.Claims.First(v => v.Type == "user_name").Value;
                if (claimUser == null)
                {
                    logger.LogError("Ref_ID: " + KBZ_REF_NO + ",TrxnRefNum: " + obj.TrxnRefNum + "Error: CLaim user is null");
                    HttpResponseMessage unauthorizeHttpResponseMessage = new System.Net.Http.HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Content = new StringContent(JsonConvert.SerializeObject(new
                        {
                            KBZRefNo = KBZ_REF_NO,
                            Error = ErrorCode.Unauthorized
                        }),
                        Encoding.UTF8, "application/json")
                    };
                    return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(unauthorizeHttpResponseMessage.Content.ReadAsStringAsync().Result)))
                    { StatusCode = ((int)unauthorizeHttpResponseMessage.StatusCode) };
                }

                RequestMGateModel mgate = new RequestMGateModel
                {
                    scheme = scheme,
                    para = parameter,
                    header = KBZ_REF_NO,
                    loggedby = claimUser, // Token user
                    url = url + "/" + _settings.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE,
                    bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                    bus_server = _settings.BUS_SERVER, // DC or DR
                    request_obj = obj,
                    json_data = new { obj },
                    checkvalidate = obj.CheckDuplicate
                };
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                if (obj.Gateway.Equals("MGate"))
                {
                    if (!obj.SubscriberNum.StartsWith("959"))
                    {
                        if (obj.SubscriberNum.StartsWith("0"))
                        {
                            string cutPhoneNumber = obj.SubscriberNum.Substring(1, obj.SubscriberNum.Length - 1);
                            obj.SubscriberNum = "95" + cutPhoneNumber;
                        }
                        else
                        {
                            obj.SubscriberNum = "95" + obj.SubscriberNum;
                        }
                    }
                    mgate.checkvalidate = obj.CheckDuplicate;

                    httpResponseMessage = _services.mgatesendsms(mgate);
                }
                else if (obj.Gateway.Equals("MGateBulk"))
                {

                    string[] array = obj.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length > _settings.SETTING.TOTAL_MOBILE)
                    {
                        err_responseModel = ReturnMessage.ExceedMobile;
                        err_responseModel.Message = err_responseModel.Message.Replace("[FIELD]", Convert.ToString(_settings.SETTING.TOTAL_MOBILE));
                        return new ObjectResult(JsonConvert.SerializeObject(new { KBZRefNo = KBZ_REF_NO, Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message } }))
                        { StatusCode = ((int)Models.HttpStatusCode.BadRequest) };
                        //return Request.CreateResponse(HttpStatusCode.BadRequest, new { KBZRefNo = KBZ_REF_NO, Error = new ErrorResponseModel[] { err_responseModel } }, "application/json");
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!array[i].StartsWith("959"))
                        {
                            if (array[i].StartsWith("0"))
                            {
                                string cutPhoneNumber = array[i].Substring(1, array[i].Length - 1);
                                array[i] = "95" + cutPhoneNumber;
                            }
                            else
                            {
                                array[i] = "95" + array[i];
                            }
                        }
                    }

                    obj.SubscriberNum = string.Join(",", array);
                    mgate = new RequestMGateModel
                    {
                        scheme = scheme,
                        para = parameter,
                        header = KBZ_REF_NO,
                        loggedby = claimUser, // Token user
                        url = url + "/" + _settings.SMS_SERVICE.MGATE.REQ_FORMAT_VALUE,
                        bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                        bus_server = _settings.BUS_SERVER, // DC or DR
                        request_obj = obj,
                        json_data = new { obj },
                        checkvalidate = obj.CheckDuplicate,
                        SecretID = obj.SecretID,
                        SecretKey = obj.SecretKey
                    };

                    //httpResponseMessage = await Task.FromResult(_services.mgate_bulksms(mgate)).Result;
                }
                else if (obj.Gateway.Equals("SMSBrix"))
                {

                    if (obj.SubscriberNum.StartsWith("959"))
                    {
                        obj.SubscriberNum = "09" + obj.SubscriberNum.Substring(3, obj.SubscriberNum.Length - 3);
                    }
                    else
                    {
                        if (!obj.SubscriberNum.StartsWith("09"))
                        {
                            obj.SubscriberNum = "09" + obj.SubscriberNum.Substring(1, obj.SubscriberNum.Length - 1);
                        }

                    }
                    //url = obj.ServiceURL1;

                    mgate = new RequestMGateModel
                    {
                        scheme = scheme,
                        para = parameter,
                        header = KBZ_REF_NO,
                        loggedby = claimUser, // Token user
                        url = url,
                        bus_ip = _settings.BUS_IP, // Which server DC IP or DR IP
                        bus_server = _settings.BUS_SERVER, // DC or DR
                        request_obj = obj,
                        json_data = new { obj },
                        checkvalidate = obj.CheckDuplicate,
                        SecretID = obj.SecretID,
                        SecretKey = obj.SecretKey
                    };

                    httpResponseMessage = _smsbrixService.smsbrixsendsms(mgate);
                }

                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
                { StatusCode = ((int)httpResponseMessage.StatusCode) };
            }
            catch (Exception ex)
            {
                logger.LogError("Ref_ID: " + KBZ_REF_NO + ",TrxnRefNum: " + obj.TrxnRefNum + "Exception: " + ex.ToString());
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = HttpContext.Request.PathBase,
                    CurrentUrl = HttpContext.Request.Path,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = KBZ_REF_NO,
                    PayLoad = JsonConvert.SerializeObject(obj),
                    TransactionRefNo = obj.TrxnRefNum,
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    ServiceName = "mgatesendsms",
                    ServiceCategory = "sms",
                    LoggedBy = claimUser
                });

                HttpResponseMessage httpResponseMessage = new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = KBZ_REF_NO,
                        Error = new BaseRespError { Code = "1004", Message = ex.Message }
                    }),
                        Encoding.UTF8, "application/json")
                };
                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)))
                { StatusCode = ((int)httpResponseMessage.StatusCode) };
            }
        }
    }
}