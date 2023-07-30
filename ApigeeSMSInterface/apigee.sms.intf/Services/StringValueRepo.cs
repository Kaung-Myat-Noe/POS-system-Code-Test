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
    public interface IStringValueRepo
    {
        ReturnResult CreateDataWithString(RedisStringValue parameter);
        Task<ReturnResult> CreateDataWithString_Task(RedisStringValue parameter);
        ReturnResult UpdateCache(RedisStringValue parameter);
        ReturnResult GetAllStringDataByKey(string Key);
        ReturnResult UpdateCacheWithFilter(RedisStringValue parameter, FilterEnum filterEnum);
        ReturnResult SendSMS(RedisStringValue parameter);
        ReturnResult UpdateCacheWithFilterByTelco(RedisStringValue parameter, FilterEnum filterEnum);
        ReturnResult DeleteCache(RedisStringValue parameter);
        ReturnResult ModifyDependencyConfigRepo(string modifiedDependency);
        Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name);
    }
    public class StringValueRepo : IStringValueRepo
    {
        public ReturnResult TelcoClientCodeList;
        public IDatabase db;
        private readonly List<PhonePrefix> phonePrefix;
        private readonly TimeSpan expireTime;
        private readonly IConfiguration Configuration;
        private static string Storage_depenency { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StringValueRepo> _logger;
        private readonly AppSettings _settings;
        public StringValueRepo(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<StringValueRepo> logger, IOptionsMonitor<AppSettings> settings, Redis redisCommon)
        {
            //_redis = redis;
            Configuration = configuration;
            _httpClientFactory = httpClientFactory;
            Storage_depenency = Configuration["dependency"];
            _configuration = configuration;
            _logger = logger;
            _settings = settings.CurrentValue;
            try
            {
                expireTime = TimeSpan.FromSeconds(Convert.ToDouble(Configuration["RedisExpireTimeInSeconds"]));
                if (Storage_depenency.Equals("Cache"))
                {
                    if (Redis._redis == null)
                    {
                        Redis.RedisConnection();
                    }
                    if (!Redis._redis.IsConnected)
                    {
                        Redis.RedisConnection();
                    }
                    db = Redis._redis.GetDatabase();

                }
                ReturnResult PhoneNumberPrefixes = GetAllStringDataByKey("PhonePrefix");
                if (PhoneNumberPrefixes.status != returnResultEnum.Fail_No_Record)
                {
                    phonePrefix = PhoneNumberPrefixes.returnResult.Value.ToObject<List<PhonePrefix>>() ?? throw new ArgumentNullException();
                }
                
            }
            catch (Exception ex)
            {
                //log
            }
        }
        public ReturnResult CreateDataWithString(RedisStringValue parameter)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {
                if (parameter == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameter));
                }
                var serialPlat = JsonConvert.SerializeObject(parameter);
                var stringSetResult = db.StringSet(parameter.Key, serialPlat);
                returnResult.status = returnResultEnum.Success;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = parameter;
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public ReturnResult GetAllStringDataByKey(string Key)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {

                var stringValue = db.StringGet(Key);

                if (!String.IsNullOrEmpty(stringValue) || !String.IsNullOrWhiteSpace(stringValue))
                {
                    var obj = JsonConvert.DeserializeObject<RedisStringValue>(stringValue);
                    returnResult.status = returnResultEnum.Success;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    returnResult.returnResult = obj;
                }
                else
                {
                    returnResult.status = returnResultEnum.Fail_No_Record;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    returnResult.returnResult = null;
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public ReturnResult ReplaceCacheByKey(RedisStringValue parameter)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {
                if (parameter == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameter));
                }
                var serialPlat = JsonConvert.SerializeObject(parameter);
                var stringSetResult = db.StringSetAsync(parameter.Key, serialPlat);
                returnResult.status = returnResultEnum.Success;
                returnResult.message = GetEnumDescription(returnResult.status);
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public ReturnResult UpdateCache(RedisStringValue parameter)
        {
            ReturnResult returnResult = new ReturnResult();
            List<SMSClientModel> smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException(); ;
            ReturnResult returnResultFromGetAllStringData = new ReturnResult();
            try
            {
                returnResultFromGetAllStringData = GetAllStringDataByKey(parameter.Key);
                if (returnResultFromGetAllStringData.returnResult != null)
                {
                    RedisStringValue? returnedData = returnResultFromGetAllStringData.returnResult as RedisStringValue;
                    List<SMSClientModel> result = returnedData.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException(); ;
                    List<SMSClientModel> modifiedResult = new List<SMSClientModel>();
                    if (result != null)
                    {
                        foreach (var item in smsClientModel)
                        {
                            foreach (var resultItem in result.ToList())
                            {
                                if (item.ClientCode.Equals(resultItem.ClientCode) && item.SubscriberNum.Equals(resultItem.SubscriberNum))
                                {
                                    result[result.IndexOf(resultItem)] = item;
                                }
                            }
                        }
                        RedisStringValue redisStringValue = new RedisStringValue();
                        redisStringValue.Key = parameter.Key;
                        redisStringValue.Value = JArray.FromObject(result);
                        ReplaceCacheByKey(redisStringValue);
                        returnResult.status = returnResultEnum.Success_Cache_Update;
                        returnResult.message = GetEnumDescription(returnResult.status);
                        returnResult.returnResult = smsClientModel;
                        return returnResult;
                    }
                    else
                    {
                        returnResult.status = returnResultEnum.Fail_No_Record;
                        returnResult.message = GetEnumDescription(returnResult.status);
                        returnResult.returnResult = smsClientModel;
                    }
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_Cache_Update;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public ReturnResult UpdateCacheWithFilter(RedisStringValue parameter, FilterEnum filterEnum)
        {
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            List<SMSClientModel> smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException();
            ReturnResult returnResultFromGetAllStringData = new ReturnResult();
            try
            {
                returnResultFromGetAllStringData = GetAllStringDataByKey(parameter.Key);
                if (returnResultFromGetAllStringData.returnResult != null)
                {
                    RedisStringValue? returnedData = returnResultFromGetAllStringData.returnResult as RedisStringValue;
                    List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>() ?? throw new ArgumentNullException();
                    List<SMSPackageLoadModel> modifiedResult = new List<SMSPackageLoadModel>();
                    if (result != null)
                    {
                        foreach (var item in smsClientModel)
                        {
                            foreach (var resultItem in result.ToList())
                            {
                                if (filterEnum == FilterEnum.By_Client_Telco)
                                {
                                    if (item.TelcoCode.Equals(resultItem.TelcoCode))
                                    {
                                        result[result.IndexOf(resultItem)].ModifiedDate = System.DateTime.Now;
                                        result[result.IndexOf(resultItem)].Provider = item.Provider;
                                    }
                                }
                                else if (filterEnum == FilterEnum.By_Client)
                                {
                                    result[result.IndexOf(resultItem)].ModifiedDate = System.DateTime.Now;
                                    result[result.IndexOf(resultItem)].Provider = item.Provider;
                                }
                            }
                        }
                        RedisStringValue redisStringValue = new RedisStringValue();
                        redisStringValue.Key = parameter.Key;
                        redisStringValue.Value = JArray.FromObject(result);
                        ReplaceCacheByKey(redisStringValue);
                        returnResult.status = returnResultEnum.Success_Cache_Update;
                        returnResult.message = GetEnumDescription(returnResult.status);
                        returnResult.returnResult = redisStringValue;
                        return returnResult;
                    }
                    else
                    {
                        returnResult.status = returnResultEnum.Fail_No_Record;
                        returnResult.message = GetEnumDescription(returnResult.status);
                        returnResult.returnResult = smsClientModel;
                    }
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_Cache_Update;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public ReturnResult UpdateCacheWithFilterByTelco(RedisStringValue parameter, FilterEnum filterEnum)
        {
            ReturnResult finalReturnResult = new ReturnResult();
            List<ReturnResult> returnResultList = new List<ReturnResult>();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            List<SMSPackageLoadModel> smsClientModel = parameter.Value.ToObject<List<SMSPackageLoadModel>>() ?? throw new ArgumentNullException();
            List<SMSPackageLoadModel> smsClients = new List<SMSPackageLoadModel>();
            ReturnResult returnResultFromGetAllStringData = new ReturnResult();
            try
            {
                if (filterEnum == FilterEnum.By_Telco)
                {
                    TelcoClientCodeList = GetAllStringDataByKey("TelcoClientCodeList");
                    string[] telcoClientCodeList = TelcoClientCodeList.returnResult.Value_String ?? throw new ArgumentNullException();
                    foreach (var telcoClientCode in telcoClientCodeList)
                    {
                        ReturnResult result = GetAllStringDataByKey(telcoClientCode);
                        if (result.status == returnResultEnum.Success)
                        {
                            smsClients = result.returnResult.Value.ToObject<List<SMSPackageLoadModel>>();
                            if (smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).Any())
                            {
                                smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).ToList().ForEach(c => c.ModifiedDate = System.DateTime.Now);
                                smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).ToList().ForEach(c => c.Provider = smsClientModel[0].Provider);
                                ReturnResult returnResult = new ReturnResult();
                                RedisStringValue redisStringValue = new RedisStringValue();
                                redisStringValue.Key = result.returnResult.Key;
                                redisStringValue.Value = JArray.FromObject(smsClients);
                                ReplaceCacheByKey(redisStringValue);
                                returnResult.status = returnResultEnum.Success_Cache_Update;
                                returnResult.message = GetEnumDescription(returnResult.status);
                                returnResult.returnResult = smsClients;
                                returnResultList.Add(returnResult);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ReturnResult returnResult = new ReturnResult();
                returnResult.status = returnResultEnum.Fail_Cache_Update;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
                returnResultList.Add(returnResult);
            }
            finalReturnResult.status = returnResultEnum.Success;
            finalReturnResult.message = GetEnumDescription(finalReturnResult.status);
            finalReturnResult.returnResult = new { returnResultList };
            return finalReturnResult;
        }
        public ReturnResult SendSMS(RedisStringValue parameter)
        {
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            SMSClientModel smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>().FirstOrDefault() ?? throw new ArgumentNullException();
            try
            {
                if (!String.IsNullOrEmpty(smsClientModel.SubscriberNum) || !String.IsNullOrWhiteSpace(smsClientModel.SubscriberNum))
                {
                    if (smsClientModel.SubscriberNum.StartsWith("0"))
                    {
                        string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                        if (phonePrefix == null)
                        {
                            returnResult.status = returnResultEnum.Update_Prefix_Cache;
                            returnResult.message = GetEnumDescription(returnResult.status);
                            returnResult.returnResult = smsClientModel;
                            return returnResult;
                        }
                        string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault(); ;
                        if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                        {
                            ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode);
                            if (provider_Data.returnResult != null)
                            {
                                RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                List<SMSClientModel> result = returnedData.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException();
                                smsClientModel.TelcoCode = phoneOperator;
                                smsClientModel.Provider = phoneOperator;
                                SMSClientModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode)).FirstOrDefault();
                                if (smsSendModel != null)
                                {
                                    returnResult.status = returnResultEnum.Success_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                    smsSendModel.Provider = phoneOperator;
                                    returnResult.returnResult = smsSendModel;
                                    // Implement SMS send here;
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                    smsClientModel.TelcoCode = phoneOperator;
                                    returnResult.returnResult = smsClientModel;
                                }
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                smsClientModel.TelcoCode = phoneOperator;
                                returnResult.returnResult = smsClientModel;
                            }
                            // Get Cache with Client Code
                        }
                        else
                        {
                            returnResult.status = returnResultEnum.Fail_SMS_Sent;
                            returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                            smsClientModel.TelcoCode = phoneOperator;
                            returnResult.returnResult = smsClientModel;
                            // Implement SMS couldn't send here;
                        }
                    }
                    // Phone Number starts with 09
                    else if (smsClientModel.SubscriberNum.StartsWith("9"))
                    {
                        string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 5);
                        string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault(); ;
                        if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                        {
                            ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode);
                            if (provider_Data.returnResult != null)
                            {
                                RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                List<SMSClientModel> result = returnedData.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException();
                                smsClientModel.TelcoCode = phoneOperator;
                                SMSClientModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode)).FirstOrDefault();
                                if (smsSendModel != null)
                                {
                                    returnResult.status = returnResultEnum.Success_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                    smsSendModel.Provider = phoneOperator;
                                    returnResult.returnResult = smsSendModel;
                                    // Implement SMS send here;
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                    smsClientModel.TelcoCode = phoneOperator;
                                    returnResult.returnResult = smsClientModel;
                                }
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                smsClientModel.TelcoCode = phoneOperator;
                                returnResult.returnResult = smsClientModel;
                            }
                            // Get Cache with Client Code
                        }
                        else
                        {
                            returnResult.status = returnResultEnum.Fail_SMS_Sent;
                            returnResult.message = GetEnumDescription(returnResult.status);
                            smsClientModel.TelcoCode = phoneOperator;
                            returnResult.returnResult = smsClientModel;
                            // Implement SMS couldn't send here;
                        }
                    }
                    // Phone Number starts with 959
                    else
                    {
                        returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                        returnResult.message = GetEnumDescription(returnResult.status);
                        returnResult.returnResult = smsClientModel;
                        //Invalid Subscriber Number
                    }
                }
                else
                {
                    returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    returnResult.returnResult = smsClientModel;
                    //Phone Number Null
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public Task<ReturnResult> CreateDataWithString_Task(RedisStringValue parameter)
        {
            Task<ReturnResult> finalReturn = null;
            ReturnResult returnResult = new ReturnResult();
            try
            {
                if (parameter == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameter));
                }

                var serialPlat = JsonConvert.SerializeObject(parameter);

                var stringSetResult = db.StringSet(parameter.Key, serialPlat);
                returnResult.status = returnResultEnum.Success;
                returnResult.message = GetEnumDescription(returnResult.status);
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            finalReturn = Task.FromResult(returnResult);
            return finalReturn;
        }
        public ReturnResult DeleteCache(RedisStringValue parameter)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {
                if (string.IsNullOrEmpty(parameter.Key) || string.IsNullOrWhiteSpace(parameter.Key))
                {
                    var server = Redis._redis.GetServer("kbz-smsgateway.2ai4u6.ng.0001.apse1.cache.amazonaws.com:6379");
                    server.FlushDatabase();
                }
                else
                {
                    db.KeyDelete(parameter.Key);
                }
                //var stringSetResult = db.StringSet(parameter.Key, serialPlat);
                returnResult.status = returnResultEnum.Success;
                returnResult.message = GetEnumDescription(returnResult.status);
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
        public async Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no,string biz_api_controller_name)
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
        private static string ConfigFileName => Path.Combine(Environment.CurrentDirectory.ToString(), $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        public ReturnResult ModifyDependencyConfigRepo(string modifiedDependency) {
            ReturnResult returnResult = new ReturnResult();
            returnResult.status = returnResultEnum.Pending;
            returnResult.message = GetEnumDescription(returnResult.status);
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            ReturnResult returnResultFromGetAllStringData = new ReturnResult();
            try
            {
                var config = ConfigurationOperations.ReadJson(ConfigFileName);
                config["dependency"] = modifiedDependency;
                ConfigurationOperations.SMSJsonUpdateDynamic(config, ConfigFileName, _logger);
                returnResult.status = returnResultEnum.Success;
                returnResult.message = GetEnumDescription(returnResult.status);
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_Cache_Update;
                returnResult.message = GetEnumDescription(returnResult.status);
                returnResult.returnResult = ex.ToString();
            }
            return returnResult;
        }
    }
}