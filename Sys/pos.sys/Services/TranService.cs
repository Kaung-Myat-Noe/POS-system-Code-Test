//using pos.sys.Entities;
//using pos.sys.Models;
//using pos.sys.Repositories;
//using AspNetCore.ServiceRegistration.Dynamic;
//using System.Net;
//using System.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;

//namespace pos.sys.Services
//{
//    [ScopedService]
//    public interface ITranService
//    {
//        Task<ResponseModel> InsertTranChecking(Transaction entity, string RefNo);
//        Task<ResponseModel> UpdatePartialTransBulk(TransactionBulk entity, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> InsertTranBulkChecking(TransactionBulk entity, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> InsertTranWithoutChecking(Transaction entity, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> UpdatePartialTrans(Transaction entity, string RefNo);
//        Task<ResponseModel> GetTrans(ProductFilter pagination, string RefNo);
//    }
//    public class TranService : ITranService
//    {
//        internal readonly ITransactionRepository _trans;
//        internal readonly ITransactionBulkRepository _transBulk;
        

//        internal ILogger<TranService> _logger;
//        public TranService(ITransactionRepository tran,ITransactionBulkRepository tranbulk, ILogger<TranService> logger)
//        {
//            _trans = tran;
//            _transBulk = tranbulk;
//            _logger = logger;
//        }
//        public async Task<ResponseModel> InsertTranChecking(Transaction entity, string RefNo)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {
//                //if (_trans.isExist(entity.CLIENT_CODE, entity.TRN_REF_NO))
//                //{

//                //}
//                await Task.FromResult(0);
//               // _logger.LogError("RefNo: " + RefNo + ", Request: " + JsonConvert.SerializeObject(entity));
//                _trans.Insert(entity);
//                if (_trans.Commit() > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    resp.HttpStatusCode = (int)HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    resp.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (DbUpdateConcurrencyException oe)
//            {
//                _logger.LogError("Client Code: " + entity.CLIENT_CODE + " Trn Ref Number: " + entity.TRN_REF_NO + " RefNo: " + RefNo + ", Exception: " + oe.ToString());
//                oe.Entries.Single().Reload();
//                _trans.Commit();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Client Code: " + entity.CLIENT_CODE + " Trn Ref Number: " + entity.TRN_REF_NO + " RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                if (ex.InnerException.Message.Contains("ORA-00001"))
//                {
//                    resp.Error = ErrorCode.Duplicate;
//                    resp.HttpStatusCode = (int)HttpStatusCode.BadRequest;
//                    return resp;
//                }
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }

//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                resp.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
//            }
//            return resp;
//        }

//        public Task<ResponseModel> InsertTranBulkChecking(TransactionBulk entity, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {
//                if (_transBulk.isExist(entity.CLIENT_CODE, entity.TRN_REF_NO))
//                {
//                    resp.Error = ErrorCode.Duplicate;
//                    statusCode = HttpStatusCode.BadRequest;
//                    return Task.FromResult(resp);
//                }

//                _transBulk.Insert(entity);
//                if (Task.FromResult(_transBulk.Commit()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    statusCode = HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (Exception ex)
//            {                
//                _logger.LogError("Exception Start");
//                //_logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                _logger.LogError("Client Code: " + entity.CLIENT_CODE + "Trn Ref Number: " + entity.TRN_REF_NO + "RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                _logger.LogError("Inner Exception Start");
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }

//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                statusCode = HttpStatusCode.InternalServerError;
//            }
//            return Task.FromResult(resp);
//        }
//        public Task<ResponseModel> InsertTranWithoutChecking(Transaction entity, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {   
//                _trans.Insert(entity);
//                if (Task.FromResult(_trans.Commit()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    statusCode = HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (Exception ex)
//            {
//                //_logger.LogError("RefNo: " + JsonConvert.SerializeObject(entity));
//                _logger.LogError("Client Code: " +entity.CLIENT_CODE + "Trn Ref Number: "+  entity.TRN_REF_NO + "RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                statusCode = HttpStatusCode.InternalServerError;
//            }
//            return Task.FromResult(resp);
//        }
//        public async Task<ResponseModel> UpdatePartialTrans(Transaction entity, string RefNo)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {
//                //_trans.UpdatePartial(entity, o => o.RESPONSE, o => o.RESPONSE_DATETIME, o => o.MERCHANT_RESPONSE, o => o.PROCESS_STAGES);
//                _trans.Update(entity); 
//                if (await _trans.CommitAsync() > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    resp.HttpStatusCode = (int)HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    resp.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (DbUpdateConcurrencyException dce)
//            {
//                /*
//                _logger.LogInformation("Error: " + dce.Message);
//                foreach (var entry in dce.Entries)
//                {
//                    var proposeValue = entry.CurrentValues;
//                    var databaseValue = entry.GetDatabaseValues();
//                    foreach (var property in proposeValue.Properties)
//                    {
//                        var proValue = proposeValue[property];
//                        var dbValue = databaseValue[property];
//                    }
//                    entry.OriginalValues.SetValues(databaseValue);
//                }
//                */

//                dce.Entries.Single().Reload();
//                _trans.Commit();

//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }

//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                resp.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
//            }
//            return resp;
//        }
//        public Task<ResponseModel> UpdatePartialTransBulk(TransactionBulk entity, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {
//                _transBulk.UpdatePartial(entity, o => o.RESPONSE, o => o.RESPONSE_DATETIME, o => o.MERCHANT_RESPONSE, o => o.PROCESS_STAGE, o=>o.M_RESP_DATETIME );
//                if (Task.FromResult(_transBulk.Commit()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    statusCode = HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Exception Start");
//                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                _logger.LogError("Inner Exception Start");
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }

//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                statusCode = HttpStatusCode.InternalServerError;
//            }
//            return Task.FromResult(resp);
//        }

//        public async Task<ResponseModel> GetTrans(ProductFilter pagination, string RefNo)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            MetaData Pagination = new MetaData();
//            pagination.Pagination.NEXT_INDEX = pagination.Pagination.NEXT_INDEX - 1;
//            try
//            {
//                int count = _trans.GetAll().Count();
//                //responseModel.Data = new { Pagination, clientCodes };
//                IList<Transaction> Records = _trans.Query(x=>x.MERCHANT_ID.Contains(pagination.SearchText) || x.CLIENT_CODE.Contains(pagination.SearchText) || x.TELCO_CODE.Contains(pagination.SearchText) || x.TRN_REF_NO.Contains(pagination.SearchText) || x.SUBSCRIBER_NO.Contains(pagination.SearchText)).OrderByDescending(y=>y.REQUEST_DATETIME).Skip(pagination.Pagination.NEXT_INDEX * pagination.Pagination.PAGE_TOTAL).Take(pagination.Pagination.PAGE_TOTAL).ToList();
//                Pagination.PageSize = pagination.Pagination.PAGE_TOTAL;
//                Pagination.CurrentPage = pagination.Pagination.NEXT_INDEX + 1;
//                Pagination.TotalCount = count;
//                Pagination.TotalPages = count % pagination.Pagination.PAGE_TOTAL != 0 ? count / pagination.Pagination.PAGE_TOTAL + 1 : count / pagination.Pagination.PAGE_TOTAL;
//                if (Records.Count() > 0)
//                {
//                    resp.Data = new { Pagination, Records};
//                    resp.HttpStatusCode = (int)HttpStatusCode.OK;
//                }
//                else
//                {
//                    resp.Error = ErrorCode.NoRecordFound;
//                    resp.HttpStatusCode = (int)HttpStatusCode.BadRequest;
//                }
//            }            
//            catch (Exception ex)
//            {
//                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                if (ex.InnerException.Message.Contains("ORA-00001"))
//                {
//                    resp.Error = ErrorCode.Duplicate;
//                    resp.HttpStatusCode = (int)HttpStatusCode.BadRequest;
//                    return resp;
//                }
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }

//                resp.Error = ErrorCode.UnknownException;
//                resp.Error.Details.Add(ErrorCode.OperationError);
//                resp.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
//            }
//            return resp;
//        }
//    }
//}
