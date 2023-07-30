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
using System.Net.Http.Headers;
using static apigee.sms.intf.Models.ReturnResultEnum;

namespace apigee.sms.intf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(CustomAuthorizeFilter))]
    [ScopedService]
    public class RedisCacheController : BaseController
    {
        private readonly IStringValueRepo _repository;
        private ILogger<RedisCacheController> logger;
        private static string SMSProductJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSProductsCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSPhonePrefixJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSPhonePrefixCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSTelcoClientJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSTelcoClientCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        public RedisCacheController(IStringValueRepo repository, ILogger<RedisCacheController> nlog)
        {
            _repository = repository;
            logger = nlog;
        }
        #region Check cache with key
        [HttpPost("GetDataByClientCode/", Name = "GetDataByClientCode")]
        public ActionResult<dynamic> GetDataByClientCode([FromBody] JArray param)
        {
            dynamic? result = null;
            List<ReturnResult> dataList = new List<ReturnResult>();
            dynamic paramObject = param;
            try
            {
                //JObject paramObject = param;
                if (paramObject != null)
                {
                    ReturnResult data = new ReturnResult();
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    foreach (var item in paramObject)
                    {
                        if (item.KEY != null && item.VALUE != null)
                        {
                            RedisStringValue redisHashValue = new RedisStringValue();
                            redisHashValue.Key = item.KEY;
                            redisHashValue.Value = item.VALUE;
                            data = _repository.GetAllStringDataByKey(redisHashValue.Key.ToString());

                        }
                        else
                        {
                            data.status = returnResultEnum.Fail_VALUE_KEY_NULL;
                            data.message = GetEnumDescription(data.status);
                        }
                        dataList.Add(data);
                    }
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
            }
            result = new { dataList };
            return result;
        }
        #endregion

        #region SMS Create Config By Key
        [HttpPost("SMSCreateConfigIntfByClientCode/", Name = "SMSCreateConfigIntfByClientCode")]
        public async Task<dynamic> SMSCreateConfigIntfByClientCode([FromBody] FilterByClientCode param)
        {
            dynamic? result = null;
            FilterByClientCode filter = param;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            List<ReturnResult> dataReturnList = new List<ReturnResult>();
            List<ResponseModel> errorReturnList = new List<ResponseModel>();

            ReturnResult dataForPhonePrefixList = new ReturnResult();
            dataForPhonePrefixList.status = returnResultEnum.Pending;
            dataForPhonePrefixList.message = GetEnumDescription(dataForPhonePrefixList.status);

            ReturnResult dataForTelcoClientCodeList = new ReturnResult();
            dataForTelcoClientCodeList.status = returnResultEnum.Pending;
            dataForTelcoClientCodeList.message = GetEnumDescription(dataForTelcoClientCodeList.status);
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var scheme = authHeader.Scheme;
                var parameter = authHeader.Parameter;
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                HttpResponseMessage httpResponseMessageForPhonePrefix = new HttpResponseMessage();
                httpResponseMessageForPhonePrefix.StatusCode = System.Net.HttpStatusCode.Processing;
                if (filter != null)
                {
                    if (filter.UpdateFilter != null && filter.UpdateFilter == 0) // Update All Cache
                    {
                        httpResponseMessage = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSCreateConfigBiz");
                        httpResponseMessageForPhonePrefix = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSGetPhonePrefixesBiz");
                    }
                    else if (filter.UpdateFilter != null && filter.UpdateFilter == 1) // Phone Prefix Update Cache
                    {
                        httpResponseMessageForPhonePrefix = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSGetPhonePrefixesBiz");
                    }
                    else if (filter.UpdateFilter != null && filter.UpdateFilter == 2) // Cache data Update
                    {
                        httpResponseMessage = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSCreateConfigBiz");
                    }
                    else if (filter.UpdateFilter > 2 || filter.UpdateFilter < 0)
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.FilterOutOfRange;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                    else if (filter.UpdateFilter == null)
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.FilterEmpty;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                    else
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.UnknownException;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                }
                else
                {
                    ResponseModel responseModel = new ResponseModel();
                    responseModel.Error = ErrorCode.FilterEmpty;
                    return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                }

                if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ResponseModel responseModelForPhonePrefix = JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    if (responseModelForPhonePrefix != null)
                    {
                        if (responseModelForPhonePrefix.Data != null)
                        {
                            RedisStringValue phonePrefixRedisStringValue = new RedisStringValue();
                            //var test = phonePrefixRedisStringValue.Value.ToObject<List<PhonePrefixes>>();
                            List<PhonePrefix> phonePrefixesList = JArray.FromObject(responseModelForPhonePrefix.Data).ToObject<List<PhonePrefixes>>().Select(x => new PhonePrefix { Operator = x.KEY, Prefixes = x.VALUE }).ToList();
                            phonePrefixRedisStringValue.Key = "PhonePrefix";
                            phonePrefixRedisStringValue.Value = JArray.FromObject(phonePrefixesList);
                            var phoneprefixforJson = new { Key = "PhonePrefix", Value = phonePrefixesList };
                            #region Create Data With Filter
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(phoneprefixforJson, SMSPhonePrefixJsonFileName, logger);
                                // 

                                dataForPhonePrefixList = await Task.FromResult(_repository.CreateDataWithString_Task(phonePrefixRedisStringValue)).Result;
                                if (dataForPhonePrefixList.status == returnResultEnum.Success)
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Json & Cache  created";
                                }
                                else
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Json & Cache  creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModelForPhonePrefix);
                                }
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(phoneprefixforJson, SMSPhonePrefixJsonFileName, logger);
                                // 

                                dataForPhonePrefixList.status = returnResultEnum.Success;
                                dataForPhonePrefixList.message = "\'Phone Prefix\' Json created";
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                dataForPhonePrefixList = await Task.FromResult(_repository.CreateDataWithString_Task(phonePrefixRedisStringValue)).Result;
                                if (dataForPhonePrefixList.status == returnResultEnum.Success)
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Cache  created";
                                }
                                else
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Cache  creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModelForPhonePrefix);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Error
                            //responseModelForPhonePrefix.Error = ErrorCode.NoRecordFound;
                            //responseModelForPhonePrefix.Error.Details.Add(ErrorCode.PhonePrefixEmpty);
                            errorReturnList.Add(responseModelForPhonePrefix);
                        }
                    }
                    else
                    {
                        // Error
                        //responseModelForPhonePrefix.Error = ErrorCode.NoRecordFound;
                        //responseModelForPhonePrefix.Error.Details.Add(ErrorCode.PhonePrefixEmpty);
                        errorReturnList.Add(responseModelForPhonePrefix);
                    }
                }
                else
                {
                    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                }
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ResponseModel responseModel = JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result);
                    if (responseModel != null)
                    {
                        if (responseModel.Data != null)
                        {
                            var Products = new { Products = responseModel.Data };
                            //ConfigurationOperations.SMSJsonUpdateDynamic(Products, SMSProductJsonFileName);
                            RedisStringValue redisStringValue = new RedisStringValue();
                            redisStringValue.Value = JArray.FromObject(responseModel.Data);
                            List<SMSPackageLoadModel> smsClientModelList = redisStringValue.Value.ToObject<List<SMSPackageLoadModel>>();
                            IEnumerable<RedisStringValue> groupedSMSModel = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => new RedisStringValue { Key = y.Key, Value = JArray.FromObject(y.ToList()) });
                            RedisStringValue groupedTelcoClientCodeList = new RedisStringValue
                            {
                                Key = "TelcoClientCodeList",
                                Value_String = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => y.Where(z1 => z1.ClientCode != null || !String.IsNullOrEmpty(z1.ClientCode)).Select(z => z.ClientCode).ToString()).ToArray()
                            };


                            var telcoclientforjson = new { Key = "TelcoClientCodeList", Value = groupedTelcoClientCodeList.Value_String };
                            #region Create Data With Filter
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(telcoclientforjson, SMSTelcoClientJsonFileName, logger);
                                //

                                dataForTelcoClientCodeList = await Task.FromResult(_repository.CreateDataWithString_Task(groupedTelcoClientCodeList)).Result;
                                if (dataForTelcoClientCodeList.status == returnResultEnum.Success)
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json & Cache created";
                                }
                                else
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json & Cache creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModel);
                                }
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(telcoclientforjson, SMSTelcoClientJsonFileName, logger);
                                //

                                dataForTelcoClientCodeList.status = returnResultEnum.Success;
                                dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json created";
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                dataForTelcoClientCodeList = await Task.FromResult(_repository.CreateDataWithString_Task(groupedTelcoClientCodeList)).Result;
                                if (dataForTelcoClientCodeList.status == returnResultEnum.Success)
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Cache created";
                                }
                                else
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Cache creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModel);
                                }
                            }
                            #endregion
                            JArray productsForJson = new JArray();
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                foreach (var item in groupedSMSModel.Where(x => x.Key.Equals(filter.CLIENTCODE)).ToList())
                                {
                                    ReturnResult data = new ReturnResult();
                                    data.status = returnResultEnum.Pending;
                                    data.message = GetEnumDescription(data.status);
                                    data = _repository.CreateDataWithString(item);

                                    if (data.status != returnResultEnum.Success)
                                    {
                                        errorReturnList.Add(new ResponseModel { Data = new { data, item }, KBZRefNo = KBZRefNo });
                                        logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + JsonConvert.SerializeObject(new { data, item }));
                                    }
                                    dataReturnList.Add(data);
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }

                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(productsForJson, SMSProductJsonFileName, logger);
                                //
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                foreach (var item in groupedSMSModel)
                                {
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }
                                ReturnResult data = new ReturnResult();
                                data.status = returnResultEnum.Success;
                                data.message = "Product Json Created";

                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(productsForJson, SMSProductJsonFileName, logger);
                                dataReturnList.Add(data);
                                //
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                foreach (var item in groupedSMSModel.Where(x => x.Key.Equals(filter.CLIENTCODE)).ToList())
                                {
                                    ReturnResult data = new ReturnResult();
                                    data.status = returnResultEnum.Pending;
                                    data.message = GetEnumDescription(data.status);
                                    data = _repository.CreateDataWithString(item);

                                    if (data.status != returnResultEnum.Success)
                                    {
                                        errorReturnList.Add(new ResponseModel { Data = new { data, item }, KBZRefNo = KBZRefNo });
                                        logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + JsonConvert.SerializeObject(new { data, item }));
                                    }
                                    dataReturnList.Add(data);
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }
                            }

                        }
                        else
                        {
                            //responseModel.Error = ErrorCode.NoRecordFound;
                            //responseModel.Error.Details.Add(ErrorCode.ProductEmpty);
                            errorReturnList.Add(responseModel);
                        }
                    }
                    else
                    {
                        // Error
                        //responseModel.Error = ErrorCode.NoRecordFound;
                        //responseModel.Error.Details.Add(ErrorCode.ProductEmpty);
                        errorReturnList.Add(responseModel);
                    }
                }
                else
                {
                    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                }
                //if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessage.StatusCode) };
                //    //return BadRequest(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.BadGateway)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return BadRequest(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessage.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.NotFound)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //else
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
            }
            catch (Exception ex)
            {
                ResponseModel data = new ResponseModel();
                data.Error = ErrorCode.CachingError;
                data.Error.Message = ex.ToString();
                logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: " + ex.ToString());
                errorReturnList.Add(data);
            }
            result = new { KBZRefNo = KBZRefNo, Data = new { dataForPhonePrefixList, dataForTelcoClientCodeList, dataReturnList }, Error = new { errorReturnList } };
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: ");
            if (errorReturnList.Count == 0)
            {
                //return Ok(result);
                result = new { KBZRefNo = KBZRefNo, Data = new { Status = 200, Message = "Cache Refreshed Successfully", dataForPhonePrefixList, dataForTelcoClientCodeList, dataReturnList }, Error = new { errorReturnList } };
                return new ObjectResult(result)
                { StatusCode = StatusCodes.Status200OK };
            }
            else
            {
                return new ObjectResult(result)
                { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }
        #endregion

        #region SMS Create Config
        [HttpPost("SMSCreateConfigIntf/", Name = "SMSCreateConfigIntf")]
        public async Task<dynamic> SMSCreateConfigIntf([FromBody] filter param)
        {
            dynamic? result = null;
            filter filter = param;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            List<ReturnResult> dataReturnList = new List<ReturnResult>();
            List<ResponseModel> errorReturnList = new List<ResponseModel>();

            ReturnResult dataForPhonePrefixList = new ReturnResult();
            dataForPhonePrefixList.status = returnResultEnum.Pending;
            dataForPhonePrefixList.message = GetEnumDescription(dataForPhonePrefixList.status);

            ReturnResult dataForTelcoClientCodeList = new ReturnResult();
            dataForTelcoClientCodeList.status = returnResultEnum.Pending;
            dataForTelcoClientCodeList.message = GetEnumDescription(dataForTelcoClientCodeList.status);
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var scheme = authHeader.Scheme;
                var parameter = authHeader.Parameter;
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage.StatusCode = System.Net.HttpStatusCode.Processing;
                HttpResponseMessage httpResponseMessageForPhonePrefix = new HttpResponseMessage();
                httpResponseMessageForPhonePrefix.StatusCode = System.Net.HttpStatusCode.Processing;
                if (filter != null)
                {
                    if (filter.UpdateFilter != null && filter.UpdateFilter == 0) // Update All Cache
                    {
                        httpResponseMessage = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSCreateConfigBiz");
                        httpResponseMessageForPhonePrefix = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSGetPhonePrefixesBiz");
                    }
                    else if (filter.UpdateFilter != null && filter.UpdateFilter == 1) // Phone Prefix Update Cache
                    {
                        httpResponseMessageForPhonePrefix = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSGetPhonePrefixesBiz");
                    }
                    else if (filter.UpdateFilter != null && filter.UpdateFilter == 2) // Cache data Update
                    {
                        httpResponseMessage = await _repository.SMSConfigCallback(scheme, parameter, KBZRefNo, "SMSCreateConfigBiz");
                    }
                    else if (filter.UpdateFilter > 2 || filter.UpdateFilter < 0)
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.FilterOutOfRange;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                    else if (filter.UpdateFilter == null)
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.FilterEmpty;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                    else
                    {
                        ResponseModel responseModel = new ResponseModel();
                        responseModel.Error = ErrorCode.UnknownException;
                        return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                    }
                }
                else
                {
                    ResponseModel responseModel = new ResponseModel();
                    responseModel.Error = ErrorCode.FilterEmpty;
                    return StatusCode(StatusCodes.Status400BadRequest, new { KBZRefNo = KBZRefNo, Data = new { }, Error = responseModel.Error });
                }

                if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ResponseModel responseModelForPhonePrefix = JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    if (responseModelForPhonePrefix != null)
                    {
                        if (responseModelForPhonePrefix.Data != null)
                        {
                            RedisStringValue phonePrefixRedisStringValue = new RedisStringValue();
                            //var test = phonePrefixRedisStringValue.Value.ToObject<List<PhonePrefixes>>();
                            List<PhonePrefix> phonePrefixesList = JArray.FromObject(responseModelForPhonePrefix.Data).ToObject<List<PhonePrefixes>>().Select(x => new PhonePrefix { Operator = x.KEY, Prefixes = x.VALUE }).ToList();
                            phonePrefixRedisStringValue.Key = "PhonePrefix";
                            phonePrefixRedisStringValue.Value = JArray.FromObject(phonePrefixesList);
                            var phoneprefixforJson = new { Key = "PhonePrefix", Value = phonePrefixesList };
                            #region Create Data With Filter
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(phoneprefixforJson, SMSPhonePrefixJsonFileName, logger);
                                // 

                                dataForPhonePrefixList = await Task.FromResult(_repository.CreateDataWithString_Task(phonePrefixRedisStringValue)).Result;
                                if (dataForPhonePrefixList.status == returnResultEnum.Success)
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Json & Cache  created";
                                }
                                else
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Json & Cache  creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModelForPhonePrefix);
                                }
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(phoneprefixforJson, SMSPhonePrefixJsonFileName, logger);
                                // 

                                dataForPhonePrefixList.status = returnResultEnum.Success;
                                dataForPhonePrefixList.message = "\'Phone Prefix\' Json created";
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                dataForPhonePrefixList = await Task.FromResult(_repository.CreateDataWithString_Task(phonePrefixRedisStringValue)).Result;
                                if (dataForPhonePrefixList.status == returnResultEnum.Success)
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Cache  created";
                                }
                                else
                                {
                                    dataForPhonePrefixList.message = "\'Phone Prefix\' Cache  creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModelForPhonePrefix);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Error
                            //responseModelForPhonePrefix.Error = ErrorCode.NoRecordFound;
                            //responseModelForPhonePrefix.Error.Details.Add(ErrorCode.PhonePrefixEmpty);
                            errorReturnList.Add(responseModelForPhonePrefix);
                        }
                    }
                    else
                    {
                        // Error
                        //responseModelForPhonePrefix.Error = ErrorCode.NoRecordFound;
                        //responseModelForPhonePrefix.Error.Details.Add(ErrorCode.PhonePrefixEmpty);
                        errorReturnList.Add(responseModelForPhonePrefix);
                    }
                }
                else
                {
                    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                }
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ResponseModel responseModel = JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result);
                    if (responseModel != null)
                    {
                        if (responseModel.Data != null)
                        {
                            var Products = new { Products = responseModel.Data };
                            //ConfigurationOperations.SMSJsonUpdateDynamic(Products, SMSProductJsonFileName);
                            RedisStringValue redisStringValue = new RedisStringValue();
                            redisStringValue.Value = JArray.FromObject(responseModel.Data);
                            List<SMSPackageLoadModel> smsClientModelList = redisStringValue.Value.ToObject<List<SMSPackageLoadModel>>();
                            IEnumerable<RedisStringValue> groupedSMSModel = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => new RedisStringValue { Key = y.Key, Value = JArray.FromObject(y.ToList()) });
                            RedisStringValue groupedTelcoClientCodeList = new RedisStringValue
                            {
                                Key = "TelcoClientCodeList",
                                Value_String = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => y.Where(z1 => z1.ClientCode != null || !String.IsNullOrEmpty(z1.ClientCode)).Select(z => z.ClientCode).ToString()).ToArray()
                            };


                            var telcoclientforjson = new { Key = "TelcoClientCodeList", Value = groupedTelcoClientCodeList.Value_String };
                            #region Create Data With Filter
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                // Json File Udate                                
                                ConfigurationOperations.SMSJsonUpdateDynamic(telcoclientforjson, SMSTelcoClientJsonFileName, logger);
                                //

                                dataForTelcoClientCodeList = await Task.FromResult(_repository.CreateDataWithString_Task(groupedTelcoClientCodeList)).Result;
                                if (dataForTelcoClientCodeList.status == returnResultEnum.Success)
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json & Cache created";
                                }
                                else
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json & Cache creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModel);
                                }
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(telcoclientforjson, SMSTelcoClientJsonFileName, logger);
                                //

                                dataForTelcoClientCodeList.status = returnResultEnum.Success;
                                dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Json created";
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                dataForTelcoClientCodeList = await Task.FromResult(_repository.CreateDataWithString_Task(groupedTelcoClientCodeList)).Result;
                                if (dataForTelcoClientCodeList.status == returnResultEnum.Success)
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Cache created";
                                }
                                else
                                {
                                    dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' Cache creation failed. Please re-run the cache update again.";
                                    errorReturnList.Add(responseModel);
                                }
                            }
                            #endregion
                            JArray productsForJson = new JArray();
                            if (filter.CreateDataWith.Contains("All") || filter.CreateDataWith.Contains("Both"))
                            {
                                foreach (var item in groupedSMSModel)
                                {
                                    ReturnResult data = new ReturnResult();
                                    data.status = returnResultEnum.Pending;
                                    data.message = GetEnumDescription(data.status);
                                    data = _repository.CreateDataWithString(item);

                                    if (data.status != returnResultEnum.Success)
                                    {
                                        errorReturnList.Add(new ResponseModel { Data = new { data, item }, KBZRefNo = KBZRefNo });
                                        logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + JsonConvert.SerializeObject(new { data, item }));
                                    }
                                    dataReturnList.Add(data);
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }

                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(productsForJson, SMSProductJsonFileName, logger);
                                //
                            }
                            else if (filter.CreateDataWith.Contains("Json"))
                            {
                                foreach (var item in groupedSMSModel)
                                {
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }
                                ReturnResult data = new ReturnResult();
                                data.status = returnResultEnum.Success;
                                data.message = "Product Json Created";

                                // Json File Udate
                                ConfigurationOperations.SMSJsonUpdateDynamic(productsForJson, SMSProductJsonFileName, logger);
                                dataReturnList.Add(data);
                                //
                            }
                            else if (filter.CreateDataWith.Contains("Cache"))
                            {
                                foreach (var item in groupedSMSModel)
                                {
                                    ReturnResult data = new ReturnResult();
                                    data.status = returnResultEnum.Pending;
                                    data.message = GetEnumDescription(data.status);
                                    data = _repository.CreateDataWithString(item);

                                    if (data.status != returnResultEnum.Success)
                                    {
                                        errorReturnList.Add(new ResponseModel { Data = new { data, item }, KBZRefNo = KBZRefNo });
                                        logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + JsonConvert.SerializeObject(new { data, item }));
                                    }
                                    dataReturnList.Add(data);
                                    JObject productForObject = new JObject();
                                    productForObject["Key"] = item.Key;
                                    productForObject["Value"] = item.Value;

                                    productsForJson.Add(productForObject);
                                }
                            }

                        }
                        else
                        {
                            //responseModel.Error = ErrorCode.NoRecordFound;
                            //responseModel.Error.Details.Add(ErrorCode.ProductEmpty);
                            errorReturnList.Add(responseModel);
                        }
                    }
                    else
                    {
                        // Error
                        //responseModel.Error = ErrorCode.NoRecordFound;
                        //responseModel.Error.Details.Add(ErrorCode.ProductEmpty);
                        errorReturnList.Add(responseModel);
                    }
                }
                else
                {
                    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                }
                //if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessage.StatusCode) };
                //    //return BadRequest(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.BadGateway)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return BadRequest(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessage.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //if (httpResponseMessageForPhonePrefix.StatusCode == System.Net.HttpStatusCode.NotFound)
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
                //else
                //{
                //    logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result);
                //    return new ObjectResult(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result))
                //    { StatusCode = ((int)httpResponseMessageForPhonePrefix.StatusCode) };
                //    //return StatusCode(StatusCodes.Status500InternalServerError, JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessageForPhonePrefix.Content.ReadAsStringAsync().Result));
                //}
            }
            catch (Exception ex)
            {
                ResponseModel data = new ResponseModel();
                data.Error = ErrorCode.CachingError;
                data.Error.Message = ex.ToString();
                logger.LogError("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: " + ex.ToString());
                errorReturnList.Add(data);
            }
            result = new { KBZRefNo = KBZRefNo, Data = new { dataForPhonePrefixList, dataForTelcoClientCodeList, dataReturnList }, Error = new { errorReturnList } };
            logger.LogInformation("API out. KBZ Reference Number:" + KBZRefNo + " .Returned Data: " + (string)JsonConvert.SerializeObject(result) + " .Error: ");
            if (errorReturnList.Count == 0)
            {
                //return Ok(result);
                result = new { KBZRefNo = KBZRefNo, Data = new { Status = 200, Message = "Cache Refreshed Successfully", dataForPhonePrefixList, dataForTelcoClientCodeList, dataReturnList }, Error = new { errorReturnList } };
                return new ObjectResult(result)
                { StatusCode = StatusCodes.Status200OK };
            }
            else
            {
                return new ObjectResult(result)
                { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }
        #endregion

        #region Modify Dependency in app setting
        [HttpPost("ModifyDependencyConfig/", Name = "ModifyDependencyConfig")]
        public async Task<dynamic> ModifyDependencyConfig([FromBody] JObject param)
        {
            dynamic? result = null;
            logger.LogInformation("Modify Dependency API in");
            ReturnResult data = new ReturnResult();
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            dynamic paramObject = param;
            string modifiedDependency = paramObject.modifiedDependency;
            try
            {
                //JObject paramObject = param;
                if (!String.IsNullOrEmpty(modifiedDependency))
                {
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    data = _repository.ModifyDependencyConfigRepo(modifiedDependency);
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
            result = new { data };
            logger.LogInformation("Modify Dependency API in");
            return StatusCode(StatusCodes.Status200OK, new { KBZRefNo = KBZRefNo, Data = data, Error = new { } });
            //return Ok(result);
        }
        #endregion

        #region Delete Cache
        [HttpPost("DeleteCache/", Name = "DeleteCache")]
        public ActionResult<dynamic> DeleteCache([FromBody] JObject param)
        {
            dynamic? result = null;
            dynamic paramData = param;
            logger.LogInformation("Delete Cache API in");
            List<ReturnResult> dataList = new List<ReturnResult>();
            ReturnResult data = new ReturnResult();
            try
            {
                //JObject paramObject = param;
                if (paramData != null)
                {
                    data.status = returnResultEnum.Pending;
                    data.message = GetEnumDescription(data.status);
                    RedisStringValue redisHashValue = new RedisStringValue();
                    redisHashValue.Key = paramData.Key;
                    data = _repository.DeleteCache(redisHashValue);
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
            }
            result = new { data };
            logger.LogInformation("Delete Cache API in");
            return result;
        }
        #endregion

        #region Reference Code (Can check for later uses)

        #region Update Cache
        //[HttpPost("UpdateCacheDataWithFilter/", Name = "UpdateCacheDataWithFilter")]
        //public ActionResult<dynamic> UpdateCacheDataWithFilter([FromBody] JObject param)
        //{
        //    dynamic? result = null;
        //    List<ReturnResult> dataList = new List<ReturnResult>();
        //    try
        //    {
        //        SMSClientModel paramObject = param.ToObject<SMSClientModel>() ?? throw new ArgumentNullException();
        //        //JObject paramObject = param;
        //        if (paramObject != null)
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            if (!string.IsNullOrEmpty(paramObject.ClientCode) && !string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
        //            {
        //                RedisStringValue redisHashValue = new RedisStringValue();
        //                redisHashValue.Key = paramObject.ClientCode;
        //                redisHashValue.Value.Add(param);
        //                data = _repository.UpdateCacheWithFilter(redisHashValue, FilterEnum.By_Client_Telco);
        //                data.message = "By Product and Telco";
        //            }
        //            else if (!string.IsNullOrEmpty(paramObject.ClientCode) && string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
        //            {
        //                RedisStringValue redisHashValue = new RedisStringValue();
        //                redisHashValue.Key = paramObject.ClientCode;
        //                redisHashValue.Value.Add(param);
        //                data = _repository.UpdateCacheWithFilter(redisHashValue, FilterEnum.By_Client);
        //                data.message = "By Product";
        //            }
        //            else if (string.IsNullOrEmpty(paramObject.ClientCode) && !string.IsNullOrEmpty(paramObject.TelcoCode) && !string.IsNullOrEmpty(paramObject.Provider))
        //            {
        //                RedisStringValue redisHashValue = new RedisStringValue();
        //                redisHashValue.Key = paramObject.ClientCode;
        //                redisHashValue.Value.Add(param);
        //                data = _repository.UpdateCacheWithFilterByTelco(redisHashValue, FilterEnum.By_Telco);
        //                data.message = "By Telco";
        //            }
        //            else
        //            {
        //                data.status = returnResultEnum.Fail_VALUE_KEY_NULL;
        //                data.message = GetEnumDescription(data.status);
        //            }
        //            dataList.Add(data);
        //        }
        //        else
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //            dataList.Add(data);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ReturnResult data = new ReturnResult();
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //        dataList.Add(data);
        //    }
        //    result = new { dataList };
        //    return result;
        //}
        #endregion

        //[HttpPost("CreateStringData/", Name = "CreateStringData")]
        //public ActionResult<dynamic> CreateStringData([FromBody] JObject param)
        //{
        //    dynamic? result = null;
        //    ReturnResult data = new ReturnResult();
        //    data.status = returnResultEnum.Pending;
        //    dynamic paramObject = param;
        //    data.message = GetEnumDescription(data.status);
        //    try
        //    {
        //        //JObject paramObject = param;
        //        if (paramObject.data != null)
        //        {
        //            RedisStringValue redisHashValue = new RedisStringValue();
        //            redisHashValue.Value = paramObject.data;
        //            data = _repository.CreateDataWithString(redisHashValue);
        //        }
        //        else
        //        {
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //    }
        //    result = new { data };
        //    return result;
        //}

        //[HttpPost("CreateCache/", Name = "CreateCache")]
        //public ActionResult<dynamic> CreateCache([FromBody] JArray param)
        //{
        //    dynamic? result = null;
        //    List<ReturnResult> dataList = new List<ReturnResult>();
        //    dynamic paramObject = param;
        //    try
        //    {
        //        //JObject paramObject = param;
        //        if (paramObject != null)
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            foreach (var item in paramObject)
        //            {
        //                if (item.VALUE != null && item.KEY != null)
        //                {
        //                    RedisStringValue redisHashValue = new RedisStringValue();
        //                    redisHashValue.Key = item.KEY;
        //                    redisHashValue.Value = item.VALUE;
        //                    data = _repository.CreateDataWithString(redisHashValue);
        //                }
        //                else
        //                {
        //                    data.status = returnResultEnum.Fail_VALUE_KEY_NULL;
        //                    data.message = GetEnumDescription(data.status);
        //                }
        //                dataList.Add(data);
        //            }
        //        }
        //        else
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //            dataList.Add(data);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ReturnResult data = new ReturnResult();
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //        dataList.Add(data);
        //    }
        //    result = new { dataList };
        //    return result;
        //}



        //[HttpPost("UpdateCacheData/", Name = "UpdateCacheData")]
        //public ActionResult<dynamic> UpdateCacheData([FromBody] JArray param)
        //{
        //    dynamic? result = null;
        //    List<ReturnResult> dataList = new List<ReturnResult>();
        //    dynamic paramObject = param;
        //    try
        //    {
        //        //JObject paramObject = param;
        //        if (paramObject != null)
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            if (paramObject.VALUE != null && paramObject.KEY != null)
        //            {
        //                RedisStringValue redisHashValue = new RedisStringValue();
        //                redisHashValue.Key = paramObject.KEY;
        //                redisHashValue.Value = paramObject.VALUE;
        //                data = _repository.UpdateCache(redisHashValue);
        //            }
        //            dataList.Add(data);
        //        }
        //        else
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //            dataList.Add(data);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ReturnResult data = new ReturnResult();
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //        dataList.Add(data);
        //    }
        //    result = new { dataList };
        //    return result;
        //}



        //[HttpPost("SendSMS/", Name = "SendSMS")]
        //public ActionResult<dynamic> SendSMS([FromBody] JObject param)
        //{
        //    dynamic? result = null;
        //    string KBZRefNo = Guid.NewGuid().ToString();
        //    dynamic logMsg = param;
        //    logger.Info("API in. TrxnRefNum: " + logMsg.TrxnRefNum);
        //    List<ReturnResult> dataList = new List<ReturnResult>();
        //    ReturnResult data = new ReturnResult();
        //    try
        //    {
        //        SMSClientModel paramObject = param.ToObject<SMSClientModel>() ?? throw new ArgumentNullException();
        //        //JObject paramObject = param;
        //        if (paramObject != null)
        //        {
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            RedisStringValue redisHashValue = new RedisStringValue();
        //            redisHashValue.Key = paramObject.ClientCode;
        //            redisHashValue.Value.Add(param);
        //            data = _smsServiceRepository.SendSMS(redisHashValue);
        //        }
        //        else
        //        {
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { KBZRefNo = KBZRefNo, Data = data, Error = ex.ToString() });
        //    }
        //    result = new { data };
        //    logger.Info("API out. TrxnRefNum: " + logMsg.TrxnRefNum);
        //    return StatusCode(StatusCodes.Status200OK, new { KBZRefNo = KBZRefNo, Data = result, Error = new { } });
        //}
        //[HttpPost("CreateCacheFormDatabase/", Name = "CreateCacheFormDatabase")]
        //public async Task<dynamic> CreateCacheFormDatabase([FromBody] JObject param)
        //{
        //    dynamic? result = null;
        //    List<ReturnResult> dataReturnList = new List<ReturnResult>();
        //    ReturnResult dataForTelcoClientCodeList = new ReturnResult();
        //    dataForTelcoClientCodeList.status = returnResultEnum.Pending;
        //    dataForTelcoClientCodeList.message = GetEnumDescription(dataForTelcoClientCodeList.status);
        //    dynamic paramObject = param;
        //    try
        //    {
        //        if (paramObject.data != null)
        //        {
        //            RedisStringValue redisHashValue = new RedisStringValue();
        //            redisHashValue.Value = paramObject.data;
        //            List<SMSClientModel> smsClientModelList = paramObject.data.ToObject<List<SMSClientModel>>();
        //            IEnumerable<RedisStringValue> groupedSMSModel = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => new RedisStringValue { Key = y.Key, Value = JArray.FromObject(y.ToList()) });
        //            RedisStringValue groupedTelcoClientCodeList = new RedisStringValue
        //            {
        //                Key = "TelcoClientCodeList",
        //                Value_String = smsClientModelList.GroupBy(x => x.ClientCode).Select(y => y.Select(z => z.ClientCode).FirstOrDefault().ToString()).ToArray()
        //            };
        //            dataForTelcoClientCodeList = await Task.FromResult(_repository.CreateDataWithString_Task(groupedTelcoClientCodeList)).Result;
        //            if (dataForTelcoClientCodeList.status == returnResultEnum.Success)
        //            {
        //                dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' cache created";
        //            }
        //            else
        //            {
        //                dataForTelcoClientCodeList.message = "\'TelcoClientCodeList\' cache creation failed. Please re-run the cache update again.";
        //            }
        //            foreach (var item in groupedSMSModel)
        //            {
        //                ReturnResult data = new ReturnResult();
        //                data.status = returnResultEnum.Pending;
        //                data.message = GetEnumDescription(data.status);
        //                data = _repository.CreateDataWithString(item);
        //                dataReturnList.Add(data);
        //            }
        //        }
        //        else
        //        {
        //            ReturnResult data = new ReturnResult();
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //            dataReturnList.Add(data);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ReturnResult data = new ReturnResult();
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //        dataReturnList.Add(data);
        //    }
        //    result = new { dataForTelcoClientCodeList, dataReturnList };
        //    return result;
        //}
        //[HttpPost("DeleteCache/", Name = "DeleteCache")]
        //public ActionResult<dynamic> DeleteCache([FromBody] JObject param)
        //{
        //    dynamic? result = null;
        //    dynamic paramData = param;
        //    logger.Info("Delete Cache API in");
        //    List<ReturnResult> dataList = new List<ReturnResult>();
        //    ReturnResult data = new ReturnResult();
        //    try
        //    {
        //        //JObject paramObject = param;
        //        if (paramData != null)
        //        {
        //            data.status = returnResultEnum.Pending;
        //            data.message = GetEnumDescription(data.status);
        //            RedisStringValue redisHashValue = new RedisStringValue();
        //            redisHashValue.Key = paramData.Key;
        //            data = _repository.DeleteCache(redisHashValue);
        //        }
        //        else
        //        {
        //            data.status = returnResultEnum.Empty_Paramater;
        //            data.message = GetEnumDescription(data.status);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        data.status = returnResultEnum.Fail;
        //        data.message = GetEnumDescription(data.status);
        //        data.returnResult = ex.ToString();
        //    }
        //    result = new { data };
        //    logger.Info("Delete Cache API in");
        //    return result;
        //}
        #endregion

    }
}
