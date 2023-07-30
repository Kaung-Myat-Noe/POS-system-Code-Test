using apigee.sms.intf.Helper;
using apigee.sms.intf.Models;
using apigee.sms.intf.Services;
using apigee.sms.intf.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static apigee.sms.intf.Models.ReturnResultEnum;

namespace apigee.sms.intf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(CustomAuthorizeFilter))]
    public class ConfigController : BaseController
    {
        private readonly IConfigService _cfg;
        private ILogger<SMSController> logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private ISmsService _smsService;
        private readonly AppSettings _settings;
        private static string SMSProductJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSProductsCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSPhonePrefixJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSPhonePrefixCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        private static string SMSTelcoClientJsonFileName => Path.Combine(Environment.CurrentDirectory.ToString() + "\\SMSCache", $"SMSTelcoClientCache.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
        public ConfigController(IConfigService cfgService, ISmsService smsService, IHttpClientFactory httpClientFactory, ILogger<SMSController> nlog, IOptionsMonitor<AppSettings> settings)
        {
            _cfg = cfgService;
            logger = nlog;
            _smsService = smsService;
            _httpClientFactory = httpClientFactory;
            _settings = settings.CurrentValue;
        }
        [HttpPost("InsertProduct")]
        public async Task<IActionResult> InsertProduct(ProductConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            {
                responseModel.KBZRefNo = KBZRefNo;
                BaseRespError baseRespError = new BaseRespError();
                responseModel.Error = ErrorCode.NoClientCode;
                return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
                { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            }
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "InsertProduct", product).Result;
            FilterByClientCode filter = new FilterByClientCode();
            filter.UpdateFilter = product.Filter.UpdateFilter;
            filter.CLIENTCODE = product.CLIENTCODE;
            filter.CreateDataWith = product.Filter.CreateDataWith;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntfByClientCode")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(ProductConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            {
                responseModel.KBZRefNo = KBZRefNo;
                BaseRespError baseRespError = new BaseRespError();
                responseModel.Error = ErrorCode.NoClientCode;
                return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
                { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            }
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "UpdateProduct", product).Result;
            FilterByClientCode filter = new FilterByClientCode();
            filter.UpdateFilter = product.Filter.UpdateFilter;
            filter.CLIENTCODE = product.CLIENTCODE;
            filter.CreateDataWith = product.Filter.CreateDataWith;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntfByClientCode")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(ProductDelete product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            {
                responseModel.KBZRefNo = KBZRefNo;
                BaseRespError baseRespError = new BaseRespError();
                responseModel.Error = ErrorCode.NoClientCode;
                return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
                { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            }
            ProductConfig productConfig = new ProductConfig();
            productConfig.CLIENTCODE = product.CLIENTCODE;
            productConfig.TOKENUSERNAME = product.TOKENUSERNAME;
            productConfig.GateWay = "Gateway";
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "DeleteProduct", productConfig).Result;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(product.Filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntf")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("UpdateProductByTelco")]
        public async Task<IActionResult> UpdateProductByTelco(ProductUpdateConfigByTelco product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            //if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            //{
            //    responseModel.KBZRefNo = KBZRefNo;
            //    BaseRespError baseRespError = new BaseRespError();
            //    responseModel.Error = ErrorCode.NoClientCode;
            //    return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
            //    { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            //}
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "UpdateProductByTelco", product).Result;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(product.Filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntf")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpGet("GetDependencyValue")]
        public async Task<IActionResult> GetDependencyValue()
        {
            dynamic? result = null;
            result = new { Depedency = _settings.dependency };
            return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result)))
            { StatusCode = (200) };
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetProduct()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.GetConfigfromBiz(scheme, parameter, KBZRefNo, "SMSCreateConfigBiz").Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }

        [HttpPost("ProductSearch")]
        public async Task<IActionResult> ProductSearch(ProductFilter productFilter)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            productFilter.Pagination.NEXT_INDEX--;
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "ProductSearch", productFilter).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }

        [HttpPost("GetConfigByProductID")]
        public async Task<IActionResult> GetConfigByProductID(ProductFilterByProductID productFilter)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "GetConfigByProductID", productFilter).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }

        #region Check cache with key
        [HttpPost("GetConfigByClientCode/", Name = "GetConfigByClientCode")]
        public async Task<IActionResult> GetConfigByClientCode(GetConfigByClientCode param)
        {
            dynamic? result = null;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel returnResult = new ResponseModel();
            HttpStatusCode code = new HttpStatusCode();
            try
            {
                returnResult.KBZRefNo = KBZRefNo;
                //JObject paramObject = param;
                if (param != null)
                {
                    if (param.Clietn_Code != null || !String.IsNullOrEmpty(param.Clietn_Code))
                    {
                        RedisStringValue redisHashValue = new RedisStringValue();
                        redisHashValue.Key = param.Clietn_Code;
                        var data = _smsService.GetDataConfig(redisHashValue.Key.ToString(), SMSProductJsonFileName, redisHashValue.Key.ToString());
                        if (data.status == returnResultEnum.Success)
                        {
                            returnResult.Data = data.returnResult;
                        }
                        else
                        {
                            returnResult.Error = ErrorCode.NoRecordFound;
                        }
                    }
                    else
                    {
                        returnResult.Error = ErrorCode.NoClientCode;
                    }
                }
                else
                {
                    returnResult.Error = ErrorCode.NoParameter;
                }

            }
            catch (Exception ex)
            {
                returnResult.Error = ErrorCode.UnknownException;
            }
            return new ObjectResult(returnResult)
            { StatusCode = ((int)code) };
        }
        #endregion

        #region Refresh Cache
        [HttpPost("RefreshCache")]
        public async Task<IActionResult> RefreshCache(filter param)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            responseModel.KBZRefNo = KBZRefNo;
            HttpStatusCode httpStatusCode;
            var content = new StringContent(JsonConvert.SerializeObject(param), System.Text.Encoding.UTF8, "application/json");
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntf")
            };
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
            requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
            if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                //{ StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                responseModel.Data = SuccessCode.RefreshFail;
                httpStatusCode = HttpStatusCode.InternalServerError;
            }
            else
            {
                responseModel.Data = SuccessCode.RefreshSuccess;
                httpStatusCode = HttpStatusCode.OK;
            }
            return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
            { StatusCode = ((int)httpStatusCode) };
        }
        #endregion

        #region Get All Cache
        [HttpGet("GetAllConfig/", Name = "GetAllConfig")]
        public async Task<IActionResult> GetAllConfig()
        {
            dynamic? result = null;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel returnResult = new ResponseModel();
            HttpStatusCode code = new HttpStatusCode();
            try
            {
                returnResult.KBZRefNo = KBZRefNo;
                //JObject paramObject = param;

                var data = _smsService.GetAllConfig();
                if (data.status == returnResultEnum.Success)
                {
                    returnResult.Data = data.returnResult;
                }
                else
                {
                    returnResult.Error = ErrorCode.NoRecordFound;
                }


            }
            catch (Exception ex)
            {
                returnResult.Error = ErrorCode.UnknownException;
            }
            return new ObjectResult(returnResult)
            { StatusCode = ((int)code) };
        }
        #endregion

        #region Get Gateway Prodcut Count
        [HttpGet("GetGateway_Product_Count")]
        public async Task<IActionResult> GetGateway_Product_Count()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.GetConfigfromBiz(scheme, parameter, KBZRefNo, "GetGateway_Product_Count").Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion
        #region Get Gateway Product Info
        [HttpPost("GetGateway_Product_Info")]
        public async Task<IActionResult> GetGateway_Product_Info(GateWayProductInfo gateway)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "GetGateway_Product_Info", gateway).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Get Client Code List WithPaging
        [HttpPost("GetClientCodeListWithPaging")]
        public async Task<IActionResult> GetClientCodeListWithPaging(Pagination pagination)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "GetClientCodeListWithPaging", pagination).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Get Client Code List
        [HttpGet("GetClientCodeList")]
        public async Task<IActionResult> GetClientCodeList()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.GetConfigfromBiz(scheme, parameter, KBZRefNo, "GetClientCodeList").Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Swing By Client Code
        [HttpPost("SwingByClientCode")]
        public async Task<IActionResult> SwingByClientCode(ClientCodeList clientCodeList)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            logger.LogInformation("Swing By Client Code Payload:" + JsonConvert.SerializeObject(clientCodeList));
            ResponseModel responseModel = new ResponseModel();
            //if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            //{
            //    responseModel.KBZRefNo = KBZRefNo;
            //    BaseRespError baseRespError = new BaseRespError();
            //    responseModel.Error = ErrorCode.NoClientCode;
            //    return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
            //    { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            //}
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "SwingByClientCode", clientCodeList).Result;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(clientCodeList.Filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntf")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Update Product Info
        [HttpPost("UpdateProductInfo")]
        public async Task<IActionResult> UpdateProductInfo(ProductInfoUpdateConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            if (product.CLIENTCODE == null || product.CLIENTCODE == "" || String.IsNullOrEmpty(product.CLIENTCODE))
            {
                responseModel.KBZRefNo = KBZRefNo;
                BaseRespError baseRespError = new BaseRespError();
                responseModel.Error = ErrorCode.NoClientCode;
                return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(responseModel)))
                { StatusCode = (Convert.ToInt32(responseModel.Error.Code)) };
            }
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "UpdateProductInfo", product).Result;
            FilterByClientCode filter = new FilterByClientCode();
            filter.UpdateFilter = product.Filter.UpdateFilter;
            filter.CLIENTCODE = product.CLIENTCODE;
            filter.CreateDataWith = product.Filter.CreateDataWith;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = new StringContent(JsonConvert.SerializeObject(filter), System.Text.Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = content,
                    RequestUri = new Uri(_settings.URL.INTF + "RedisCache/SMSCreateConfigIntfByClientCode")
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
                requestMessage.Headers.Add("KBZ_REF_NO", KBZRefNo);
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage cacheCreationResponseMessage = await Task.FromResult(client.SendAsync(requestMessage).Result);
                if (cacheCreationResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new ObjectResult(JsonConvert.DeserializeObject(cacheCreationResponseMessage.Content.ReadAsStringAsync().Result))
                    { StatusCode = ((int)cacheCreationResponseMessage.StatusCode) };
                }
            }
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Get Product By Client Code or Telco Code
        [HttpPost("GetProductBy")]
        public async Task<IActionResult> GetProductByClientCode(ProductSearchBy productSearchBy)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            string endpoint = "";
            if (productSearchBy.TELCOCODE == null && productSearchBy.CLIENTCODE == null || productSearchBy.TELCOCODE == "" && productSearchBy.CLIENTCODE == "")
            {

                ResponseModel response = new ResponseModel();
                response.KBZRefNo = KBZRefNo;
                response.Error = ErrorCode.NoTelcoOrClient;
                return new ObjectResult(response)
                { StatusCode = (Int32.Parse(response.Error.Code)) };
            }
            else
            {
                if (productSearchBy.TELCOCODE == null || productSearchBy.TELCOCODE == "")
                {
                    endpoint = "GetProductByClientCode";
                }
                if (productSearchBy.CLIENTCODE == "" || productSearchBy.CLIENTCODE == null)
                {
                    endpoint = "GetProductByTelco";
                }
                else if (productSearchBy.CLIENTCODE == null && productSearchBy.CLIENTCODE == null || productSearchBy.CLIENTCODE == "" && productSearchBy.CLIENTCODE == "")
                {
                    ResponseModel response = new ResponseModel();
                    response.KBZRefNo = KBZRefNo;
                    response.Error = ErrorCode.NoTelcoOrClient;
                    return new ObjectResult(response)
                    { StatusCode = (Int32.Parse(response.Error.Code)) };
                }
            }
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, endpoint, productSearchBy).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Get Transaction and Config Records
        [HttpPost("GetSMSConfigRecords")]
        public async Task<IActionResult> GetSMSConfigRecords(ProductFilter productFilter)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //productFilter.Pagination.NEXT_INDEX--;
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "GetSMSConfigRecords", productFilter).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetTransactionRecords")]
        public async Task<IActionResult> GetTransactionRecords(TransactionFilter productFilter)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //productFilter.Pagination.NEXT_INDEX--;
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBizExtended(scheme, parameter, KBZRefNo, "GetTransactionRecords", productFilter).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion

        #region Get SMS Monthly Report
        [HttpPost("GetSMSMonthlyReport")]
        public async Task<IActionResult> GetSMSMonthlyReport(SMSMonthlyCount productFilter)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //productFilter.Pagination.NEXT_INDEX--;
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            ResponseModel responseModel = new ResponseModel();
            HttpResponseMessage responseMessage = _cfg.ProductFilterToBiz(scheme, parameter, KBZRefNo, "GetSMSMonthlyReport", productFilter).Result;
            return new ObjectResult(JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        #endregion        
    }
}
