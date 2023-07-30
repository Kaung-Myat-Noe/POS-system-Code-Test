using apigee.sms.biz.Models;
using Newtonsoft.Json;
using apigee.sms.biz.Controllers;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using System.Net;
using NLog;
using NLog.Web;
using HttpStatusCode = System.Net.HttpStatusCode;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.Extensions.Options;

namespace apigee.sms.biz.Utilities
{
    [ScopedService]
    public class Utility : BaseController
    {
        private static ILogger<Utility> logger;
        private static HttpRequestMessage requestMsg;
        private static HttpResponseMessage responseMsg;
        private static string BaseAddress = null;
        internal readonly IHttpClientFactory _client;
        internal static CancellationTokenSource cts;
        internal readonly AppSettings _settings;
        public Utility(IHttpClientFactory httpClient, ILogger<Utility> nlog, IOptionsMonitor<AppSettings> setting)
        {
            logger = nlog;
            _client = httpClient;
            _settings = setting.CurrentValue;
        }
        public static bool InsertTransaction(TelcoSMSModel model, string scheme, string parameter, out dynamic return_obj)
        {
            //client = new HttpClient();
            //logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("System ROUTE:" + JsonConvert.SerializeObject(_settings.URL.SYSTEM));
                //var systemRoutes = CONFIG.ROUTES.Business.ROUTE.ToList();
                string sys_sms_URL = _settings.URL.SYSTEM;


                //InsertTransWithChecking
                if (model.CHECKVALIDATE == "Y")
                {
                    BaseAddress = sys_sms_URL + "InsertTransWithChecking";
                }
                else
                {
                    BaseAddress = sys_sms_URL + "InsertTransWithoutChecking";
                }

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model) + "TELCO_REF_NO:" + model.TRN_REF_NO);
                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(Convert.ToInt16(CONFIG.ROUTES.Business.TIMEOUT)));
                var client = _client.CreateClient();
                responseMsg = client.SendAsync(requestMsg).Result;
                logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
                ResponseModel returnedData = JsonConvert.DeserializeObject<ResponseModel>(responseMsg.Content.ReadAsStringAsync().Result);
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Respond JSON:" + JsonConvert.SerializeObject(responseMsg.Content.ReadAsStringAsync().Result));
                if (responseMsg.StatusCode == HttpStatusCode.OK)
                {
                    return_obj = ReturnMessage.Success;
                    return true;
                }
                else if (responseMsg.StatusCode == HttpStatusCode.BadRequest)
                {
                    return_obj = ReturnMessage.DuplicateTranRefNo;
                    return false;
                }
                else
                {
                    return_obj = new ErrorResponseModel { Code = returnedData.Error.Code, Message = returnedData.Error.Message};
                    return false;
                }
                logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Message: " + JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result)));
            }
            catch (Exception ex)
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception:" + ex.StackTrace + "\n Message:" + ex.Message);
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = BaseAddress,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = KBZRefNo,
                    PayLoad = JsonConvert.SerializeObject(model),
                    TransactionRefNo = model.TRN_REF_NO,
                    Message = ex.Message,
                    Exception = ex.ToString()
                });
                return_obj = ReturnMessage.BusinessException;
                return false;
            }
        }

        public static bool InsertBulkTransaction(Telco_Bulk_SMS_Tran model, string scheme, string parameter, out dynamic return_obj)
        {
            //client = new HttpClient();
            //logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                //var systemRoutes = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == model.CLIENT_CODE).ToList();
                //string sys_sms_URL = String.Empty;
                //if (systemRoutes.Count() > 0)
                //{
                //    sys_sms_URL = systemRoutes.FirstOrDefault().SYSURL;
                //}
                //else
                //{
                //    return_obj = ReturnMessage.NoRoute;
                //    return false;
                //}
                string sys_sms_URL = _settings.URL.SYSTEM;


                BaseAddress = sys_sms_URL + "InsertTransBulkWithChecking";
                
                //    BaseAddress = sys_sms_URL + "InsertTransWithoutChecking";
                

                //BaseAddress = sys_sms_URL + "save_bulksms";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);

                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + "Request System Url:" + sys_sms_URL + ", TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model));

                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(Convert.ToInt16(CONFIG.ROUTES.Business.TIMEOUT)));
                var client = _client.CreateClient();
                responseMsg = client.SendAsync(requestMsg).Result;
                logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Respond JSON:" + JsonConvert.SerializeObject(responseMsg.Content.ReadAsStringAsync().Result));
                if (responseMsg.StatusCode == HttpStatusCode.OK)
                {
                    return_obj = ReturnMessage.Success;
                    return true;
                }
                else if (responseMsg.StatusCode == HttpStatusCode.BadRequest)
                {
                    return_obj = ReturnMessage.DuplicateTranRefNo;
                    return false;
                }
                else
                {
                    return_obj = ReturnMessage.BusinessException;
                    return false;
                }
                logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Message: " + JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result)));
            }
            catch (Exception ex)
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception:" + ex.StackTrace + "\n Message:" + ex.Message);
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = BaseAddress,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = KBZRefNo,
                    PayLoad = JsonConvert.SerializeObject(model),
                    TransactionRefNo = model.TRN_REF_NO,
                    Message = ex.Message,
                    Exception = ex.ToString()
                });
                return_obj = ReturnMessage.BusinessException;
                return false;
            }
        }

        public async static void UpdateTransaction(TelcoSMSModel model, string scheme, string parameter)
        {
            //client = new HttpClient();
            //logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model));
                //var systemRoutes = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == model.CLIENT_CODE).ToList();
                //string sys_sms_URL = String.Empty;
                //if (systemRoutes.Count() > 0)
                //{
                //    sys_sms_URL = systemRoutes.FirstOrDefault().SYSURL;
                //}
                string sys_sms_URL = _settings.URL.SYSTEM;
                BaseAddress = sys_sms_URL + "UpdateTrans";

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);

                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(Convert.ToInt16(CONFIG.ROUTES.Business.TIMEOUT)));
                var client = _client.CreateClient();
                //responseMsg = client.SendAsync(requestMsg).Result;
                //logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
                //logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Message: " + JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result)));
                await client.SendAsync(requestMsg);
            }
            catch (Exception e)
            {
                logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception: " + e.ToString() + "\n Message: " + e.Message);
            }
        }

        public async static void UpdateBulkTransaction(Telco_Bulk_SMS_Tran model, string scheme, string parameter)
        {
            try
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model));
                //var systemRoutes = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == model.CLIENT_CODE).ToList();
                //string sys_sms_URL = String.Empty;
                //if (systemRoutes.Count() > 0)
                //{
                //    sys_sms_URL = systemRoutes.FirstOrDefault().SYSURL;
                //}
                string sys_sms_URL = _settings.URL.SYSTEM;

                BaseAddress = sys_sms_URL + "UpdateTransBulk";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);
                var client = _client.CreateClient();
                //client.SendAsync(requestMsg);
                await client.SendAsync(requestMsg);
                //logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
            }
            catch (Exception e)
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception: " + e.ToString() + "\n Message: " + e.Message);
            }
        }

        public static void InsertTransactionAsync(TelcoSMSModel model, string scheme, string parameter)
        {
            try
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("ROUTES:" + JsonConvert.SerializeObject(_settings.URL.SYSTEM));
                //model.CHECKVALIDATE = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == model.CLIENT_CODE).FirstOrDefault().CHECKVALIDATE;
                //var systemRoutes = CONFIG.ROUTES.Business.ROUTE.Where(o => o.CHANNEL == model.CLIENT_CODE).ToList();
                //string sys_sms_URL = String.Empty;
                //if (systemRoutes.Count() > 0)
                //{
                //    sys_sms_URL = systemRoutes.FirstOrDefault().SYSURL;
                //}
                string sys_sms_URL = _settings.URL.SYSTEM;

                if (model.CHECKVALIDATE == "Y")
                {
                    BaseAddress = sys_sms_URL + "InsertTransWithChecking";
                }
                else
                {
                    BaseAddress = sys_sms_URL + "InsertTransWithoutChecking";
                }

                //BaseAddress = sys_sms_URL + "saveasync";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model) + "TELCO_REF_NO:" + model.TRN_REF_NO);
                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(Convert.ToInt16(CONFIG.ROUTES.Business.TIMEOUT)));
                var client = _client.CreateClient();
                responseMsg = client.SendAsync(requestMsg).Result;
                logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
            }
            catch (Exception ex)
            {
                if (CONFIG.LOG.Business.APILOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception:" + ex.StackTrace + "\n Message:" + ex.Message);
                new BaseController().logsAudit(new AuditLogModel
                {
                    HttpCode = Models.HttpStatusCode.InternalServerError,
                    HttpVerb = HttpVerbs.POST,
                    SourceUrl = BaseAddress,
                    PayLoadType = PayLoadType.RESPONSE,
                    LogLevel = Models.LogLevel.ERROR,
                    KBZMessageID = KBZRefNo,
                    PayLoad = JsonConvert.SerializeObject(model),
                    TransactionRefNo = model.TRN_REF_NO,
                    Message = ex.Message,
                    Exception = ex.ToString()
                });
            }
        }
    }
}