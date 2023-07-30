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
    public interface ISmsService
    {
        Task<ReturnResult> SendSMS(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_Model smsRequestModel);
        Task<ReturnResult> SendSMSBulk(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_Model smsRequestModel);
        Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name);
        ReturnResult ModifyDependencyConfigRepo(string modifiedDependency);
        ReturnResult GetAllStringDataByKey(string Key, string filePath = null, string filterKey = null);
        ReturnResult GetDataConfig(string Key, string filePath = null, string filterKey = null);
        ReturnResult UpdateCacheWithFilter(RedisStringValue parameter, FilterEnum filterEnum);
        ReturnResult UpdateCacheWithFilterByTelco(RedisStringValue parameter, FilterEnum filterEnum);
        ReturnResult GetAllConfig();
        Task<ReturnResult> TestSendSMS(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_GATEWAY_Model smsRequestModel);
    }
    public class SMSService : ISmsService
    {
        public ReturnResult TelcoClientCodeList;
        public IDatabase db;
        private List<PhonePrefix> phonePrefix;
        private readonly TimeSpan expireTime;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SMSService> _logger;
        private readonly AppSettings _settings;
        private static string Storage_depenency { get; set; }
        private static string phoneLength { get; set; }
        private StringValueRepo stringValueRepo;
        private Redis _redisCommon;

        private static string SMSProductJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSProductsCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSPhonePrefixJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSPhonePrefixCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSTelcoClientJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSTelcoClientCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        public SMSService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SMSService> logger, IOptionsMonitor<AppSettings> settings, Redis redisCommon)
        {
            _redisCommon = redisCommon;
            Configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            Storage_depenency = Configuration["dependency"];
            phoneLength = Configuration["PhoneNumberLength"];
            _logger = logger;
            _settings = settings.CurrentValue;
            try
            {
                expireTime = TimeSpan.FromSeconds(Convert.ToDouble(Configuration["RedisExpireTimeInSeconds"]));
                if (Storage_depenency.Equals("Cache"))
                {
                    if (Redis._redis == null || !Redis._redis.IsConnected)
                    {
                        Redis.RedisConnection();
                    }
                    db = Redis._redis.GetDatabase();

                }
                ReturnResult PhoneNumberPrefixes = GetAllStringDataByKey("PhonePrefix", SMSPhonePrefixJsonFileName);
                if (PhoneNumberPrefixes.status != returnResultEnum.Fail_No_Record)
                {
                    phonePrefix = PhoneNumberPrefixes.returnResult.Value.ToObject<List<PhonePrefix>>();
                }

            }
            catch (Exception ex)
            {
                //log
                _logger.LogError(ex.ToString());
            }
        }
        public Lazy<ConnectionMultiplexer> LazyConnection
        {
            get
            {
                return new Lazy<ConnectionMultiplexer>(
                    () => ConnectionMultiplexer.Connect(
                        Configuration.GetConnectionString("RedisCacheAWSConnection")));
            }
        }
        public ReturnResult GetAllStringDataByKey(string Key, string filePath = null, string filterKey = null)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {

                dynamic stringValue = null;
                if (Storage_depenency.Contains("Cache"))
                {
                    _logger.LogInformation("Getting Data form cache");
                    db = Redis._redis.GetDatabase();
                    stringValue = db.StringGet(Key);
                }
                else if (Storage_depenency.Contains("Json"))
                {
                    _logger.LogInformation("Getting Data form json");
                    stringValue = JsonConvert.SerializeObject(ConfigurationOperations.ReadJson(filePath));
                    if (filterKey != null)
                    {
                        List<RedisStringValue> jsonData = JsonConvert.DeserializeObject<List<RedisStringValue>>(stringValue);
                        stringValue = JsonConvert.SerializeObject(jsonData.Where(x => x.Key.Equals(filterKey)).FirstOrDefault());
                    }
                }

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
        public ReturnResult ReplaceCacheByKey(RedisStringValue parameter, string filePath = null)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {
                if (parameter == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(parameter));
                }
                dynamic stringSetResult = null;
                if (Storage_depenency.Contains("Cache"))
                {
                    var serialPlat = JsonConvert.SerializeObject(parameter);
                    stringSetResult = db.StringSetAsync(parameter.Key, serialPlat);
                    _logger.LogInformation("Replacing Cache");
                }
                else if (Storage_depenency.Contains("Json"))
                {
                    JArray jsonArray = ConfigurationOperations.ReadJson(filePath);
                    _logger.LogInformation("Replacing Json");
                    //foreach (var item in jsonArray) 
                    //{
                    //    if (item["Key"].ToString().Equals(parameter.Key)) {
                    //        jsonArray[jsonArray.IndexOf(item)]["Value"] = parameter.Value;
                    //    }
                    //}
                    jsonArray.Where(x => x["Key"].ToString().Equals(parameter.Key)).ToList().ForEach(c => c["Value"] = parameter.Value);

                    ConfigurationOperations.SMSJsonUpdateDynamic(jsonArray, filePath, _logger);
                    //List<RedisStringValue> jsonData = JsonConvert.DeserializeObject<List<RedisStringValue>>(stringSetResult);
                }

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
        public async Task<ReturnResult> SendSMS(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_Model smsRequestModel)
        {
            string tokenUserName = Common.GetClaimUsername(httpparameter);
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            SMSClientModel smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>().FirstOrDefault();
            smsClientModel.SubscriberNum = smsClientModel.SubscriberNum.Trim();
            try
            {
                if (!String.IsNullOrEmpty(smsClientModel.SubscriberNum) || !String.IsNullOrWhiteSpace(smsClientModel.SubscriberNum))
                {
                    string[] subscriberArray = smsClientModel.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (subscriberArray.Count() == 1)
                    {
                        if (smsClientModel.SubscriberNum.Length < int.Parse(phoneLength))
                        {
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.InvalidPhoneNumberError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                            _logger.LogError("Phone Error" + JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result));
                            return returnResult;
                        }
                        // Phone Number starts with 09
                        if (smsClientModel.SubscriberNum.StartsWith("0") || smsClientModel.SubscriberNum.StartsWith("9"))
                        {
                            string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 5);
                            if (!subscriberPrefix.StartsWith("959"))
                            {
                                if (subscriberPrefix.StartsWith("9"))
                                {
                                    smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                                    subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                                }
                                else
                                {
                                    subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                                }

                            }
                            _logger.LogInformation("Phone start with: " + subscriberPrefix);
                            if (phonePrefix == null)
                            {
                                returnResult.status = returnResultEnum.Update_Prefix_Cache;
                                returnResult.message = GetEnumDescription(returnResult.status);
                                returnResult.returnResult = smsClientModel;
                                return returnResult;
                            }
                            string dependency = Configuration["dependency"];
                            string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault();
                            if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                            {

                                ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                                if (provider_Data.returnResult != null)
                                {
                                    RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                    List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                    smsClientModel.TelcoCode = phoneOperator;
                                    smsClientModel.Provider = phoneOperator;
                                    SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                    if (smsSendModel != null)
                                    {
                                        returnResult.status = returnResultEnum.Success_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                        smsSendModel.Provider = phoneOperator;
                                        returnResult.returnResult = smsSendModel;
                                        // Implement SMS send here;
                                        smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                        smsSendModel.Message = smsRequestModel.Message;
                                        smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                        smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                        smsSendModel.Msg_type = smsRequestModel.Msg_type;
                                        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                        _logger.LogWarning("Environment" + env);
                                        if (env.Equals("Development"))
                                        {
                                            _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("UAT"))
                                        {
                                            _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("PRODUCTION"))
                                        {
                                            _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                        }
                                        else
                                        {
                                            _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                            returnResult.message = GetEnumDescription(returnResult.status);
                                            HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                            httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                            BaseRespError responseModel = ErrorCode.WrongEnv;
                                            httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                            returnResult.returnResult = httpResponseErrorMessage;
                                            return returnResult;
                                        }
                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.OK;
                                        _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                        httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                    else
                                    {
                                        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                        smsClientModel.TelcoCode = phoneOperator;

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                        BaseRespError responseModel = ErrorCode.ClientCodeError;
                                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                    smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                    BaseRespError responseModel = ErrorCode.ClientCodeError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                // Get Cache with Client Code
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                                smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                BaseRespError responseModel = ErrorCode.CacheError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                // Implement SMS couldn't send here;
                            }
                        }
                        // Phone Number starts with 959
                        else if (smsClientModel.SubscriberNum.StartsWith("959"))
                        {
                            string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 5);
                            _logger.LogInformation("Phone start with 9: " + subscriberPrefix);
                            string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault();
                            if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                            {
                                ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName);
                                if (provider_Data.returnResult != null)
                                {
                                    RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                    List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                    smsClientModel.TelcoCode = phoneOperator;
                                    SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                    if (smsSendModel != null)
                                    {
                                        returnResult.status = returnResultEnum.Success_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                        smsSendModel.Provider = phoneOperator;
                                        returnResult.returnResult = smsSendModel;
                                        // Implement SMS send here;
                                        smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                        smsSendModel.Message = smsRequestModel.Message;
                                        smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                        smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                        smsSendModel.Msg_type = smsRequestModel.Msg_type;

                                        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                        _logger.LogWarning("Environment" + env);
                                        if (env.Equals("Development"))
                                        {
                                            _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("UAT"))
                                        {
                                            _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("PRODUCTION"))
                                        {
                                            _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                        }
                                        else
                                        {
                                            _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                            returnResult.message = GetEnumDescription(returnResult.status);
                                            HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                            httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                            BaseRespError responseModel = ErrorCode.WrongEnv;
                                            httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                            returnResult.returnResult = httpResponseErrorMessage;
                                            return returnResult;
                                        }

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                                        _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                        httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                    else
                                    {
                                        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                        smsClientModel.TelcoCode = phoneOperator;

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                        BaseRespError responseModel = ErrorCode.ClientCodeError;
                                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                    smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                    BaseRespError responseModel = ErrorCode.ProviderError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                // Get Cache with Client Code
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                                smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                BaseRespError responseModel = ErrorCode.CacheError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                // Implement SMS couldn't send here;
                            }
                        }
                        #region In case of subscriber starting with only "9"
                        //else if (smsClientModel.SubscriberNum.StartsWith("9"))
                        //{
                        //    //smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                        //    string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 3);
                        //    if (!subscriberPrefix.Contains("959")) {
                        //        smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                        //        subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                        //    }
                        //    _logger.LogInformation("Phone start with 9: " + subscriberPrefix);
                        //    if (phonePrefix == null)
                        //    {
                        //        returnResult.status = returnResultEnum.Update_Prefix_Cache;
                        //        returnResult.message = GetEnumDescription(returnResult.status);
                        //        returnResult.returnResult = smsClientModel;
                        //        return returnResult;
                        //    }
                        //    string dependency = Configuration["dependency"];
                        //    string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault(); ;
                        //    if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                        //    {

                        //        ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                        //        if (provider_Data.returnResult != null)
                        //        {
                        //            RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                        //            List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>() ?? throw new ArgumentNullException();
                        //            smsClientModel.TelcoCode = phoneOperator;
                        //            smsClientModel.Provider = phoneOperator;
                        //            SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode)).FirstOrDefault();
                        //            if (smsSendModel != null)
                        //            {
                        //                returnResult.status = returnResultEnum.Success_SMS_Sent;
                        //                returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                        //                smsSendModel.Provider = phoneOperator;
                        //                returnResult.returnResult = smsSendModel;
                        //                // Implement SMS send here;
                        //                smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                        //                smsSendModel.Message = smsRequestModel.Message;
                        //                smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                        //                smsSendModel.ClientCode = smsRequestModel.ClientCode;
                        //                smsSendModel.Msg_type = smsRequestModel.Msg_type;
                        //                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        //                if (env.Equals("Development"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                        //                }
                        //                else if (env.Contains("UAT"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                        //                }
                        //                else if (env.Contains("Prod"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                        //                }
                        //                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;

                        //                httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                        //                returnResult.returnResult = httpResponseMessage;
                        //            }
                        //            else
                        //            {
                        //                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //                returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                        //                smsClientModel.TelcoCode = phoneOperator;

                        //                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //                BaseRespError responseModel = ErrorCode.ClientCodeError;
                        //                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //                returnResult.returnResult = httpResponseMessage;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //            returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                        //            smsClientModel.TelcoCode = phoneOperator;

                        //            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //            BaseRespError responseModel = ErrorCode.ProviderError;
                        //            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //            returnResult.returnResult = httpResponseMessage;
                        //        }
                        //        // Get Cache with Client Code
                        //    }
                        //    else
                        //    {
                        //        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //        returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                        //        smsClientModel.TelcoCode = phoneOperator;

                        //        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //        BaseRespError responseModel = ErrorCode.CacheError;
                        //        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //        returnResult.returnResult = httpResponseMessage;
                        //        // Implement SMS couldn't send here;
                        //    }
                        //}
                        #endregion
                        else
                        {
                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                            returnResult.message = GetEnumDescription(returnResult.status);
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.InvalidSubscriberError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                            //Invalid Subscriber Number
                        }
                    }
                    else
                    {
                        List<string> phoneOperator = new List<string>();
                        foreach (string item in subscriberArray)
                        {
                            if (item.Length < int.Parse(phoneLength))
                            {
                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                BaseRespError responseModel = ErrorCode.InvalidPhoneNumberError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                _logger.LogError("Phone Error" + JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result));
                                return returnResult;
                            }
                            string subscriberPrefix = item.Substring(0, 3);
                            if (item.StartsWith("0") || item.StartsWith("9"))
                            {
                                if (!subscriberPrefix.StartsWith("959"))
                                {
                                    if (subscriberPrefix.StartsWith("9"))
                                    {
                                        string sub = "0" + item;
                                        subscriberPrefix = sub.Substring(0, 4);
                                    }
                                }
                                else
                                {
                                    subscriberPrefix = item.Substring(0, 5);
                                }
                            }
                            phoneOperator.Add(phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault());
                        }
                        if (phoneOperator.Count > 0 && !phoneOperator.Distinct().Skip(1).Any())
                        {

                            ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                            if (provider_Data.returnResult != null)
                            {
                                RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                smsClientModel.TelcoCode = phoneOperator[0];
                                smsClientModel.Provider = phoneOperator[0];
                                SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                if (smsSendModel != null)
                                {
                                    returnResult.status = returnResultEnum.Success_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status);
                                    //smsSendModel.Provider = phoneOperator;
                                    returnResult.returnResult = smsSendModel;
                                    // Implement SMS send here;
                                    smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                    smsSendModel.Message = smsRequestModel.Message;
                                    smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                    smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                    smsSendModel.Msg_type = smsRequestModel.Msg_type;
                                    smsSendModel.Provider = phoneOperator[0];
                                    if (!smsSendModel.Gateway.Equals("MGateBulk"))
                                    {
                                        HttpResponseMessage httpResponseMessageforGateway = new HttpResponseMessage();
                                        httpResponseMessageforGateway.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                        BaseRespError responseModel = ErrorCode.InvalidGateway;
                                        httpResponseMessageforGateway.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessageforGateway;
                                        _logger.LogError("Gate Way Error" + JsonConvert.DeserializeObject(httpResponseMessageforGateway.Content.ReadAsStringAsync().Result));
                                        return returnResult;
                                    }
                                    string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                    _logger.LogWarning("Environment" + env);
                                    if (env.Equals("Development"))
                                    {
                                        _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                    }
                                    else if (env.Contains("UAT"))
                                    {
                                        _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                    }
                                    else if (env.Contains("PRODUCTION"))
                                    {
                                        _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                    }
                                    else
                                    {
                                        _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                        returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                        returnResult.message = GetEnumDescription(returnResult.status);
                                        HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                        httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                        BaseRespError responseModel = ErrorCode.WrongEnv;
                                        httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseErrorMessage;
                                        return returnResult;
                                    }
                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                                    _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                    httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                    //smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                    BaseRespError responseModel = ErrorCode.ClientCodeError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                //smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                BaseRespError responseModel = ErrorCode.ProviderError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                            }
                        }
                        else
                        {
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.ProvierAreDifferent;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                        }

                    }
                }
                else
                {
                    returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    BaseRespError responseModel = ErrorCode.PhoneNumberNullError;
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                    returnResult.returnResult = httpResponseMessage;
                    //Phone Number Null
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                returnResult.message = GetEnumDescription(returnResult.status);
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                BaseRespError responseModel = ErrorCode.UnknownException;
                responseModel.Details.Add(new BaseRespErrorDetail { ErrorCode = "400", ErrorDescription = ex.ToString() });
                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                returnResult.returnResult = httpResponseMessage;
                _logger.LogError("Message: " + ex.ToString());
                _logger.LogError("Innser Message: " + ex.InnerException.ToString());
            }
            return returnResult;
        }

        public async Task<ReturnResult> SendSMSBulk(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_Model smsRequestModel)
        {
            string tokenUserName = Common.GetClaimUsername(httpparameter);
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            SMSClientModel smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>().FirstOrDefault();
            smsClientModel.SubscriberNum = smsClientModel.SubscriberNum.Trim();
            try
            {
                if (!String.IsNullOrEmpty(smsClientModel.SubscriberNum) || !String.IsNullOrWhiteSpace(smsClientModel.SubscriberNum))
                {
                    string[] subscriberArray = smsClientModel.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    List<string> phoneOperator = new List<string>();
                    foreach (string item in subscriberArray)
                    {
                        if (item.Length < int.Parse(phoneLength))
                        {
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.InvalidPhoneNumberError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                            _logger.LogError("Phone Error" + JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result));
                            return returnResult;
                        }
                        string subscriberPrefix = item.Substring(0, 3);
                        if (item.StartsWith("0") || item.StartsWith("9"))
                        {
                            if (!subscriberPrefix.StartsWith("959"))
                            {
                                if (subscriberPrefix.StartsWith("9"))
                                {
                                    string sub = "0" + item;
                                    subscriberPrefix = sub.Substring(0, 4);
                                }
                            }
                            else
                            {
                                subscriberPrefix = item.Substring(0, 5);
                            }
                        }
                        phoneOperator.Add(phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault());
                    }
                    if (phoneOperator.Count > 0 && !phoneOperator.Distinct().Skip(1).Any())
                    {

                        ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                        if (provider_Data.returnResult != null)
                        {
                            RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                            List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                            smsClientModel.TelcoCode = phoneOperator[0];
                            smsClientModel.Provider = phoneOperator[0];
                            SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                            if (smsSendModel != null)
                            {
                                returnResult.status = returnResultEnum.Success_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status);
                                //smsSendModel.Provider = phoneOperator;
                                returnResult.returnResult = smsSendModel;
                                // Implement SMS send here;
                                smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                smsSendModel.Message = smsRequestModel.Message;
                                smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                smsSendModel.Msg_type = smsRequestModel.Msg_type;
                                smsSendModel.Provider = phoneOperator[0];
                                if (!smsSendModel.Gateway.Equals("MGateBulk"))
                                {
                                    HttpResponseMessage httpResponseMessageforGateway = new HttpResponseMessage();
                                    httpResponseMessageforGateway.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                    BaseRespError responseModel = ErrorCode.InvalidGateway;
                                    httpResponseMessageforGateway.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessageforGateway;
                                    _logger.LogError("Gate Way Error" + JsonConvert.DeserializeObject(httpResponseMessageforGateway.Content.ReadAsStringAsync().Result));
                                    return returnResult;
                                }
                                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                _logger.LogWarning("Environment" + env);
                                if (env.Equals("Development"))
                                {
                                    _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                }
                                else if (env.Contains("UAT"))
                                {
                                    _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                }
                                else if (env.Contains("PRODUCTION"))
                                {
                                    _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                    smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                }
                                else
                                {
                                    _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                    returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                    returnResult.message = GetEnumDescription(returnResult.status);
                                    HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                    httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                    BaseRespError responseModel = ErrorCode.WrongEnv;
                                    httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseErrorMessage;
                                    return returnResult;
                                }
                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;

                                httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                                returnResult.returnResult = httpResponseMessage;
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                //smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                BaseRespError responseModel = ErrorCode.ClientCodeError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                            }
                        }
                        else
                        {
                            returnResult.status = returnResultEnum.Fail_SMS_Sent;
                            returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                            //smsClientModel.TelcoCode = phoneOperator;

                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                            BaseRespError responseModel = ErrorCode.ProviderError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                        }
                    }
                    else
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        BaseRespError responseModel = ErrorCode.ProvierAreDifferent;
                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        returnResult.returnResult = httpResponseMessage;
                    }
                }
                else
                {
                    returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    BaseRespError responseModel = ErrorCode.PhoneNumberNullError;
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                    returnResult.returnResult = httpResponseMessage;
                    //Phone Number Null
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                returnResult.message = GetEnumDescription(returnResult.status);
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                BaseRespError responseModel = ErrorCode.UnknownException;
                responseModel.Details.Add(new BaseRespErrorDetail { ErrorCode = "400", ErrorDescription = ex.ToString() });
                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                returnResult.returnResult = httpResponseMessage;
                _logger.LogError("Message: " + ex.ToString());
            }
            return returnResult;
        }

        public async Task<HttpResponseMessage> SMSConfigCallback(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name)
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

        public async Task<HttpResponseMessage> SMSSendToBiz(string scheme, string parameter, string kbz_ref_no, string biz_api_controller_name, SMSPackageLoadModel reqModel)
        {
            ResponseModel resp = new ResponseModel();
            try
            {

                var client = _httpClientFactory.CreateClient();
                var test = JsonConvert.SerializeObject(reqModel);
                var content = new StringContent(JsonConvert.SerializeObject(reqModel), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
                };
                if (reqModel.Gateway.Equals("MGate"))
                {
                    requestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = content,
                        RequestUri = new Uri(_settings.URL.MGate.BIZ + biz_api_controller_name)
                    };
                }
                else if (reqModel.Gateway.Equals("MGateBulk"))
                {
                    if (_settings.URL.MGateBulk.BIZ.Contains("/v1/api/sms/"))
                    {
                        requestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            Content = content,
                            RequestUri = new Uri(_settings.URL.MGateBulk.BIZ + "wrapperbulksms")
                        };
                    }
                    else
                    {
                        requestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            Content = content,
                            RequestUri = new Uri(_settings.URL.MGateBulk.BIZ + biz_api_controller_name)
                        };
                    }

                }
                else if (reqModel.Gateway.Equals("SMSBrix"))
                {
                    requestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = content,
                        RequestUri = new Uri(_settings.URL.SMSBrix.BIZ + biz_api_controller_name)
                    };
                }
                //HttpRequestMessage requestMessage = new HttpRequestMessage
                //{
                //    Method = HttpMethod.Post,
                //    Content = content,
                //    RequestUri = new Uri(_settings.URL.BIZ + biz_api_controller_name)
                //};
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", kbz_ref_no);
                HttpResponseMessage responsedMsg = await Task.FromResult(client.SendAsync(requestMessage).Result);
                //var test1 = JsonConvert.DeserializeObject(responsedMsg.Content.ReadAsStringAsync().Result);
                return responsedMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError("KBZ_REF_NO: " + kbz_ref_no + ", SMSSend Exception Message: " + ex.ToString());
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
        public ReturnResult ModifyDependencyConfigRepo(string modifiedDependency)
        {
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
                //var test = _configuration["dependency"];
                //_configuration["dependency"] = modifiedDependency;
                //var test1 = _configuration["dependency"];
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
        public ReturnResult UpdateCacheWithFilter(RedisStringValue parameter, FilterEnum filterEnum)
        {
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            List<SMSClientModel> smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>() ?? throw new ArgumentNullException();
            ReturnResult returnResultFromGetAllStringData = new ReturnResult();
            try
            {
                returnResultFromGetAllStringData = GetAllStringDataByKey(parameter.Key, SMSProductJsonFileName, parameter.Key);
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
                                        result[result.IndexOf(resultItem)].Gateway = item.Provider;
                                    }
                                }
                                else if (filterEnum == FilterEnum.By_Client)
                                {
                                    result[result.IndexOf(resultItem)].ModifiedDate = System.DateTime.Now;
                                    result[result.IndexOf(resultItem)].Provider = item.Provider;
                                    result[result.IndexOf(resultItem)].Gateway = item.Provider;
                                }
                            }
                        }
                        RedisStringValue redisStringValue = new RedisStringValue();
                        redisStringValue.Key = parameter.Key;
                        redisStringValue.Value = JArray.FromObject(result);
                        ReplaceCacheByKey(redisStringValue, SMSProductJsonFileName);
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
                    TelcoClientCodeList = GetAllStringDataByKey("TelcoClientCodeList", SMSTelcoClientJsonFileName);
                    string[] telcoClientCodeList = TelcoClientCodeList.returnResult.Value.ToObject<string[]>() ?? throw new ArgumentNullException();
                    foreach (var telcoClientCode in telcoClientCodeList)
                    {
                        ReturnResult result = GetAllStringDataByKey(telcoClientCode, SMSProductJsonFileName, telcoClientCode);
                        if (result.status == returnResultEnum.Success)
                        {
                            smsClients = result.returnResult.Value.ToObject<List<SMSPackageLoadModel>>();
                            if (smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).Any())
                            {
                                smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).ToList().ForEach(c => c.ModifiedDate = System.DateTime.Now);
                                smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).ToList().ForEach(c => c.Provider = smsClientModel[0].Provider);
                                smsClients.Where(x => x.TelcoCode == smsClientModel[0].TelcoCode).ToList().ForEach(c => c.Gateway = smsClientModel[0].Provider);
                                ReturnResult returnResult = new ReturnResult();
                                RedisStringValue redisStringValue = new RedisStringValue();
                                redisStringValue.Key = result.returnResult.Key;
                                redisStringValue.Value = JArray.FromObject(smsClients);
                                ReplaceCacheByKey(redisStringValue, SMSProductJsonFileName);
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

        public ReturnResult GetDataConfig(string Key, string filePath = null, string filterKey = null)
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {

                string stringValue = String.Empty;
                if (Storage_depenency.Contains("Cache"))
                {
                    _logger.LogInformation("Getting Data form cache");
                    db = Redis._redis.GetDatabase();
                    stringValue = db.StringGet(Key);
                }
                else if (Storage_depenency.Contains("Json"))
                {
                    _logger.LogInformation("Getting Data form json");
                    stringValue = JsonConvert.SerializeObject(ConfigurationOperations.ReadJson(filePath));
                    if (filterKey != null)
                    {
                        List<RedisStringValue> jsonData = JsonConvert.DeserializeObject<List<RedisStringValue>>(stringValue);
                        stringValue = JsonConvert.SerializeObject(jsonData.Where(x => x.Key.Equals(filterKey)).FirstOrDefault());
                    }
                }

                if (!String.IsNullOrEmpty(stringValue) && !String.IsNullOrWhiteSpace(stringValue) && stringValue != null && stringValue != "null")
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
        public ReturnResult GetAllConfig()
        {
            ReturnResult returnResult = new ReturnResult();
            try
            {
                List<RedisStringValue> listValues = new List<RedisStringValue>();
                string stringValue = String.Empty;
                if (Storage_depenency.Contains("Cache"))
                {
                    _logger.LogInformation("Getting Data form cache");
                    db = Redis._redis.GetDatabase();
                    var server = Redis._redis.GetServer(_configuration.GetConnectionString("RedisCacheAWSConnection"));
                    foreach (var key in server.Keys())
                    {
                        stringValue = db.StringGet(key);

                        if (!String.IsNullOrEmpty(stringValue) && !String.IsNullOrWhiteSpace(stringValue) && stringValue != null && stringValue != "null")
                        {
                            RedisStringValue obj = JsonConvert.DeserializeObject<RedisStringValue>(stringValue);
                            listValues.Add(obj);
                        }
                        else
                        {
                            RedisStringValue obj = new RedisStringValue();
                            obj.Key = key;
                            obj.Value = null;
                            listValues.Add(obj);
                        }
                    }
                }
                else if (Storage_depenency.Contains("Json"))
                {
                    _logger.LogInformation("Getting Data form json");
                    stringValue = JsonConvert.SerializeObject(ConfigurationOperations.ReadJson(SMSProductJsonFileName));
                    string stringValue1 = JsonConvert.SerializeObject(ConfigurationOperations.ReadJson(SMSTelcoClientJsonFileName));
                    string stringValue2 = JsonConvert.SerializeObject(ConfigurationOperations.ReadJson(SMSPhonePrefixJsonFileName));
                    listValues.AddRange(JsonConvert.DeserializeObject<List<RedisStringValue>>(stringValue));
                    listValues.Add(JsonConvert.DeserializeObject<RedisStringValue>(stringValue1));
                    listValues.Add(JsonConvert.DeserializeObject<RedisStringValue>(stringValue2));
                }

                if (listValues.Count > 0)
                {
                    returnResult.status = returnResultEnum.Success;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    returnResult.returnResult = listValues;
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
        public async Task<ReturnResult> TestSendSMS(RedisStringValue parameter, string scheme, string httpparameter, string KBZRefNo, SMS_Send_Request_GATEWAY_Model smsRequestModel)
        {
            string tokenUserName = Common.GetClaimUsername(httpparameter);
            ReturnResult returnResult = new ReturnResult();
            //ReturnResult PhonePrefix = GetAllStringDataByKey("PhonePrefix");
            SMSClientModel smsClientModel = parameter.Value.ToObject<List<SMSClientModel>>().FirstOrDefault();
            smsClientModel.SubscriberNum = smsClientModel.SubscriberNum.Trim();
            try
            {
                if (!String.IsNullOrEmpty(smsClientModel.SubscriberNum) || !String.IsNullOrWhiteSpace(smsClientModel.SubscriberNum))
                {
                    string[] subscriberArray = smsClientModel.SubscriberNum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (subscriberArray.Count() == 1)
                    {
                        if (smsClientModel.SubscriberNum.Length < int.Parse(phoneLength))
                        {
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.InvalidPhoneNumberError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                            _logger.LogError("Phone Error" + JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result));
                            return returnResult;
                        }
                        // Phone Number starts with 09
                        if (smsClientModel.SubscriberNum.StartsWith("0") || smsClientModel.SubscriberNum.StartsWith("9"))
                        {
                            string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 5);
                            if (!subscriberPrefix.StartsWith("959"))
                            {
                                if (subscriberPrefix.StartsWith("9"))
                                {
                                    smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                                    subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                                }
                                else
                                {
                                    subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                                }

                            }
                            _logger.LogInformation("Phone start with: " + subscriberPrefix);
                            if (phonePrefix == null)
                            {
                                returnResult.status = returnResultEnum.Update_Prefix_Cache;
                                returnResult.message = GetEnumDescription(returnResult.status);
                                returnResult.returnResult = smsClientModel;
                                return returnResult;
                            }
                            string dependency = Configuration["dependency"];
                            string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault();
                            if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                            {

                                ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                                if (provider_Data.returnResult != null)
                                {
                                    RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                    List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                    smsClientModel.TelcoCode = phoneOperator;
                                    smsClientModel.Provider = phoneOperator;
                                    SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                    if (smsSendModel != null)
                                    {
                                        returnResult.status = returnResultEnum.Success_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                        smsSendModel.Provider = phoneOperator;
                                        returnResult.returnResult = smsSendModel;
                                        // Implement SMS send here;
                                        smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                        smsSendModel.Message = smsRequestModel.Message;
                                        smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                        smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                        smsSendModel.Msg_type = smsRequestModel.Msg_type;
                                        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                        _logger.LogWarning("Environment" + env);
                                        if (env.Equals("Development"))
                                        {
                                            _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("UAT"))
                                        {
                                            _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("PRODUCTION"))
                                        {
                                            _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                        }
                                        else
                                        {
                                            _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                            returnResult.message = GetEnumDescription(returnResult.status);
                                            HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                            httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                            BaseRespError responseModel = ErrorCode.WrongEnv;
                                            httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                            returnResult.returnResult = httpResponseErrorMessage;
                                            return returnResult;
                                        }
                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.OK;
                                        smsSendModel.Gateway = smsRequestModel.Gateway;
                                        _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                        httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "TestSendSMS", smsSendModel);
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                    else
                                    {
                                        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                        smsClientModel.TelcoCode = phoneOperator;

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                        BaseRespError responseModel = ErrorCode.ClientCodeError;
                                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                    smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                    BaseRespError responseModel = ErrorCode.ClientCodeError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                // Get Cache with Client Code
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                                smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                BaseRespError responseModel = ErrorCode.CacheError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                // Implement SMS couldn't send here;
                            }
                        }
                        // Phone Number starts with 959
                        else if (smsClientModel.SubscriberNum.StartsWith("959"))
                        {
                            string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 5);
                            _logger.LogInformation("Phone start with 9: " + subscriberPrefix);
                            string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault();
                            if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                            {
                                ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName);
                                if (provider_Data.returnResult != null)
                                {
                                    RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                    List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                    smsClientModel.TelcoCode = phoneOperator;
                                    SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                    if (smsSendModel != null)
                                    {
                                        returnResult.status = returnResultEnum.Success_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                                        smsSendModel.Provider = phoneOperator;
                                        returnResult.returnResult = smsSendModel;
                                        // Implement SMS send here;
                                        smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                        smsSendModel.Message = smsRequestModel.Message;
                                        smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                        smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                        smsSendModel.Msg_type = smsRequestModel.Msg_type;

                                        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                        _logger.LogWarning("Environment" + env);
                                        if (env.Equals("Development"))
                                        {
                                            _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("UAT"))
                                        {
                                            _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                        }
                                        else if (env.Contains("PRODUCTION"))
                                        {
                                            _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                            smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                        }
                                        else
                                        {
                                            _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                            returnResult.message = GetEnumDescription(returnResult.status);
                                            HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                            httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                            BaseRespError responseModel = ErrorCode.WrongEnv;
                                            httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                            returnResult.returnResult = httpResponseErrorMessage;
                                            return returnResult;
                                        }

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                                        smsSendModel.Gateway = smsRequestModel.Gateway;
                                        _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                        httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "TestSendSMS", smsSendModel);
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                    else
                                    {
                                        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                        returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                        smsClientModel.TelcoCode = phoneOperator;

                                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                        BaseRespError responseModel = ErrorCode.ClientCodeError;
                                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessage;
                                    }
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                    smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                    BaseRespError responseModel = ErrorCode.ProviderError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                // Get Cache with Client Code
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                                smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                BaseRespError responseModel = ErrorCode.CacheError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                // Implement SMS couldn't send here;
                            }
                        }
                        #region In case of subscriber starting with only "9"
                        //else if (smsClientModel.SubscriberNum.StartsWith("9"))
                        //{
                        //    //smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                        //    string subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 3);
                        //    if (!subscriberPrefix.Contains("959")) {
                        //        smsClientModel.SubscriberNum = "0" + smsClientModel.SubscriberNum;
                        //        subscriberPrefix = smsClientModel.SubscriberNum.Substring(0, 4);
                        //    }
                        //    _logger.LogInformation("Phone start with 9: " + subscriberPrefix);
                        //    if (phonePrefix == null)
                        //    {
                        //        returnResult.status = returnResultEnum.Update_Prefix_Cache;
                        //        returnResult.message = GetEnumDescription(returnResult.status);
                        //        returnResult.returnResult = smsClientModel;
                        //        return returnResult;
                        //    }
                        //    string dependency = Configuration["dependency"];
                        //    string phoneOperator = phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault(); ;
                        //    if (!String.IsNullOrEmpty(phoneOperator) || !String.IsNullOrWhiteSpace(phoneOperator))
                        //    {

                        //        ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                        //        if (provider_Data.returnResult != null)
                        //        {
                        //            RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                        //            List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>() ?? throw new ArgumentNullException();
                        //            smsClientModel.TelcoCode = phoneOperator;
                        //            smsClientModel.Provider = phoneOperator;
                        //            SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode)).FirstOrDefault();
                        //            if (smsSendModel != null)
                        //            {
                        //                returnResult.status = returnResultEnum.Success_SMS_Sent;
                        //                returnResult.message = GetEnumDescription(returnResult.status) + " Provide: " + phoneOperator;
                        //                smsSendModel.Provider = phoneOperator;
                        //                returnResult.returnResult = smsSendModel;
                        //                // Implement SMS send here;
                        //                smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                        //                smsSendModel.Message = smsRequestModel.Message;
                        //                smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                        //                smsSendModel.ClientCode = smsRequestModel.ClientCode;
                        //                smsSendModel.Msg_type = smsRequestModel.Msg_type;
                        //                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        //                if (env.Equals("Development"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                        //                }
                        //                else if (env.Contains("UAT"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                        //                }
                        //                else if (env.Contains("Prod"))
                        //                {
                        //                    smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                        //                }
                        //                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;

                        //                httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "WrapperSendSMS", smsSendModel);
                        //                returnResult.returnResult = httpResponseMessage;
                        //            }
                        //            else
                        //            {
                        //                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //                returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                        //                smsClientModel.TelcoCode = phoneOperator;

                        //                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //                BaseRespError responseModel = ErrorCode.ClientCodeError;
                        //                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //                returnResult.returnResult = httpResponseMessage;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //            returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                        //            smsClientModel.TelcoCode = phoneOperator;

                        //            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //            BaseRespError responseModel = ErrorCode.ProviderError;
                        //            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //            returnResult.returnResult = httpResponseMessage;
                        //        }
                        //        // Get Cache with Client Code
                        //    }
                        //    else
                        //    {
                        //        returnResult.status = returnResultEnum.Fail_SMS_Sent;
                        //        returnResult.message = GetEnumDescription(returnResult.status) + " Phone number prefix does not exist in cache. Please update phone number prefix cache";
                        //        smsClientModel.TelcoCode = phoneOperator;

                        //        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        //        httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        //        BaseRespError responseModel = ErrorCode.CacheError;
                        //        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                        //        returnResult.returnResult = httpResponseMessage;
                        //        // Implement SMS couldn't send here;
                        //    }
                        //}
                        #endregion
                        else
                        {
                            returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                            returnResult.message = GetEnumDescription(returnResult.status);
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.InvalidSubscriberError;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                            //Invalid Subscriber Number
                        }
                    }
                    else
                    {
                        List<string> phoneOperator = new List<string>();
                        foreach (string item in subscriberArray)
                        {
                            if (item.Length < int.Parse(phoneLength))
                            {
                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                BaseRespError responseModel = ErrorCode.InvalidPhoneNumberError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                                _logger.LogError("Phone Error" + JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result));
                                return returnResult;
                            }
                            string subscriberPrefix = item.Substring(0, 3);
                            if (item.StartsWith("0") || item.StartsWith("9"))
                            {
                                if (!subscriberPrefix.StartsWith("959"))
                                {
                                    if (subscriberPrefix.StartsWith("9"))
                                    {
                                        string sub = "0" + item;
                                        subscriberPrefix = sub.Substring(0, 4);
                                    }
                                }
                                else
                                {
                                    subscriberPrefix = item.Substring(0, 5);
                                }
                            }
                            phoneOperator.Add(phonePrefix.Where(x => x.Prefixes.Contains(subscriberPrefix)).Select(x => x.Operator).FirstOrDefault());
                        }
                        if (phoneOperator.Count > 0 && !phoneOperator.Distinct().Skip(1).Any())
                        {

                            ReturnResult provider_Data = GetAllStringDataByKey(smsClientModel.ClientCode, SMSProductJsonFileName, smsClientModel.ClientCode);
                            if (provider_Data.returnResult != null)
                            {
                                RedisStringValue? returnedData = provider_Data.returnResult as RedisStringValue;
                                List<SMSPackageLoadModel> result = returnedData.Value.ToObject<List<SMSPackageLoadModel>>();
                                smsClientModel.TelcoCode = phoneOperator[0];
                                smsClientModel.Provider = phoneOperator[0];
                                SMSPackageLoadModel smsSendModel = result.Where(x => x.TelcoCode.Equals(smsClientModel.TelcoCode) && x.ClientCode.Equals(smsClientModel.ClientCode) && x.TokenUserName.Equals(tokenUserName)).FirstOrDefault();
                                if (smsSendModel != null)
                                {
                                    returnResult.status = returnResultEnum.Success_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status);
                                    //smsSendModel.Provider = phoneOperator;
                                    returnResult.returnResult = smsSendModel;
                                    // Implement SMS send here;
                                    smsSendModel.SubscriberNum = smsRequestModel.SubscriberNum;
                                    smsSendModel.Message = smsRequestModel.Message;
                                    smsSendModel.TrxnRefNum = smsRequestModel.TrxnRefNum;
                                    smsSendModel.ClientCode = smsRequestModel.ClientCode;
                                    smsSendModel.Msg_type = smsRequestModel.Msg_type;
                                    smsSendModel.Provider = phoneOperator[0];
                                    if (!smsSendModel.Gateway.Equals("MGateBulk"))
                                    {
                                        HttpResponseMessage httpResponseMessageforGateway = new HttpResponseMessage();
                                        httpResponseMessageforGateway.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                        BaseRespError responseModel = ErrorCode.InvalidGateway;
                                        httpResponseMessageforGateway.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseMessageforGateway;
                                        _logger.LogError("Gate Way Error" + JsonConvert.DeserializeObject(httpResponseMessageforGateway.Content.ReadAsStringAsync().Result));
                                        return returnResult;
                                    }
                                    string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                                    _logger.LogWarning("Environment" + env);
                                    if (env.Equals("Development"))
                                    {
                                        _logger.LogInformation("Dev Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                    }
                                    else if (env.Contains("UAT"))
                                    {
                                        _logger.LogInformation("UAT Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.UAT_SenderID;
                                    }
                                    else if (env.Contains("PRODUCTION"))
                                    {
                                        _logger.LogInformation("Production Sender ID" + smsSendModel.Pro_SenderID);
                                        smsSendModel.SENDERNAME = smsSendModel.Pro_SenderID;
                                    }
                                    else
                                    {
                                        _logger.LogError("Sender Name empty:" + smsSendModel.SENDERNAME);
                                        returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                                        returnResult.message = GetEnumDescription(returnResult.status);
                                        HttpResponseMessage httpResponseErrorMessage = new HttpResponseMessage();
                                        httpResponseErrorMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                        BaseRespError responseModel = ErrorCode.WrongEnv;
                                        httpResponseErrorMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                        returnResult.returnResult = httpResponseErrorMessage;
                                        return returnResult;
                                    }
                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                                    smsSendModel.Gateway = smsRequestModel.Gateway;
                                    _logger.LogInformation("SMS Payload: " + JsonConvert.SerializeObject(smsSendModel));
                                    httpResponseMessage = await SMSSendToBiz(scheme, httpparameter, KBZRefNo, "TestSendSMS", smsSendModel);
                                    returnResult.returnResult = httpResponseMessage;
                                }
                                else
                                {
                                    returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                    returnResult.message = GetEnumDescription(returnResult.status) + " Subscriber does not exist in cache";
                                    //smsClientModel.TelcoCode = phoneOperator;

                                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                                    BaseRespError responseModel = ErrorCode.ClientCodeError;
                                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                    returnResult.returnResult = httpResponseMessage;
                                }
                            }
                            else
                            {
                                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                                returnResult.message = GetEnumDescription(returnResult.status) + " Provider Empty";
                                //smsClientModel.TelcoCode = phoneOperator;

                                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                                BaseRespError responseModel = ErrorCode.ProviderError;
                                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                                returnResult.returnResult = httpResponseMessage;
                            }
                        }
                        else
                        {
                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            BaseRespError responseModel = ErrorCode.ProvierAreDifferent;
                            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                            returnResult.returnResult = httpResponseMessage;
                        }

                    }
                }
                else
                {
                    returnResult.status = returnResultEnum.Fail_SMS_Invalid_SubscriberNum;
                    returnResult.message = GetEnumDescription(returnResult.status);
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                    httpResponseMessage.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    BaseRespError responseModel = ErrorCode.PhoneNumberNullError;
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                    returnResult.returnResult = httpResponseMessage;
                    //Phone Number Null
                }
            }
            catch (Exception ex)
            {
                returnResult.status = returnResultEnum.Fail_SMS_Sent;
                returnResult.message = GetEnumDescription(returnResult.status);
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                BaseRespError responseModel = ErrorCode.UnknownException;
                responseModel.Details.Add(new BaseRespErrorDetail { ErrorCode = "400", ErrorDescription = ex.ToString() });
                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { KBZRefNo = KBZRefNo, Error = responseModel }), Encoding.UTF8, "application/json");
                returnResult.returnResult = httpResponseMessage;
                _logger.LogError("Message: " + ex.ToString());
                _logger.LogError("Innser Message: " + ex.InnerException.ToString());
            }
            return returnResult;
        }
    }
}
