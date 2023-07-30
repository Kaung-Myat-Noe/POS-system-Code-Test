using System.Text;
using System.Text.Json;
using apigee.sms.intf.Helper;
using apigee.sms.intf.Models;
using apigee.sms.intf.Utility;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using static apigee.sms.intf.Models.ReturnResultEnum;

namespace apigee.sms.intf.Services
{
    [ScopedService]
    public interface IConfigService
    {
        Task<HttpResponseMessage> ConfigtoBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, ProductConfig reqModel);
        Task<HttpResponseMessage> ProductFilterToBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel);
        Task<HttpResponseMessage> GetConfigfromBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name);
        Task<HttpResponseMessage> ProductFilterToBizExtended(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel);
    }
    public class ConfigService : IConfigService
    {
        private readonly IConnectionMultiplexer _redis;
        public ReturnResult TelcoClientCodeList;
        public IDatabase db;
        private List<PhonePrefix> phonePrefix;
        private readonly TimeSpan expireTime;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SMSService> _logger;
        private readonly AppSettings _settings;
        
        public ConfigService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SMSService> logger, IOptionsMonitor<AppSettings> settings)
        {
            Configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _settings = settings.CurrentValue;
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
                    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
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

        public async Task<HttpResponseMessage> GetConfigfromBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name)
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
                    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
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

        public async Task<HttpResponseMessage> ProductFilterToBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel)
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
                    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
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

        public async Task<HttpResponseMessage> ProductFilterToBizExtended(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, dynamic reqModel)
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
                    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
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
