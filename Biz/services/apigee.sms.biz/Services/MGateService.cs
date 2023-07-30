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
using apigee.sms.biz.Models;
using AspNetCore.ServiceRegistration.Dynamic;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace apigee.sms.biz.Services
{
    [ScopedService]
    public interface IServices
    {
        HttpResponseMessage mgatesendsms(RequestMGateModel obj);
        //Task<HttpResponseMessage> mgate_bulksms(RequestMGateModel model);
    }
    public class MGateService : IServices
    {
        private ILogger<MGateService> logger;
        
        internal string BaseAddress = null;
        private dynamic return_obj;
        const string secret = "t@1c0@p!";
        internal readonly IHttpClientFactory _client;
        internal static CancellationTokenSource cts;
        internal readonly AppSettings _settings;
        internal readonly ICRUDService _crud;
        public MGateService(ILogger<MGateService> nlog, IHttpClientFactory httpClient, ICRUDService crud, IOptionsMonitor<AppSettings> setting)
        {
            logger = nlog;
            _crud = crud;
            _client = httpClient;
            _settings = setting.CurrentValue;
        }
        public HttpResponseMessage mgatesendsms(RequestMGateModel obj)
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
                    MERCHANT_ID = "MGATE",
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

                obj.request_obj.Message = Security.Decrypt(obj.request_obj.Message, true, secret);
                var param = DynamicQueryString(obj.url, obj.request_obj, obj.json_data, obj.loggedby);
                BaseAddress = "http://" + new Uri(obj.url).Authority;
                HttpRequestMessage requestMsg = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(BaseAddress + param) };
                //await Task.Run(() => client.SendAsync(requestMsg));
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(obj.scheme, obj.para);
                requestMsg.Headers.Add("KBZRefNo", obj.header);
                var client = _client.CreateClient();

                #region Send SMS (Actual Working Code) (!!!!!!!!!!!!!! Uncomment Before Publishing)
                if (_settings.SMS_ON.Equals("Y"))
                {
                    client.SendAsync(requestMsg);
                }
                #region Log audit
                //HttpResponseMessage response = await Task.FromResult(client.SendAsync(requestMsg).Result);
                //logger.LogInformation("Respond from MGate: " + JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result));
                //sys_model.MERCHANT_RESPONSE = response.Content.ReadAsStringAsync().Result;
                //new BaseController().logsAudit(new AuditLogModel
                //{
                //    HttpCode = (apigee.sms.biz.Models.HttpStatusCode)response.StatusCode,
                //    HttpVerb = HttpVerbs.POST,
                //    SourceUrl = BaseAddress,
                //    CurrentUrl = BaseAddress + param,
                //    PayLoadType = PayLoadType.RESPONSE,
                //    LogLevel = Models.LogLevel.INFO,
                //    KBZMessageID = obj.header,
                //    PayLoad = JsonConvert.SerializeObject(obj.request_obj),
                //    TransactionRefNo = obj.request_obj.TrxnRefNum,
                //    ServiceName = "MGATE",
                //    ServiceCategory = "sms",
                //    ResponseCode = response.StatusCode.ToString(),
                //    Message = response.Content.ReadAsStringAsync().Result,
                //});
                #endregion
                #endregion

                sys_model.RESPONSE_DATETIME = DateTime.Now;
                //sys_model.REQUEST = JsonConvert.SerializeObject(requestMsg.Content.ReadAsStringAsync().Result);
                sys_model.RESPONSE = JsonConvert.SerializeObject(new { KBZRefNo = obj.header, Data = new ResponseModelForUtil[] { ReturnMessage.Pending } });
                if (obj.checkvalidate == "Y")
                {
                    Task.Run(() => _crud.UpdateTransaction(sys_model, obj.scheme, obj.para));
                }
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
                    ServiceName = "MGATE",
                    ServiceCategory = "sms"
                });

                return new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = obj.header,
                        Error = new BaseRespError { Code = ReturnMessage.BusinessException.Code, Message = ReturnMessage.BusinessException.Message }
                    }),
                        Encoding.UTF8, "application/json")
                };
            }
        }

        internal string DynamicQueryString(string url, SMSPackageLoadModel bizModel, dynamic json, string username)
        {
            string retrieve_Val = "";
            try
            {
                var query = HttpUtility.ParseQueryString(new Uri(url).Query);
                if (HttpUtility.ParseQueryString(new Uri(url).Query).HasKeys())
                {
                    foreach (var item in HttpUtility.ParseQueryString(new Uri(url).Query).AllKeys)
                    {
                        string[] array = query[item].Split(',');
                        logger.LogInformation(JsonConvert.SerializeObject(array));
                        string temp = "";
                        for (int i = 0; i < array.Count(); i++)
                        {
                            if (bizModel.GetType().GetProperties().Where(o => o.Name == array[i]).Any())
                            {
                                logger.LogInformation("before : " + array[i]);
                                retrieve_Val = Convert.ToString(bizModel.GetType().GetProperties().Where(o => o.Name == array[i]).FirstOrDefault().GetValue(bizModel, bizModel.GetType().GetProperties().Where(o => o.Name == array[i]).FirstOrDefault().GetIndexParameters()));
                                logger.LogInformation(array[i] + " , " + retrieve_Val);
                                if (array[i] == "Msg_type")
                                {
                                    if (string.IsNullOrEmpty(retrieve_Val))
                                        temp += 3;
                                    else if (retrieve_Val.ToLower() == "general")
                                        temp += 3;
                                    else if (retrieve_Val.ToLower() == "otp" || retrieve_Val.ToLower().Contains("access code"))
                                        temp += 5;
                                }
                                else
                                    temp += retrieve_Val;
                            }
                            else
                            {
                                //temp = json[0][array[i].ToString()].ToString();                           
                                temp = json.GetType().GetProperty(array[i]).GetValue(json);
                            }
                        }
                        query[item] = temp;
                    }
                }
                string querystring = new Uri(url).AbsolutePath + "?" + query.ToString().Replace("+", " ");
                return querystring;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return null;
            }
        }
        /*
        public async Task<HttpResponseMessage> mgate_bulksms(RequestMGateModel model)
        {
            try
            {
                SMS_Send_Request_Model payLoad = new SMS_Send_Request_Model();
                payLoad.SubscriberNum = model.request_obj.SubscriberNum;
                payLoad.TrxnRefNum = model.request_obj.TrxnRefNum;
                payLoad.Message = model.request_obj.Message;
                payLoad.Msg_type = model.request_obj.Msg_type;
                payLoad.ClientCode = model.request_obj.ClientCode;


                Telco_Bulk_SMS_Tran sys_model = new Telco_Bulk_SMS_Tran
                {
                    MERCHANT_ID = "MGATE BULK",
                    CLIENT_CODE = model.request_obj.ClientCode,
                    REQUEST_DATETIME = DateTime.Now,
                    MESSAGE = model.request_obj.Message,
                    PROCESS_STAGE = "PEND",
                    REQUEST = JsonConvert.SerializeObject(payLoad),
                    SUBSCRIBER_NO = model.request_obj.SubscriberNum,
                    TRN_REF_NO = model.request_obj.TrxnRefNum,
                    BUS_IP = model.bus_ip,
                    BUS_SERVER = model.bus_server,
                    CHECKVALIDATE = model.checkvalidate,
                    TELCO_CODE = model.request_obj.TelcoCode,
                    SMS_MESSAGE = model.request_obj.Message,
                    PROCESS_STAGES = "PEND",
                    TELCO_REF_NO = model.request_obj.ClientCode + model.request_obj.TrxnRefNum,
                    MERCHANT_REQUEST = JsonConvert.SerializeObject(model),
                    M_REQ_DATETIME = DateTime.Now
                };

                if (!_crud.InsertBulkTransaction(sys_model, model.scheme, model.para, out return_obj))
                {
                    ErrorResponseModel err_responseModel = (ErrorResponseModel)return_obj;
                    if (err_responseModel.Code == "1004")
                        return new System.Net.Http.HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError,
                            Content = new StringContent(JsonConvert.SerializeObject(new
                            {
                                KBZRefNo = model.header,
                                Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message }
                            }),
                    Encoding.UTF8, "application/json")
                        };
                    else
                        return new System.Net.Http.HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Content = new StringContent(JsonConvert.SerializeObject(new
                            {
                                KBZRefNo = model.header,
                                Error = new BaseRespError { Code = err_responseModel.Code, Message = err_responseModel.Message }
                            }),
                    Encoding.UTF8, "application/json")
                        };
                }
                sys_model.RESPONSE_DATETIME = DateTime.Now;

                await Task.Run(() => send_mgatebulk(model, sys_model));

                //HttpResponseMessage response = await Task.FromResult(client.SendAsync(requestMsg).Result);

                //new BaseController().logsAudit(new AuditLogModel
                //{
                //    HttpCode = (apigee.sms.biz.Models.HttpStatusCode)response.StatusCode,
                //    HttpVerb = HttpVerbs.POST,
                //    SourceUrl = BaseAddress,
                //    PayLoadType = PayLoadType.RESPONSE,
                //    LogLevel = Models.LogLevel.INFO,
                //    KBZMessageID = model.header,
                //    PayLoad = JsonConvert.SerializeObject(model.request_obj),
                //    TransactionRefNo = model.request_obj.TrxnRefNum,
                //    ServiceName = "MGATE",
                //    ServiceCategory = "sms",
                //    ResponseCode = response.StatusCode.ToString(),
                //    Message = response.Content.ReadAsStringAsync().Result,
                //});

                return new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = model.header,
                        Data = new ResponseModelForUtil[] { ReturnMessage.Pending }
                    }),
                        Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                logger.LogError("Ref_ID: " + model.header + ",TrxnRefNum: " + model.request_obj.TrxnRefNum + "Exception: " + ex.ToString());
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = BaseAddress,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = model.header,
                    PayLoad = JsonConvert.SerializeObject(model.request_obj),
                    TransactionRefNo = model.request_obj.TrxnRefNum,
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    ServiceName = "MGATEBULK",
                    ServiceCategory = "sms"
                });

                return new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        KBZRefNo = model.header,
                        Error = new BaseRespError { Code = ReturnMessage.BusinessException.Code, Message = ReturnMessage.BusinessException.Message }
                    }),
                        Encoding.UTF8, "application/json")
                };
            }
        }
        
        private void send_mgatebulk(RequestMGateModel model, Telco_Bulk_SMS_Tran sys_model)
        {
            model.request_obj.Message = Security.Decrypt(model.request_obj.Message, true, secret);
            var param = DynamicQueryString(model.url, model.request_obj, model.json_data, model.loggedby);

            BaseAddress = "http://" + new Uri(model.url).Authority;
            sys_model.M_REQ_DATETIME = DateTime.Now;
            HttpRequestMessage requestMsg = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(BaseAddress + param) };
            cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(Convert.ToInt16(60)));
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue(model.scheme, model.para);
            requestMsg.Headers.Add("KBZRefNo", model.header);
            var client = _client.CreateClient();

            #region MGate Bulk Send SMS (!!!!!!!!!!!!!! Uncomment Before Publishing)
            if (_settings.SMS_ON.Equals("Y"))
            {
                Task.Run(() => client.SendAsync(requestMsg, cts.Token));
            }
            #region log audit
            //HttpResponseMessage responseMsg = client.SendAsync(requestMsg, cts.Token).Result;
            //logger.LogInformation("Respond from MGate Bulk: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
            //sys_model.MERCHANT_RESPONSE = responseMsg.Content.ReadAsStringAsync().Result;
            //new BaseController().logsAudit(new AuditLogModel
            //{
            //    HttpCode = (apigee.sms.biz.Models.HttpStatusCode)responseMsg.StatusCode,
            //    HttpVerb = HttpVerbs.POST,
            //    SourceUrl = BaseAddress,
            //    CurrentUrl = BaseAddress + param,
            //    PayLoadType = PayLoadType.RESPONSE,
            //    LogLevel = Models.LogLevel.INFO,
            //    KBZMessageID = model.header,
            //    PayLoad = JsonConvert.SerializeObject(model.request_obj),
            //    TransactionRefNo = model.request_obj.TrxnRefNum,
            //    ServiceName = "MGATEBULK",
            //    ServiceCategory = "sms",
            //    ResponseCode = responseMsg.StatusCode.ToString(),
            //    Message = responseMsg.Content.ReadAsStringAsync().Result,
            //});
            #endregion
            #endregion

            sys_model.MERCHANT_REQUEST = BaseAddress + param;
            sys_model.M_RESP_DATETIME = DateTime.Now;
            sys_model.SMS_MESSAGE = model.request_obj.Message;
            sys_model.PROCESS_STAGES = "PEND";

            sys_model.RESPONSE = JsonConvert.SerializeObject(new { KBZRefNo = model.header, Data = new ResponseModelForUtil[] { ReturnMessage.Pending } });
            Utility.UpdateBulkTransaction(sys_model, model.scheme, model.para);
        }
        */
    }
}