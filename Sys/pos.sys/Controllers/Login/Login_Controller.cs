using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pos.sys.Services;
using pos.sys.Models;
using pos.sys.Common;
using pos.sys.Entities;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using pos.sys.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace pos.sys.Controllers
{
    [Route("api/")]
    [ApiController]
    //[TypeFilter(typeof(CustomAuthorizeFilter))]
    public class Login_Controller : BaseController
    {
        internal readonly IUserService _userService;
        internal readonly ILogger<Login_Controller> _logger;
        private readonly AppSettings _settings;
        HttpStatusCode code;
        public Login_Controller(IUserService userService, ILogger<Login_Controller> logger, IOptionsMonitor<AppSettings> settings)
        {
            _userService = userService;
            _logger = logger;
            _settings = settings.CurrentValue;
        }

        [HttpGet("getuser")]
        public IActionResult getuser()
        {
            AssignLogID();
            _logger.LogInformation("API in.  Reference Number:" + RefNo);
            ResponseModel responseModel = new ResponseModel();
            try
            {
                IList<UserModel> config = _userService.GetUser();
                if (config.Count > 0)
                {
                    responseModel.Data = config;
                    return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
                }
                else
                {
                    responseModel.Error = ErrorCode.NoRecordFound;
                    return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Start");
                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception Start");
                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
                }
                responseModel.Error = ErrorCode.UnknownException;
                responseModel.Error.Details.Add(ErrorCode.OperationError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginModel loginModel)
        {
            AssignLogID();
            _logger.LogInformation("API in.  Reference Number:" + RefNo);
            ResponseModel responseModel = new ResponseModel();
            try
            {
                UserModel user= _userService.Login(loginModel);
                if (user != null)
                {
                    var token = GenerateToken(user, _settings.JwtConfig.issuer, _settings.JwtConfig.audienceId, _settings.JwtConfig.expiration, _settings.JwtConfig.Key);
                    responseModel.Data = token;
                    return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
                }
                else
                {
                    responseModel.Error = ErrorCode.WrongUserNameAndPass;
                    return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.WrongUserNameAndPass });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Start");
                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception Start");
                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
                }
                responseModel.Error = ErrorCode.UnknownException;
                responseModel.Error.Details.Add(ErrorCode.OperationError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
            }
        }
        //private string GenerateToken(UserModel entity)
        //{
        //    var now = DateTime.UtcNow;

        //    var claim = new[]
        //    {
        //            new Claim(ClaimTypes.Name, "SmthGood"),
        //            new Claim("SmthGood:email", entity.email),
        //            new Claim("SmthGood:name", entity.name),
        //            new Claim("SmthGood:Id", entity.Id.ToString())

        //    };
        //    var secretKey = Guid.NewGuid().ToString().Replace("-", "");
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        //    var tokeOptions = new JwtSecurityToken(
        //        issuer: _settings.JwtConfig.issuer,
        //        audience: _settings.JwtConfig.audienceId,
        //        claims: claim,
        //        notBefore: now,
        //        expires: now.AddMinutes(_settings.JwtConfig.expiration),
        //        signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        //}
        //[HttpGet("SMSGetPhonePrefixesSys/", Name = "SMSGetPhonePrefixesSys")]
        //public IActionResult SMSGetPhonePrefixesSys()
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        IList<PhonePrefixes> config = _configService.GetPhonePrefixes();
        //        if (config.Count > 0)
        //        {
        //            responseModel.Data = config;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("InsertProduct")]
        //public IActionResult InsertProduct(ProductConfig product)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel response = _configService.InsertProduct(product, RefNo, out code).Result;
        //    return new ObjectResult(response) { StatusCode = ((int)code) };
        //}
        //[HttpPost("UpdateProduct")]
        //public IActionResult UpdateProduct(ProductUpdateConfig product)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel response = _configService.UpdateProduct(product, RefNo, out code).Result;
        //    return new ObjectResult(response) { StatusCode = ((int)code) };
        //}
        //[HttpPost("UpdateProductByTelco")]
        //public IActionResult UpdateProductByTelco(ProductUpdateConfigByTelco product)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel response = _configService.UpdateProductByTelco(product, RefNo, out code).Result;
        //    if (response.HttpStatusCode == (int)HttpStatusCode.OK)
        //    {
        //        IList<ConfigModelChecker> configs = JsonConvert.DeserializeObject<IList<ConfigModelChecker>>(JsonConvert.SerializeObject(_configService.GetConfigByTelco(product.TELCOCODE).ToList()));
        //        if (configs.Count > 0)
        //        {
        //            response.Data = configs;
        //            return Ok(new { RefNo = RefNo, Data = response.Data, Error = response.Error });
        //        }
        //        else
        //        {
        //            response.Data = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = response.Data, Error = response.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    else
        //    {
        //        return new ObjectResult(response) { StatusCode = ((int)code) };
        //    }
        //}
        //[HttpPost("DeleteProduct")]
        //public IActionResult DeleteProduct(ProductDelete product)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel response = _configService.DeleteProduct(product, RefNo, out code).Result;
        //    return new ObjectResult(response) { StatusCode = ((int)code) };
        //}

        //[HttpGet("GetConfigWithFilter")]
        //public IActionResult GetConfigWithFilter()
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        IList<ConfigModel> config = _configService.GetConfig();
        //        if (config.Count > 0)
        //        {
        //            responseModel.Data = config;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            //responseModel.Data = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}

        //[HttpPost("ProductSearch")]
        //public IActionResult ProductSearch(ProductFilter productFilter)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    MetaData Pagination = new MetaData();

        //    try
        //    {

        //        ListData config = new ListData();
        //        config = _configService.GetFilterdeConfig(productFilter);
        //        Pagination.PageSize = productFilter.Pagination.PAGE_TOTAL;
        //        Pagination.CurrentPage = productFilter.Pagination.NEXT_INDEX + 1;
        //        Pagination.TotalCount = config.ProductCount;
        //        Pagination.TotalPages = config.ProductCount % productFilter.Pagination.PAGE_TOTAL != 0 ? config.ProductCount / productFilter.Pagination.PAGE_TOTAL + 1 : config.ProductCount / productFilter.Pagination.PAGE_TOTAL;

        //        if (config.Products.Count > 0)
        //        {
        //            responseModel.Data = new { Pagination, config.Products };
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = new { Pagination, config.Products };
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());

        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }

        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetConfigByProductID")]
        //public IActionResult GetConfigByProductID(ProductFilterByProductID productFilter)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {

        //        List<ProductConfigSearch> config = new List<ProductConfigSearch>();
        //        config = _configService.GetConfigByProductID(productFilter);

        //        if (config.Count > 0)
        //        {
        //            responseModel.Data = config;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());

        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }

        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}

        //[HttpPost("GetGateway_Product_Info")]
        //public IActionResult GetGateway_Product_Info(GateWayProductInfo gateway)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        IList<ConfigModelChecker> clientCodes = JsonConvert.DeserializeObject<IList<ConfigModelChecker>>(JsonConvert.SerializeObject(_configService.GetConfigByGateway(gateway.GATEWAY).DistinctBy(x => x.ClientCode).ToList()));

        //        if (clientCodes.Count > 0)
        //        {
        //            responseModel.Data = clientCodes;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}

        //[HttpGet("GetGateway_Product_Count")]
        //public IActionResult GetGateway_Product_Count()
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        IList<ConfigModel> config = _configService.GetConfig();
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, COUNT = y.Select(z => z.ClientCode).Distinct().Count() }).ToList();
        //        if (clientCodes.Count > 0)
        //        {
        //            responseModel.Data = clientCodes;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return BadRequest(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetClientCodeListWithPaging")]
        //public IActionResult GetClientCodeListWithPaging(Pagination pagination)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    MetaData Pagination = new MetaData();
        //    pagination.NEXT_INDEX = pagination.NEXT_INDEX - 1;
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        List<ListDataByClientCodeOneProduct> clientCodes = _configService.GetFilterdeConfigByClientCodeWithPage(pagination);

        //        if (clientCodes.Count > 0)
        //        {
        //            Pagination.PageSize = pagination.PAGE_TOTAL;
        //            Pagination.CurrentPage = pagination.NEXT_INDEX + 1;
        //            Pagination.TotalCount = clientCodes[0].ProductCount;
        //            Pagination.TotalPages = clientCodes[0].ProductCount % pagination.PAGE_TOTAL != 0 ? clientCodes[0].ProductCount / pagination.PAGE_TOTAL + 1 : clientCodes[0].ProductCount / pagination.PAGE_TOTAL;
        //            responseModel.Data = new { Pagination, clientCodes };
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = new { Pagination, clientCodes };
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpGet("GetClientCodeList")]
        //public IActionResult GetClientCodeList()
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        List<ListDataByClientCodeOneProduct> clientCodes = _configService.GetFilterdeConfigByClientCode();

        //        if (clientCodes.Count > 0)
        //        {
        //            responseModel.Data = clientCodes;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("SwingByClientCode")]
        //public IActionResult SwingByClientCode(ClientCodeList clientCodeList)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number: " + RefNo);
        //    _logger.LogInformation("Swing By Client Code Payload: " + JsonConvert.SerializeObject(clientCodeList));
        //    IList<ListDataByClientCodeOneProduct> productConfigSearches = new List<ListDataByClientCodeOneProduct>();
        //    //IList<ProductConfigSearch> productConfigSearches = new List<ProductConfigSearch>();
        //    //foreach (var clientCode in clientCodeList.CLIENTCODES)
        //    //{
        //    //    productConfigSearches.Add(_configService.GetConfigFullVersionByClientCode(clientCode));
        //    //}
        //    ResponseModel responseModel = new ResponseModel();
        //    ResponseModel response = new ResponseModel();
        //    //if (clientCodeList.TELCOLIST == null || clientCodeList.TELCOLIST.Count == 0) {
        //    //    responseModel.Error = ErrorCode.TelcoCodeListNull;
        //    //    return BadRequest(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    //}

        //    response = _configService.SwingByClientCode(clientCodeList, RefNo, out code).Result;
        //    if (code == HttpStatusCode.OK)
        //    {
        //        foreach (string clientCode in clientCodeList.CLIENTCODES)
        //        {
        //            ListDataByClientCodeOneProduct listData = new ListDataByClientCodeOneProduct();
        //            listData.ClientCode = clientCode;
        //            listData.Product = _configService.GetConfigFullVersionByClientCode(clientCode);
        //            productConfigSearches.Add(listData);
        //        }


        //        if (productConfigSearches.Count > 0)
        //        {
        //            responseModel.Data = productConfigSearches;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    else
        //    {
        //        return new ObjectResult(response) { StatusCode = ((int)code) };
        //    }
        //}
        //[HttpPost("UpdateProductInfo")]
        //public IActionResult UpdateProductInfo(ProductInfoUpdateConfig product)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel response = _configService.UpdateProductInfo(product, RefNo, out code).Result;
        //    return new ObjectResult(response) { StatusCode = ((int)code) };
        //}
        //[HttpPost("GetProductByTelco")]
        //public IActionResult GetProductByTelco(ProductSearchBy productSearchBy)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        IList<ConfigModelChecker> configs = JsonConvert.DeserializeObject<IList<ConfigModelChecker>>(JsonConvert.SerializeObject(_configService.GetConfigByTelco(productSearchBy.TELCOCODE).ToList()));

        //        if (configs.Count > 0)
        //        {
        //            responseModel.Data = configs;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            //responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetProductByClientCode")]
        //public IActionResult GetProductByClientCode(ProductSearchBy productSearchBy)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        //List<Product> clientCodes = _configService.GetConfigByClientCode(productSearchBy.CLIENTCODE);
        //        IList<ConfigModelChecker> clientCodes = JsonConvert.DeserializeObject<IList<ConfigModelChecker>>(JsonConvert.SerializeObject(_configService.GetConfigByClientCode(productSearchBy.CLIENTCODE))).ToList();
        //        if (clientCodes.Count > 0)
        //        {
        //            responseModel.Data = clientCodes;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetSMSConfigRecords")]
        //public IActionResult GetSMSConfigRecords(ProductFilter productFilter)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    MetaData Pagination = new MetaData();
        //    productFilter.Pagination.NEXT_INDEX = productFilter.Pagination.NEXT_INDEX - 1;
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        ListSMSRecordData clientCodes = _configService.GetSMSConfigRecords(productFilter);

        //        if (clientCodes.Records.Count > 0)
        //        {
        //            Pagination.PageSize = productFilter.Pagination.PAGE_TOTAL;
        //            Pagination.CurrentPage = productFilter.Pagination.NEXT_INDEX + 1;
        //            Pagination.TotalCount = clientCodes.ProductCount;
        //            Pagination.TotalPages = clientCodes.ProductCount % productFilter.Pagination.PAGE_TOTAL != 0 ? clientCodes.ProductCount / productFilter.Pagination.PAGE_TOTAL + 1 : clientCodes.ProductCount / productFilter.Pagination.PAGE_TOTAL;
        //            responseModel.Data = new { Pagination, clientCodes.Records };
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = new { Pagination, clientCodes.Records };
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetTransactionRecords")]
        //public IActionResult GetTransactionRecords(TransactionFilter productFilter)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    MetaData Pagination = new MetaData();
        //    productFilter.Pagination.NEXT_INDEX = productFilter.Pagination.NEXT_INDEX - 1;
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        ListTransactionRecordData clientCodes = _configService.GetTrans(productFilter);

        //        if (clientCodes.Records.Count > 0)
        //        {
        //            Pagination.PageSize = productFilter.Pagination.PAGE_TOTAL;
        //            Pagination.CurrentPage = productFilter.Pagination.NEXT_INDEX + 1;
        //            Pagination.TotalCount = clientCodes.ProductCount;
        //            Pagination.TotalPages = clientCodes.ProductCount % productFilter.Pagination.PAGE_TOTAL != 0 ? clientCodes.ProductCount / productFilter.Pagination.PAGE_TOTAL + 1 : clientCodes.ProductCount / productFilter.Pagination.PAGE_TOTAL;
        //            responseModel.Data = new { Pagination, clientCodes.Records };
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = new { Pagination, clientCodes.Records };
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
        //[HttpPost("GetSMSMonthlyReport")]
        //public IActionResult GetSMSMonthlyReport(SMSMonthlyCountRequest productFilter)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        IList<SMSMonthlyCountResponse> clientCodes = _configService.GetMonthlySMSCount(productFilter);
        //        //return Ok();

        //        if (clientCodes.Count > 0)
        //        {
        //            responseModel.Data = new { FromMonth = productFilter.StartDate.ToString("MMMM", CultureInfo.InvariantCulture), ToMonth = productFilter.EndDate.ToString("MMMM", CultureInfo.InvariantCulture), clientCodes };
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = clientCodes;
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = ErrorCode.NoRecordFound });
        //    }
        //}

        //[HttpPost("GetGateway")]
        //public IActionResult GetGateway(string gateWay)
        //{
        //    AssignLogID();
        //    _logger.LogInformation("API in.  Reference Number:" + RefNo);
        //    ResponseModel responseModel = new ResponseModel();
        //    try
        //    {
        //        //IList<ConfigModel> config = _configService.GetConfigByGateway(gateway);
        //        //var clientCodes = config.ToList().Select(y => new GatewayData { GATEWAY = y.Gateway, CLIENTCODE= config.ToList().Select(z=>z.ClientCode).Distinct().ToList() }).GroupBy(c=>c.GATEWAY).Distinct().ToList();
        //        //IList<GatewayData> clientCodes = config.GroupBy(x => x.Gateway).Select(y => new GatewayData { GATEWAY = y.Key, CLIENTCODE = y.Select(z => z.ClientCode).Distinct().ToList() }).ToList();
        //        Gateway clientCodes = _configService.GetGateway(gateWay);

        //        if (clientCodes != null)
        //        {

        //            responseModel.Data = clientCodes;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //        }
        //        else
        //        {
        //            responseModel.Data = clientCodes;
        //            responseModel.Error = ErrorCode.NoRecordFound;
        //            return Ok(new { RefNo = RefNo, Data = responseModel.Data, Error = ErrorCode.NoRecordFound, Message_Detail = ErrorCode.NoRecordFound });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception Start");
        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
        //        if (ex.InnerException != null)
        //        {
        //            _logger.LogError("Inner Exception Start");
        //            _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
        //        }
        //        responseModel.Error = ErrorCode.UnknownException;
        //        responseModel.Error.Details.Add(ErrorCode.OperationError);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { RefNo = RefNo, Data = responseModel.Data, Error = responseModel.Error });
        //    }
        //}
    }
}
