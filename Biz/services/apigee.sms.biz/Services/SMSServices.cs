using apigee.sms.biz.Models;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace apigee.sms.biz.Services
{
    [ScopedService]
    public interface ISMSServices
    {
        Task<HttpResponseMessage> SMSSend(string scheme, string parameter, string kbz_ref_no, SMSClientModel smsClientModel);
    }
    public class SMSServices : ISMSServices
    {
        internal readonly ILogger<SMSServices> _logger;
        internal readonly AppSettings _settings;
        private readonly TimeSpan expireTime;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public SMSServices(IHttpClientFactory factory, ILogger<SMSServices> logger, IOptionsMonitor<AppSettings> setting)
        {
            _httpClientFactory = factory;
            _logger = logger;
            _settings = setting.CurrentValue;
        }

        public async Task<HttpResponseMessage> SMSSend(string scheme, string parameter, string kbz_ref_no, SMSClientModel smsClientModel)
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
                    RequestUri = new Uri(smsClientModel.ServiceURL1)
                    //Service URL 1,2,3 need to check
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
    }
}
