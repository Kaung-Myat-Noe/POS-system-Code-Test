using apigee.sms.biz.Common;
using apigee.sms.biz.Models;
using apigee.sms.biz.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace apigee.sms.biz.Controllers
{
    [Route("api/")]
    [ApiController]
    [TypeFilter(typeof(CustomAuthorizeFilter))]
    public class ConfigController : BaseController
    {
        internal readonly IConfigService _cfg;
        private System.Net.HttpStatusCode status;
        private ILogger<ConfigController> logger;
        public ConfigController(IConfigService cfg, ILogger<ConfigController> nlog)
        {
            _cfg = cfg;
            logger = nlog;
        }

        [HttpGet("getconfig")]
        public IActionResult getconfig()
        {
            AssignLogID();
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            scheme = authHeader.Scheme;
            parameter = authHeader.Parameter;
            HttpResponseMessage responseMessage = _cfg.GetConfig(scheme, parameter, KBZRefNo);

            return new ObjectResult(JsonConvert.DeserializeObject<RespConfigModel>(responseMessage.Content.ReadAsStringAsync().Result)) { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpGet("SMSCreateConfigBiz/", Name = "SMSCreateConfigBiz")]
        public async Task<IActionResult> SMSCreateConfigBiz()
        {
            AssignLogID();
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            scheme = authHeader.Scheme;
            parameter = authHeader.Parameter;

            HttpResponseMessage httpResponseMessage = await _cfg.SMSGetMethod(scheme, parameter, KBZRefNo, "getconfig");
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
            else if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return BadRequest(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
            else
                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
        }
        [HttpGet("SMSGetPhonePrefixesBiz/", Name = "SMSGetPhonePrefixesBiz")]
        public async Task<IActionResult> SMSGetPhonePrefixesBiz()
        {
            AssignLogID();
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            scheme = authHeader.Scheme;
            parameter = authHeader.Parameter;

            HttpResponseMessage httpResponseMessage = await _cfg.SMSGetMethod(scheme, parameter, KBZRefNo, "SMSGetPhonePrefixesSys");
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
            else if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return BadRequest(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
            else
                return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(httpResponseMessage.Content.ReadAsStringAsync().Result)));
        }
        [HttpPost("InsertProduct")]
        public IActionResult InsertProduct(ProductConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "InsertProduct", product).Result;
            
            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("UpdateProduct")]
        public IActionResult UpdateProduct(ProductConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "UpdateProduct", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("UpdateProductByTelco")]
        public IActionResult UpdateProductByTelco(ProductUpdateConfigByTelco product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "UpdateProductByTelco", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("DeleteProduct")]
        public IActionResult DeleteProduct(ProductConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoBiz(scheme, parameter, KBZRefNo, "DeleteProduct", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetProduct")]
        public IActionResult GetProduct()
        {
            AssignLogID();
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            scheme = authHeader.Scheme;
            parameter = authHeader.Parameter;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.GetConfig(scheme, parameter, KBZRefNo);

            return new ObjectResult(JsonConvert.DeserializeObject<RespConfigModel>(responseMessage.Content.ReadAsStringAsync().Result)) { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("ProductSearch")]
        public IActionResult ProductSearch(ProductFilter product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "ProductSearch", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetConfigByProductID")]
        public IActionResult GetConfigByProductID(ProductFilterByProductID product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetConfigByProductID", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetGateway_Product_Info")]
        public IActionResult GetGateway_Product_Info(GateWayProductInfo gateway)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetGateway_Product_Info", gateway).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpGet("GetGateway_Product_Count")]
        public IActionResult GetGateway_Product_Count()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSysGetMethod(scheme, parameter, KBZRefNo, "GetGateway_Product_Count", null).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }

        // New Portal apis
        [HttpPost("GetClientCodeListWithPaging")]
        public IActionResult GetClientCodeListWithPaging(Pagination pagination)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetClientCodeListWithPaging", pagination).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpGet("GetClientCodeList")]
        public IActionResult GetClientCodeList()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSysGetMethod(scheme, parameter, KBZRefNo, "GetClientCodeList", null).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("SwingByClientCode")]
        public IActionResult SwingByClientCode(ClientCodeList clientCodeList)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            logger.LogInformation("Swing By Client Code Payload: " + JsonConvert.SerializeObject(clientCodeList));
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "SwingByClientCode", clientCodeList).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("UpdateProductInfo")]
        public IActionResult UpdateProductInfo(ProductInfoUpdateConfig product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "UpdateProductInfo", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetProductByTelco")]
        public IActionResult GetProductByTelco(ProductSearchBy TelcoCode)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetProductByTelco", TelcoCode).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetProductByClientCode")]
        public IActionResult GetProductByClientCode(ProductSearchBy Client_Code)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetProductByClientCode", Client_Code).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetSMSConfigRecords")]
        public IActionResult GetSMSConfigRecords(ProductFilter product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetSMSConfigRecords", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetTransactionRecords")]
        public IActionResult GetTransactionRecords(TransactionFilter product)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSysExtendedTimeout(scheme, parameter, KBZRefNo, "GetTransactionRecords", product).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        [HttpPost("GetSMSMonthlyReport")]
        public IActionResult GetSMSMonthlyReport(SMSMonthlyCount TelcoCode)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var scheme = authHeader.Scheme;
            var parameter = authHeader.Parameter;
            AssignLogID();
            string KBZRefNo = KBZ_REF_NO;
            logger.LogInformation("API in. KBZ Reference Number:" + KBZRefNo);
            HttpResponseMessage responseMessage = _cfg.ConfigtoSys(scheme, parameter, KBZRefNo, "GetSMSMonthlyReport", TelcoCode).Result;

            return new ObjectResult(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ResponseModel>(responseMessage.Content.ReadAsStringAsync().Result)))
            { StatusCode = ((int)responseMessage.StatusCode) };
        }
        // New Portal apis
    }
}
