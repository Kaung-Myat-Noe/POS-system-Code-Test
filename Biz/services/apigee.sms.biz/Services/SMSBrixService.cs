using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using apigee.sms.biz.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using apigee.sms.biz.Controllers;
using apigee.sms.biz.Utilities;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using NLog;
using NLog.Web;
using HttpStatusCode = System.Net.HttpStatusCode;
using AspNetCore.ServiceRegistration.Dynamic;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace apigee.sms.biz.Services
{
    [ScopedService]
    public interface ISMSBrixServices
    {
        HttpResponseMessage smsbrixsendsms(RequestMGateModel obj);        
    }
    public class SMSBrixService : ISMSBrixServices
    {
        private ILogger<SMSBrixService> logger;
        internal string BaseAddress = null;
        private dynamic return_obj;
        internal readonly ICRUDService _crud;
        const string secret = "t@1c0@p!";
        internal readonly IHttpClientFactory _client;
        internal static CancellationTokenSource cts;
        internal readonly AppSettings _settings;
        public SMSBrixService(ILogger<SMSBrixService> nlog, ICRUDService crud, IHttpClientFactory clientfactory, IOptionsMonitor<AppSettings> setting)
        {
            logger = nlog;
            _crud = crud;
            _client = clientfactory;
            _settings = setting.CurrentValue;
        }
        public HttpResponseMessage smsbrixsendsms(RequestMGateModel obj)
        {
            if (string.IsNullOrEmpty(obj.request_obj.Msg_type))
                obj.request_obj.Msg_type = "general";
            try
            {
                SMS_Send_Request_Model payLoad = new SMS_Send_Request_Model();
                payLoad.SubscriberNum = obj.request_obj.SubscriberNum;
                payLoad.TrxnRefNum = obj.request_obj.TrxnRefNum;
                payLoad.Message = obj.request_obj.Message;
                payLoad.Msg_type = obj.request_obj.Msg_type;
                payLoad.ClientCode = obj.request_obj.ClientCode;

                TelcoSMSModel sys_model = new TelcoSMSModel
                {
                    TELCO_CODE = obj.request_obj.TelcoCode,
                    MERCHANT_ID = "SMSBRIX",
                    CLIENT_CODE = obj.request_obj.ClientCode,
                    REQUEST_DATETIME = DateTime.Now,
                    SMS_MESSAGE = obj.request_obj.Message,
                    PROCESS_STAGES = "PEND",
                    REQUEST = JsonConvert.SerializeObject(payLoad),
                    SUBSCRIBER_NO = obj.request_obj.SubscriberNum,
                    TELCO_REF_NO = obj.request_obj.ClientCode + obj.request_obj.TrxnRefNum,
                    MERCHANT_REQUEST = JsonConvert.SerializeObject(obj),
                    TRN_REF_NO = obj.request_obj.TrxnRefNum,
                    BUS_IP = obj.bus_ip,
                    BUS_SERVER = obj.bus_server,
                    CHECKVALIDATE = obj.checkvalidate
                };
                if (obj.checkvalidate == "Y")
                {
                    if (!_crud.InsertTransaction(sys_model, obj.scheme, obj.para, out return_obj))
                    {
                        ErrorResponseModel err_responseModel = (ErrorResponseModel)return_obj;
                        if (err_responseModel.Code == "1004")
                        {
                            return new System.Net.Http.HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.InternalServerError,
                                Content = new StringContent(JsonConvert.SerializeObject(new
                                {
                                    KBZRefNo = obj.header,
                                    Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message }
                                }),
                        Encoding.UTF8, "application/json")
                            };
                        }
                        else
                        {
                            return new System.Net.Http.HttpResponseMessage
                            {

                                StatusCode = HttpStatusCode.BadRequest,
                                Content = new StringContent(JsonConvert.SerializeObject(new
                                {
                                    KBZRefNo = obj.header,
                                    Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message }
                                }),
                            Encoding.UTF8, "application/json")
                            };
                        }
                    }
                }

                obj.request_obj.Message = Security.Decrypt(obj.request_obj.Message, true, secret);                
                var formContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("number", obj.request_obj.SubscriberNum),
                new KeyValuePair<string, string>("senderid", obj.request_obj.SENDERNAME),
                new KeyValuePair<string, string>("message", obj.request_obj.Message),
                new KeyValuePair<string, string>("customid", obj.request_obj.TrxnRefNum),
                new KeyValuePair<string, string>("product", obj.request_obj.ClientCode),
                });
                BaseAddress = "http://" + new Uri(obj.url).Authority;
                HttpRequestMessage requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(obj.url), Content = formContent };

                logger.LogInformation(formContent.ReadAsStringAsync().Result);

                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode($"{obj.SecretID}:{obj.SecretKey}"));
                requestMsg.Headers.Add("KBZRefNo", obj.header);
                var client = _client.CreateClient("HttpClientWithSSLUntrusted");
                //await Task.Run(() => client.SendAsync(requestMsg));

                #region Send SMS (Actual Working Code) (!!!!!!!!!!!!!! Uncomment Before Publishing)
                if (_settings.SMS_ON.Equals("Y"))
                {
                    //client.SendAsync(requestMsg);
                    client.SendAsync(requestMsg);
                    //logger.LogInformation("SMSBRix Response: " + test.Content.ReadAsStream());
                }
                #region log audit
                //HttpResponseMessage response = await Task.FromResult(client.SendAsync(requestMsg).Result);
                //logger.LogInformation("Respond from smsbrix: " + JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result));
                //sys_model.MERCHANT_RESPONSE = response.Content.ReadAsStringAsync().Result;
                //new BaseController().logsAudit(new AuditLogModel
                //{
                //    HttpCode = (apigee.sms.biz.Models.HttpStatusCode)response.StatusCode,
                //    HttpVerb = HttpVerbs.POST,
                //    SourceUrl = BaseAddress,
                //    PayLoadType = PayLoadType.RESPONSE,
                //    CurrentUrl = obj.url,
                //    LogLevel = Models.LogLevel.INFO,
                //    KBZMessageID = obj.header,
                //    PayLoad = JsonConvert.SerializeObject(obj.request_obj),
                //    TransactionRefNo = obj.request_obj.TrxnRefNum,
                //    ServiceName = "SMSBRIX",
                //    ServiceCategory = "sms",
                //    ResponseCode = response.StatusCode.ToString(),
                //    Message = response.Content.ReadAsStringAsync().Result,
                //});
                #endregion
                #endregion

                sys_model.RESPONSE_DATETIME = DateTime.Now;
                sys_model.REQUEST = JsonConvert.SerializeObject(formContent.ReadAsStringAsync().Result);
                sys_model.RESPONSE = JsonConvert.SerializeObject(new { KBZRefNo = obj.header, Data = new ResponseModelForUtil[] { ReturnMessage.Pending } });
                if (obj.checkvalidate == "Y")
                    Task.Run(()=> _crud.UpdateTransaction(sys_model, obj.scheme, obj.para));
                else
                {
                    sys_model.RESPONSE_DATETIME = DateTime.Now;
                    sys_model.RESPONSE = JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = obj.header,
                        Data = new ResponseModelForUtil[] { ReturnMessage.Pending }
                    });
                    _crud.InsertTransactionAsync(sys_model, obj.scheme, obj.para);
                }
                return new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = obj.header,
                        Data = new ResponseModelForUtil[] { ReturnMessage.Pending }
                    }),
                        Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                logger.LogError("Ref_ID: " + obj.header + ",TrxnRefNum: " + obj.request_obj.TrxnRefNum + "Exception: " + ex.ToString());
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = BaseAddress,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = obj.header,
                    PayLoad = JsonConvert.SerializeObject(obj.request_obj),
                    TransactionRefNo = obj.request_obj.TrxnRefNum,
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    ServiceName = "smsbrixsendsms",
                    ServiceCategory = "sms"
                });

                return new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = obj.header,
                        Data = obj,
                        Error = new BaseRespError { Code = ReturnMessage.BusinessException.Code, Message = ReturnMessage.BusinessException.Message }
                    }),
                        Encoding.UTF8, "application/json")
                };
            }
        }
        public static string Base64Encode(string textToEncode)
        {
            byte[] textAsBytes = Encoding.UTF8.GetBytes(textToEncode);
            return Convert.ToBase64String(textAsBytes);
        }       
    }
}