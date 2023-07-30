using apigee.sms.biz.Models;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace apigee.sms.biz.Services
{
    [ScopedService]
    public interface IConfigService
    {
        HttpResponseMessage GetConfig(string scheme, string param, string KBZRefNo);
        Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no);
        Task<HttpResponseMessage> SMSGetMethod(string scheme, string parameter, string kbz_ref_no, string system_api_controller_name);
        Task<HttpResponseMessage> ConfigtoBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, ProductConfig reqModel);

        Task<HttpResponseMessage> ConfigtoSys(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel);
        Task<HttpResponseMessage> ConfigtoSysGetMethod(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel);
        Task<HttpResponseMessage> ConfigtoSysExtendedTimeout(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel);
    }
    public class ConfigService : IConfigService
    {
        internal readonly IHttpClientFactory _factory;
        internal readonly ILogger<ConfigService> _logger;
        internal readonly AppSettings _settings;            
        private readonly IHttpClientFactory _httpClientFactory;        
        HttpResponseMessage responseMsg;
        public ConfigService(IHttpClientFactory factory, ILogger<ConfigService> logger, IOptionsMonitor<AppSettings> setting)
        {
            _factory = factory;
            _httpClientFactory = factory;
            _logger = logger;
            _settings = setting.CurrentValue;
        }

        public HttpResponseMessage GetConfig(string scheme, string param, string KBZRefNo)
        {            
            var _client = _factory.CreateClient("system");            
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, param);
            _client.DefaultRequestHeaders.Add("KBZ_REF_NO", KBZRefNo);
            
            responseMsg = _client.GetAsync("getconfig").Result;
            return responseMsg;
        }
        public async Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                //var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    //Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + "getconfig")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                return await Task.FromResult(client.SendAsync(requestMessage).Result);
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
        public async Task<HttpResponseMessage> SMSGetMethod(string scheme, string parameter, string kbz_ref_no, string system_api_controller_name)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                //var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    //Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + system_api_controller_name)
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                return await Task.FromResult(client.SendAsync(requestMessage).Result);
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
        public async Task<HttpResponseMessage> ConfigtoBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, ProductConfig reqModel)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + biz_api_controller_name)
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                HttpResponseMessage responsedMsg = await Task.FromResult(client.SendAsync(requestMessage).Result);
                var test1 = JsonConvert.DeserializeObject(responsedMsg.Content.ReadAsStringAsync().Result);
                return responsedMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        public async Task<HttpResponseMessage> ConfigtoSys(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + biz_api_controller_name)
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                HttpResponseMessage responsedMsg = await Task.FromResult(client.SendAsync(requestMessage).Result);
                var test1 = JsonConvert.DeserializeObject(responsedMsg.Content.ReadAsStringAsync().Result);
                return responsedMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
        public async Task<HttpResponseMessage> ConfigtoSysGetMethod(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + biz_api_controller_name)
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                HttpResponseMessage responsedMsg = await Task.FromResult(client.SendAsync(requestMessage).Result);
                var test1 = JsonConvert.DeserializeObject(responsedMsg.Content.ReadAsStringAsync().Result);
                return responsedMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        public async Task<HttpResponseMessage> ConfigtoSysExtendedTimeout(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(300);
                var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.SYSTEM + biz_api_controller_name)
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                HttpResponseMessage responsedMsg = await Task.FromResult(client.SendAsync(requestMessage).Result);
                var test1 = JsonConvert.DeserializeObject(responsedMsg.Content.ReadAsStringAsync().Result);
                return responsedMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSConfigCallback Exception Message: " + ex.ToString());
                resp.Error = ErrorCode.UnknownException;
                resp.Error.Details.Add(ErrorCode.OperationError);
                resp.Error.Details[0].ErrorDescription = ex.Message;
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = kbz_ref_no, Error = resp.Error }), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
