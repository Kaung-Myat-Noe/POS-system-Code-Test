using apigee.sms.biz.Models;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace apigee.sms.biz.Services
{
    [ScopedService]
    public interface ICRUDService
    {
        bool InsertTransaction(TelcoSMSModel model, string scheme, string parameter, out dynamic return_obj);
        void UpdateTransaction(TelcoSMSModel model, string scheme, string parameter);
        void InsertTransactionAsync(TelcoSMSModel model, string scheme, string parameter);
    }
    public class CRUDService : ICRUDService
    {
        private static ILogger<CRUDService> logger;
        private static HttpRequestMessage requestMsg;
        private static HttpResponseMessage responseMsg;
        private static string BaseAddress = null;
        internal readonly IHttpClientFactory _client;
        internal readonly AppSettings _settings;
        internal string KBZRefNo = string.Empty;
        public CRUDService(IHttpClientFactory httpClient, ILogger<CRUDService> nlog, IOptionsMonitor<AppSettings> setting)
        {
            logger = nlog;
            _client = httpClient;
            _settings = setting.CurrentValue;
        }

        public bool InsertTransaction(TelcoSMSModel model, string scheme, string parameter, out dynamic return_obj)
        {
            try
            {
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogInformation("System ROUTE:" + JsonConvert.SerializeObject(_settings.URL.SYSTEM));

                string sys_sms_URL = _settings.URL.OLDSYS;
                //InsertTransWithChecking
                if (model.CHECKVALIDATE == "Y")
                {
                    BaseAddress = sys_sms_URL + "InsertTransWithChecking";
                }
                else
                {
                    BaseAddress = sys_sms_URL + "InsertTransBulkWithChecking";
                }

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZRefNo", KBZRefNo);

                var client = _client.CreateClient();
                responseMsg = client.SendAsync(requestMsg).Result;
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    if (responseMsg != null)
                    {
                        if (!responseMsg.IsSuccessStatusCode)
                        {

                            if (responseMsg.Content != null)
                            {
                                logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Respond JSON:" + responseMsg.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }
                ResponseModel returnedData = null;



                if (responseMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    returnedData = JsonConvert.DeserializeObject<ResponseModel>(responseMsg.Content.ReadAsStringAsync().Result);
                    return_obj = ReturnMessage.Success;
                    return true;
                }
                else if (responseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return_obj = ReturnMessage.DuplicateTranRefNo;
                    return false;
                }
                else
                {
                    return_obj = new ErrorResponseModel { Code = returnedData.Error.Code, Message = returnedData.Error.Message };
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception:" + ex.ToString());
                return_obj = ReturnMessage.BusinessException;
                return false;
            }
        }

        public void UpdateTransaction(TelcoSMSModel model, string scheme, string parameter)
        {
            try
            {
                //if (_settings.LOG.Business.TEXTLOG == "Y")
                logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model));

                string sys_sms_URL = _settings.URL.OLDSYS;
                //BaseAddress = sys_sms_URL + "UpdateTrans";
                BaseAddress = sys_sms_URL + "UpdateTrans";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(BaseAddress) };
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                requestMsg.Headers.Add("KBZ_REF_NO", KBZRefNo);

                var client = _client.CreateClient();
                Task.Run(() => client.SendAsync(requestMsg));
            }
            catch (Exception e)
            {
                logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception: " + e.ToString() + "\n Message: " + e.Message);
            }
        }

        public void InsertTransactionAsync(TelcoSMSModel model, string scheme, string parameter)
        {
            try
            {
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogInformation("ROUTES:" + JsonConvert.SerializeObject(_settings.URL.SYSTEM));

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
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogInformation("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + ", Request JSON:" + JsonConvert.SerializeObject(model) + "TELCO_REF_NO:" + model.TRN_REF_NO);
                var client = _client.CreateClient();
                responseMsg = client.SendAsync(requestMsg).Result;
                logger.LogInformation("Returned Data From System: " + JsonConvert.DeserializeObject(responseMsg.Content.ReadAsStringAsync().Result));
            }
            catch (Exception ex)
            {
                if (_settings.LOG.Business.TEXTLOG == "Y")
                    logger.LogError("Ref_ID: " + KBZRefNo + ",TrxnRefNum: " + model.TRN_REF_NO + "\n Exception:" + ex.StackTrace + "\n Message:" + ex.Message);
            }
        }
    }
}
