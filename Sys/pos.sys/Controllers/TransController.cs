//using pos.sys.Common;
//using pos.sys.Entities;
//using pos.sys.Models;
//using pos.sys.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Net;
//using System.Net.Http.Headers;

//namespace pos.sys.Controllers
//{
//    [Route("api/")]
//    [ApiController]
//    //[TypeFilter(typeof(CustomAuthorizeFilter))]
//    public class TransController : BaseController
//    {
//        internal readonly ITranService _trnsrv;
//        internal readonly ILogger<TransController> _logger;
//        string scheme, parameter = String.Empty;
//        HttpStatusCode code;
//        public TransController(ITranService trnsrv, ILogger<TransController> logger)
//        {
//            _trnsrv = trnsrv;
//            _logger = logger;
//        }

//        [HttpPost("InsertTransWithChecking")]
//        public IActionResult InsertTransWithChecking(Transaction entity)
//        {
//            AssignLogID();
                      
//            ResponseModel response = _trnsrv.InsertTranChecking(entity, RefNo).Result;
//            return new ObjectResult(response){ StatusCode = (response.HttpStatusCode)};
//        }
//        [HttpPost("InsertTransBulkWithChecking")]
//        public IActionResult InsertTransBulkWithChecking(TransactionBulk entity)
//        {
//            AssignLogID();
            
//            ResponseModel response = _trnsrv.InsertTranBulkChecking(entity, RefNo, out code).Result;
//            return new ObjectResult(response) { StatusCode = ((int)code) };
//        }
//        [HttpPost("InsertTransWithoutChecking")]
//        public IActionResult InsertTransWithoutChecking(Transaction entity)
//        {
//            AssignLogID();
            
//            ResponseModel response = _trnsrv.InsertTranWithoutChecking(entity, RefNo, out code).Result;
//            return new ObjectResult(response) { StatusCode = ((int)code) };
//        }

//        [HttpPost("UpdateTrans")]
//        public IActionResult UpdateTrans(Transaction entity)
//        {
//            AssignLogID();
            
//            ResponseModel response = _trnsrv.UpdatePartialTrans(entity, RefNo).Result;
//            return new ObjectResult(response) { StatusCode = (response.HttpStatusCode) };
//        }

//        [HttpPost("UpdateTransBulk")]
//        public IActionResult UpdateTransBulk(TransactionBulk entity)
//        {
//            AssignLogID();
            
//            ResponseModel response = _trnsrv.UpdatePartialTransBulk(entity, RefNo, out code).Result;
//            return new ObjectResult(response) { StatusCode = ((int)code) };
//        }

//        //[HttpPost("GetTransactionRecord")]
//        //public IActionResult UpdateTransBulk(ProductFilter pagination)
//        //{
//        //    AssignLogID();
//        //    //var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
//        //    //scheme = authHeader.Scheme;
//        //    //parameter = authHeader.Parameter;
//        //    ResponseModel response = _trnsrv.GetTrans(pagination, RefNo).Result;
//        //    return new ObjectResult(response) { StatusCode = ((int)code) };
//        //}
//    }
//}
