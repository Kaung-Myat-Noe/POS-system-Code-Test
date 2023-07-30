using apigee.sms.intf.Helper;
using apigee.sms.intf.Models;
using apigee.sms.intf.Services;
using apigee.sms.intf.Utility;
using AspNetCore.ServiceRegistration.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Web;
using StackExchange.Redis;
using System.Net.Http.Headers;
using static apigee.sms.intf.Models.ReturnResultEnum;

namespace apigee.sms.intf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(CustomAuthorizeFilter))]
    public class SMSController : BaseController
    {
        private readonly ISmsService _sms;
        private ILogger<SMSController> logger;
        public SMSController(IConfiguration configuration,ISmsService smsService, ILogger<SMSController> nlog)
        {
            _sms = smsService;
            logger = nlog;            
        }

        #region SMS Send 
        [HttpPost("SendSMS/", Name = "SendSMS")]
        public async Task<dynamic> SendSMS([FromBody] SMS_Send_Request_Model param)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            dynamic? result = null;
             
            Request.Headers.TryGetValue("KBZ_REF_NO", out var KBZRefNo);
            if (String.IsNullOrEmpty(KBZRefNo)) 
            {
                KBZRefNo = Guid.NewGuid().ToString();
            }
            dynamic logMsg = param;
            logger.LogInformation("API in. KBZ Reference Number:"+ KBZRefNo);
            List<ReturnResult> dataList = new List<ReturnResult>();
            ReturnResult data = new ReturnResult();
            try
            {
                //SMSClientModel paramObject = param.ToObject<SMSClientModel>() ?? throw new ArgumentNullException();
                SMS_Send_Request_Model paramObject = param;
                //JObject paramObject = param;
                if (paramObject != null)
                {
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    RedisStringValue redisHashValue = new RedisStringValue();
                    redisHashValue.Key = paramObject.ClientCode;
                    redisHashValue.Value.Add(JObject.FromObject(param));
                    data = await _sms.SendSMS(redisHashValue, scheme,parameter, KBZRefNo, param);
                }
                else
                {
                    data.status = returnResultEnum.Empty_Paramater;
                    data.message = GetEnumDescription(data.status);
                }
            }
            catch (Exception ex)
            {
                data.status = returnResultEnum.Fail;
                data.message = GetEnumDescription(data.status);
                data.returnResult = ex.ToString();
                return StatusCode(StatusCodes.Status500InternalServerError, new { KBZRefNo = KBZRefNo, Data = data, Error = ex.ToString() });
            }

            HttpResponseMessage httpResponseMessage = data.returnResult;
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo);
            //var test = JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result);
            return new ObjectResult(JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)httpResponseMessage.StatusCode) };
            //return StatusCode(StatusCodes.Status200OK, new { KBZRefNo = KBZRefNo, Data = result, Error = new { } });
        }
        #endregion

        #region SMS Bulk Send 
        [HttpPost("SendSMSBulk/", Name = "SendSMSBulk")]
        public async Task<dynamic> SendSMSBulk([FromBody] SMS_Send_Request_Model param)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            dynamic? result = null;
            string KBZRefNo = Guid.NewGuid().ToString();
            dynamic logMsg = param;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            List<ReturnResult> dataList = new List<ReturnResult>();
            ReturnResult data = new ReturnResult();
            try
            {
                //SMSClientModel paramObject = param.ToObject<SMSClientModel>() ?? throw new ArgumentNullException();
                SMS_Send_Request_Model paramObject = param;
                //JObject paramObject = param;
                if (paramObject != null)
                {
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    RedisStringValue redisHashValue = new RedisStringValue();
                    redisHashValue.Key = paramObject.ClientCode;
                    redisHashValue.Value.Add(JObject.FromObject(param));
                    data = await _sms.SendSMSBulk(redisHashValue, scheme, parameter, KBZRefNo, param);
                }
                else
                {
                    data.status = returnResultEnum.Empty_Paramater;
                    data.message = GetEnumDescription(data.status);
                }
             }
            catch (Exception ex)
            {
                data.status = returnResultEnum.Fail;
                data.message = GetEnumDescription(data.status);
                data.returnResult = ex.ToString();
                return StatusCode(StatusCodes.Status500InternalServerError, new { KBZRefNo = KBZRefNo, Data = data, Error = ex.ToString() });
            }

            HttpResponseMessage httpResponseMessage = data.returnResult;
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo);
            var test = JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result);
            return new ObjectResult(JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)httpResponseMessage.StatusCode) };
            //return StatusCode(StatusCodes.Status200OK, new { KBZRefNo = KBZRefNo, Data = result, Error = new { } });
        }
        #endregion

        #region Update Cache
        [HttpPost("UpdateCacheDataWithFilter_SMS/", Name = "UpdateCacheDataWithFilter_SMS")]
        public ActionResult<dynamic> UpdateCacheDataWithFilter_SMS([FromBody] Update_Request_Model param)
        {
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            dynamic? result = null;
            List<ReturnResult> dataList = new List<ReturnResult>();
            try
            {
                SMSClientModel paramObject = JsonConvert.DeserializeObject<SMSClientModel>(JsonConvert.SerializeObject(param)) ?? throw new ArgumentNullException();
                //JObject paramObject = param;
                 if (paramObject != null)
                {
                    ReturnResult data = new ReturnResult();
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    if (!string.IsNullOrEmpty(paramObject.ClientCode) && !string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
                    {
                        RedisStringValue redisHashValue = new RedisStringValue();
                        redisHashValue.Key = paramObject.ClientCode;
                        redisHashValue.Value.Add(JObject.FromObject(param));
                        data = _sms.UpdateCacheWithFilter(redisHashValue, FilterEnum.By_Client_Telco);
                        data.message = "By Product and Telco";
                        logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(data) + " .Error: ");
                    }
                    else if (!string.IsNullOrEmpty(paramObject.ClientCode) && string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
                    {
                        RedisStringValue redisHashValue = new RedisStringValue();
                        redisHashValue.Key = paramObject.ClientCode;
                        redisHashValue.Value.Add(JObject.FromObject(param));
                        data = _sms.UpdateCacheWithFilter(redisHashValue, FilterEnum.By_Client);
                        data.message = "By Product";
                        logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(data) + " .Error: ");
                    }
                    else if (string.IsNullOrEmpty(paramObject.ClientCode) && !string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
                    {
                        RedisStringValue redisHashValue = new RedisStringValue();
                        redisHashValue.Key = paramObject.ClientCode;
                        redisHashValue.Value.Add(JObject.FromObject(param));
                        data = _sms.UpdateCacheWithFilterByTelco(redisHashValue, FilterEnum.By_Telco);
                        data.message = "By Telco";
                        logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(data) + " .Error: ");
                    }
                    else
                    {
                        data.status = returnResultEnum.Fail_VALUE_KEY_NULL;
                        data.message = GetEnumDescription(data.status);
                        logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(data) + " .Error: ");
                    }
                    dataList.Add(data);
                }
                else
                {
                    ReturnResult data = new ReturnResult();
                    data.status = returnResultEnum.Empty_Paramater;
                    data.message = GetEnumDescription(data.status);
                    dataList.Add(data);
                }

            }
            catch (Exception ex)
            {
                ReturnResult data = new ReturnResult();
                data.status = returnResultEnum.Fail;
                data.message = GetEnumDescription(data.status);
                data.returnResult = ex.ToString();
                dataList.Add(data);
                logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: "+ex.ToString());
            }
            result = new { KBZRefNo = KBZRefNo, Data = new { dataList }, Error = new{ } };
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: ");
            //result = new { dataList };
            return result;
        }
        #endregion

        #region SMS Send For Testing
        [HttpPost("TestSendSMS/", Name = "TestSendSMS")]
        public async Task<dynamic> TestSendSMS([FromBody] SMS_Send_Request_GATEWAY_Model param)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            dynamic? result = null;

            Request.Headers.TryGetValue("KBZ_REF_NO", out var KBZRefNo);
            if (String.IsNullOrEmpty(KBZRefNo))
            {
                KBZRefNo = Guid.NewGuid().ToString();
            }
            dynamic logMsg = param;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            List<ReturnResult> dataList = new List<ReturnResult>();
            ReturnResult data = new ReturnResult();
            try
            {
                //SMSClientModel paramObject = param.ToObject<SMSClientModel>() ?? throw new ArgumentNullException();
                SMS_Send_Request_GATEWAY_Model paramObject = param;
                //JObject paramObject = param;
                if (paramObject != null)
                {
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    RedisStringValue redisHashValue = new RedisStringValue();
                    redisHashValue.Key = paramObject.ClientCode;
                    redisHashValue.Value.Add(JObject.FromObject(param));
                    data = await _sms.TestSendSMS(redisHashValue, scheme, parameter, KBZRefNo, param);
                }
                else
                {
                    data.status = returnResultEnum.Empty_Paramater;
                    data.message = GetEnumDescription(data.status);
                }
            }
            catch (Exception ex)
            {
                data.status = returnResultEnum.Fail;
                data.message = GetEnumDescription(data.status);
                data.returnResult = ex.ToString();
                return StatusCode(StatusCodes.Status500InternalServerError, new { KBZRefNo = KBZRefNo, Data = data, Error = ex.ToString() });
            }

            HttpResponseMessage httpResponseMessage = data.returnResult;
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo);
            //var test = JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result);
            return new ObjectResult(JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)httpResponseMessage.StatusCode) };
            //return StatusCode(StatusCodes.Status200OK, new { KBZRefNo = KBZRefNo, Data = result, Error = new { } });
        }
        #endregion
    }
}
