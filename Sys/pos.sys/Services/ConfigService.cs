//using pos.sys.Entities;
//using pos.sys.Models;
//using pos.sys.Repositories;
//using AspNetCore.ServiceRegistration.Dynamic;
//using AutoMapper;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Newtonsoft.Json;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Diagnostics;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net;
//using System.Reflection;
//using System.Runtime.CompilerServices;

//namespace pos.sys.Services
//{
//    [ScopedService]
//    public interface IConfigService
//    {
//        IList<ConfigModel> GetConfig();
//        List<ProductConfigSearch> GetConfigByProductID(ProductFilterByProductID productFilter);
//        ListData GetFilterdeConfig(ProductFilter productFilter);
//        IList<PhonePrefixes> GetPhonePrefixes();
//        IList<ProductConfigSearch> GetConfigFullVersion();
//        Task<ResponseModel> UpdateProductByTelco(ProductUpdateConfigByTelco entityProductConfig, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> InsertProduct(ProductConfig productEntity, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> UpdateProduct(ProductUpdateConfig entityProductConfig, string RefNo, out HttpStatusCode statusCode);
//        Task<ResponseModel> DeleteProduct(ProductDelete entityProductConfig, string RefNo, out HttpStatusCode statusCode);
//        IList<ConfigModel> GetConfigByGateway(string gateway);
//        IList<ConfigModel> GetConfigByTelco(string TelcoCode);
//        List<Product> GetConfigByClientCode(string clientCode);
//        List<ListDataByClientCodeOneProduct> GetFilterdeConfigByClientCode();
//        List<ListDataByClientCodeOneProduct> GetFilterdeConfigByClientCodeWithPage(Pagination pagination);
//        Task<ResponseModel> SwingByClientCode(ClientCodeList clientCodeList, string RefNo, out HttpStatusCode statusCode);
//        ProductConfigSearch GetConfigFullVersionByClientCode(string CLIENTCODE);
//        Task<ResponseModel> UpdateProductInfo(ProductInfoUpdateConfig entityProductConfig, string RefNo, out HttpStatusCode statusCode);
//        ListSMSRecordData GetSMSConfigRecords(ProductFilter productFilter);
//        ListTransactionRecordData GetTrans(TransactionFilter productFilter);
//        IList<ProductConfigSearch> GetConfigFullVersionByClientCodeList(string CLIENTCODE);
//        IList<SMSMonthlyCountResponse> GetMonthlySMSCount(SMSMonthlyCountRequest smsCount);
//        Gateway GetGateway(string gateWay);
//    }
//    public class ConfigService : IConfigService
//    {
//        internal readonly IGatewayRepository _gateway;
//        internal readonly ISMSConfigRecordRepository _configRecord;
//        internal readonly IProductRepository _product;
//        internal readonly ITransactionRepository _trans;
//        internal readonly ITransactionTempRepository _transTemp;
//        internal readonly ITransactionHistoryRepository _transHistory;
//        internal readonly IMappingRepository _mapping;
//        internal readonly IPhonePrefixRepository _phonePrefix;
//        internal ILogger<ConfigService> _logger;
//        internal readonly ConfigDBContext _ctx;
//        internal readonly IMapper _mapper;

//        public ConfigService(IGatewayRepository gateway, IProductRepository product, IMappingRepository mapping, IPhonePrefixRepository phonePrefix, ISMSConfigRecordRepository smsConfigRecordRepository, ITransactionRepository transactionRepository, ITransactionTempRepository transactionTempRepository, ITransactionHistoryRepository transactionHistoryRepository, ILogger<ConfigService> logger, IMapper mapper, ConfigDBContext ctx)
//        {
//            _gateway = gateway;
//            _product = product;
//            _mapping = mapping;
//            _phonePrefix = phonePrefix;
//            _configRecord = smsConfigRecordRepository;
//            _logger = logger;
//            _ctx = ctx;
//            _trans = transactionRepository;
//            _transTemp = transactionTempRepository;
//            _transHistory = transactionHistoryRepository;
//            _mapper = mapper;
//        }

//        public IList<ConfigModel> GetConfig()
//        {
//            IList<ConfigModel> config = (from m in _mapping.GetAll()
//                                         join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                         join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                         select new ConfigModel()
//                                         {
//                                             ClientCode = p.CLIENTCODE,
//                                             CheckDuplicate = p.CHECKDUPLICATE,
//                                             TelcoCode = m.OPERATOR,
//                                             TokenUserName = p.TOKENUSERNAME,
//                                             UAT_SenderID = p.UAT_SENDERID,
//                                             Pro_SenderID = p.PRO_SENDERID,
//                                             Department = p.DEPARTMENT,
//                                             Gateway = g.GATEWAY,
//                                             ServiceURL1 = g.SERVICEURL1,
//                                             ServiceURL2 = g.SERVICEURL2,
//                                             ServiceURL3 = g.SERVICEURL3,
//                                             Timeout = Convert.ToInt16(g.TIMEOUT.Value),
//                                             SecretID = p.SECRETID,
//                                             SecretKey = p.SECRETKEY,
//                                             Route_to_env = p.ROUTE_TO_ENV,
//                                             clientid = p.CLIENTID,
//                                             clientsecret = p.CLIENTSECRET
//                                         }).ToList();
//            return config;
//        }
//        public Gateway GetGateway(string gateWay)
//        {
//            Gateway config = _gateway.Query(x=>x.GATEWAY.Equals(gateWay)).FirstOrDefault();
//            return config;
//        }

//        public IList<ConfigModel> GetConfigByGateway(string gateway)
//        {
//            IList<ConfigModel> config = (from m in _mapping.GetAll()
//                                         join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                         join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                         where g.GATEWAY.Equals(gateway)
//                                         select new ConfigModel()
//                                         {
//                                             ProductID = p.PRODUCTID,
//                                             ClientCode = p.CLIENTCODE,
//                                             CheckDuplicate = p.CHECKDUPLICATE,
//                                             TokenUserName = p.TOKENUSERNAME,
//                                             UAT_SenderID = p.UAT_SENDERID,
//                                             Pro_SenderID = p.PRO_SENDERID,
//                                             Department = p.DEPARTMENT,
//                                             Gateway = g.GATEWAY,
//                                             ServiceURL1 = g.SERVICEURL1,
//                                             ServiceURL2 = g.SERVICEURL2,
//                                             ServiceURL3 = g.SERVICEURL3,
//                                             SecretID = p.SECRETID,
//                                             SecretKey = p.SECRETKEY
//                                         }).ToList();
//            return config;
//        }


//        public IList<ProductConfigSearch> GetConfigFullVersion()
//        {
//            IList<ProductConfigSearch> config = (from m in _mapping.GetAll()
//                                                 join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                                 join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                                 select new ProductConfigSearch()
//                                                 {
//                                                     PRODUCTID = p.PRODUCTID == null ? String.Empty : p.PRODUCTID,
//                                                     CLIENTCODE = p.CLIENTCODE == null ? String.Empty : p.CLIENTCODE,
//                                                     TOKENUSERNAME = p.TOKENUSERNAME == null ? String.Empty : p.TOKENUSERNAME,
//                                                     UAT_SENDERID = p.UAT_SENDERID == null ? String.Empty : p.UAT_SENDERID,
//                                                     PRO_SENDERID = p.PRO_SENDERID == null ? String.Empty : p.PRO_SENDERID,
//                                                     DEPARTMENT = p.DEPARTMENT == null ? String.Empty : p.DEPARTMENT,
//                                                     DESCRIPTION = p.DESCRIPTION == null ? String.Empty : p.DESCRIPTION,
//                                                     REQUESTER = p.REQUESTER == null ? String.Empty : p.REQUESTER,
//                                                     REQUESTDATETIME = p.REQUESTDATETIME == null ? System.DateTime.Now : p.REQUESTDATETIME,
//                                                     TARGETDATETIME = p.TARGETDATETIME == null ? System.DateTime.Now : p.TARGETDATETIME,
//                                                     APPROVEDDATETIME = p.APPROVEDDATETIME == null ? System.DateTime.Now : p.APPROVEDDATETIME,
//                                                     APPROVEDBY = p.APPROVEDBY == null ? String.Empty : p.APPROVEDBY,
//                                                     CHECKDUPLICATE = p.CHECKDUPLICATE == null ? String.Empty : p.CHECKDUPLICATE,
//                                                     SECRETID = p.SECRETID == null ? String.Empty : p.SECRETID,
//                                                     SECRETKEY = p.SECRETKEY == null ? String.Empty : p.SECRETKEY,
//                                                     ROUTE_TO_ENV = p.ROUTE_TO_ENV == null ? String.Empty : p.ROUTE_TO_ENV,
//                                                     CLIENTID = p.CLIENTID == null ? String.Empty : p.CLIENTID,
//                                                     CLIENTSECRET = p.CLIENTSECRET == null ? String.Empty : p.CLIENTSECRET,
//                                                     GateWay = g.GATEWAY == null ? String.Empty : g.GATEWAY,
//                                                     OPERATOR = m.OPERATOR == null ? String.Empty : m.OPERATOR
//                                                 }).ToList();
//            return config;
//        }
//        public IList<ProductConfigSearch> GetConfigFullVersionByProductID(string PRODUCTID)
//        {
//            IList<ProductConfigSearch> config = (from m in _mapping.GetAll()
//                                                 join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                                 join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                                 where p.PRODUCTID == PRODUCTID
//                                                 select new ProductConfigSearch()
//                                                 {
//                                                     PRODUCTID = p.PRODUCTID == null ? String.Empty : p.PRODUCTID,
//                                                     CLIENTCODE = p.CLIENTCODE == null ? String.Empty : p.CLIENTCODE,
//                                                     TOKENUSERNAME = p.TOKENUSERNAME == null ? String.Empty : p.TOKENUSERNAME,
//                                                     UAT_SENDERID = p.UAT_SENDERID == null ? String.Empty : p.UAT_SENDERID,
//                                                     PRO_SENDERID = p.PRO_SENDERID == null ? String.Empty : p.PRO_SENDERID,
//                                                     DEPARTMENT = p.DEPARTMENT == null ? String.Empty : p.DEPARTMENT,
//                                                     DESCRIPTION = p.DESCRIPTION == null ? String.Empty : p.DESCRIPTION,
//                                                     REQUESTER = p.REQUESTER == null ? String.Empty : p.REQUESTER,
//                                                     REQUESTDATETIME = p.REQUESTDATETIME == null ? System.DateTime.Now : p.REQUESTDATETIME,
//                                                     TARGETDATETIME = p.TARGETDATETIME == null ? System.DateTime.Now : p.TARGETDATETIME,
//                                                     APPROVEDDATETIME = p.APPROVEDDATETIME == null ? System.DateTime.Now : p.APPROVEDDATETIME,
//                                                     APPROVEDBY = p.APPROVEDBY == null ? String.Empty : p.APPROVEDBY,
//                                                     CHECKDUPLICATE = p.CHECKDUPLICATE == null ? String.Empty : p.CHECKDUPLICATE,
//                                                     SECRETID = p.SECRETID == null ? String.Empty : p.SECRETID,
//                                                     SECRETKEY = p.SECRETKEY == null ? String.Empty : p.SECRETKEY,
//                                                     ROUTE_TO_ENV = p.ROUTE_TO_ENV == null ? String.Empty : p.ROUTE_TO_ENV,
//                                                     CLIENTID = p.CLIENTID == null ? String.Empty : p.CLIENTID,
//                                                     CLIENTSECRET = p.CLIENTSECRET == null ? String.Empty : p.CLIENTSECRET,
//                                                     GateWay = g.GATEWAY == null ? String.Empty : g.GATEWAY,
//                                                     OPERATOR = m.OPERATOR == null ? String.Empty : m.OPERATOR
//                                                 }).ToList();
//            return config;
//        }
//        public ProductConfigSearch GetConfigFullVersionByClientCode(string CLIENTCODE)
//        {
//            ProductConfigSearch? config = (from m in _mapping.GetAll()
//                                           join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                           join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                           where p.CLIENTCODE == CLIENTCODE
//                                           select new ProductConfigSearch()
//                                           {
//                                               PRODUCTID = p.PRODUCTID == null ? String.Empty : p.PRODUCTID,
//                                               CLIENTCODE = p.CLIENTCODE == null ? String.Empty : p.CLIENTCODE,
//                                               TOKENUSERNAME = p.TOKENUSERNAME == null ? String.Empty : p.TOKENUSERNAME,
//                                               UAT_SENDERID = p.UAT_SENDERID == null ? String.Empty : p.UAT_SENDERID,
//                                               PRO_SENDERID = p.PRO_SENDERID == null ? String.Empty : p.PRO_SENDERID,
//                                               DEPARTMENT = p.DEPARTMENT == null ? String.Empty : p.DEPARTMENT,
//                                               DESCRIPTION = p.DESCRIPTION == null ? String.Empty : p.DESCRIPTION,
//                                               REQUESTER = p.REQUESTER == null ? String.Empty : p.REQUESTER,
//                                               REQUESTDATETIME = p.REQUESTDATETIME == null ? System.DateTime.Now : p.REQUESTDATETIME,
//                                               TARGETDATETIME = p.TARGETDATETIME == null ? System.DateTime.Now : p.TARGETDATETIME,
//                                               APPROVEDDATETIME = p.APPROVEDDATETIME == null ? System.DateTime.Now : p.APPROVEDDATETIME,
//                                               APPROVEDBY = p.APPROVEDBY == null ? String.Empty : p.APPROVEDBY,
//                                               CHECKDUPLICATE = p.CHECKDUPLICATE == null ? String.Empty : p.CHECKDUPLICATE,
//                                               SECRETID = p.SECRETID == null ? String.Empty : p.SECRETID,
//                                               SECRETKEY = p.SECRETKEY == null ? String.Empty : p.SECRETKEY,
//                                               ROUTE_TO_ENV = p.ROUTE_TO_ENV == null ? String.Empty : p.ROUTE_TO_ENV,
//                                               CLIENTID = p.CLIENTID == null ? String.Empty : p.CLIENTID,
//                                               CLIENTSECRET = p.CLIENTSECRET == null ? String.Empty : p.CLIENTSECRET,
//                                               GateWay = g.GATEWAY == null ? String.Empty : g.GATEWAY,
//                                               OPERATOR = m.OPERATOR == null ? String.Empty : m.OPERATOR
//                                           }).FirstOrDefault();
//            return config;
//        }
//        public IList<ProductConfigSearch> GetConfigFullVersionByClientCodeList(string CLIENTCODE)
//        {
//            IList<ProductConfigSearch>? config = (from m in _mapping.GetAll()
//                                                  join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                                  join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                                  where p.CLIENTCODE == CLIENTCODE
//                                                  select new ProductConfigSearch()
//                                                  {
//                                                      PRODUCTID = p.PRODUCTID == null ? String.Empty : p.PRODUCTID,
//                                                      CLIENTCODE = p.CLIENTCODE == null ? String.Empty : p.CLIENTCODE,
//                                                      TOKENUSERNAME = p.TOKENUSERNAME == null ? String.Empty : p.TOKENUSERNAME,
//                                                      UAT_SENDERID = p.UAT_SENDERID == null ? String.Empty : p.UAT_SENDERID,
//                                                      PRO_SENDERID = p.PRO_SENDERID == null ? String.Empty : p.PRO_SENDERID,
//                                                      DEPARTMENT = p.DEPARTMENT == null ? String.Empty : p.DEPARTMENT,
//                                                      DESCRIPTION = p.DESCRIPTION == null ? String.Empty : p.DESCRIPTION,
//                                                      REQUESTER = p.REQUESTER == null ? String.Empty : p.REQUESTER,
//                                                      REQUESTDATETIME = p.REQUESTDATETIME == null ? System.DateTime.Now : p.REQUESTDATETIME,
//                                                      TARGETDATETIME = p.TARGETDATETIME == null ? System.DateTime.Now : p.TARGETDATETIME,
//                                                      APPROVEDDATETIME = p.APPROVEDDATETIME == null ? System.DateTime.Now : p.APPROVEDDATETIME,
//                                                      APPROVEDBY = p.APPROVEDBY == null ? String.Empty : p.APPROVEDBY,
//                                                      CHECKDUPLICATE = p.CHECKDUPLICATE == null ? String.Empty : p.CHECKDUPLICATE,
//                                                      SECRETID = p.SECRETID == null ? String.Empty : p.SECRETID,
//                                                      SECRETKEY = p.SECRETKEY == null ? String.Empty : p.SECRETKEY,
//                                                      ROUTE_TO_ENV = p.ROUTE_TO_ENV == null ? String.Empty : p.ROUTE_TO_ENV,
//                                                      CLIENTID = p.CLIENTID == null ? String.Empty : p.CLIENTID,
//                                                      CLIENTSECRET = p.CLIENTSECRET == null ? String.Empty : p.CLIENTSECRET,
//                                                      GateWay = g.GATEWAY == null ? String.Empty : g.GATEWAY,
//                                                      OPERATOR = m.OPERATOR == null ? String.Empty : m.OPERATOR
//                                                  }).ToList();
//            return config;
//        }
//        public IList<ProductConfigSearch> GetConfigFullVersionFiltered(ProductFilter productFilter)
//        {
//            IList<ProductConfigSearch> config = (from m in _mapping.GetAll()
//                                                 join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                                 join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                                 where m.OPERATOR.ToUpper().Contains(productFilter.SearchText) ||
//                    p.CLIENTCODE.ToUpper().Contains(productFilter.SearchText) ||
//                    p.TOKENUSERNAME.ToUpper().Contains(productFilter.SearchText) ||
//                    p.UAT_SENDERID.ToUpper().Contains(productFilter.SearchText) ||
//                    p.PRO_SENDERID.ToUpper().Contains(productFilter.SearchText) ||
//                    p.DEPARTMENT.ToUpper().Contains(productFilter.SearchText) ||
//                    p.REQUESTER.ToUpper().Contains(productFilter.SearchText) ||
//                    p.CHECKDUPLICATE.ToUpper().Contains(productFilter.SearchText) ||
//                    g.GATEWAY.ToUpper().Contains(productFilter.SearchText) ||
//                    p.ROUTE_TO_ENV.ToUpper().Contains(productFilter.SearchText)
//                                                 select new ProductConfigSearch()
//                                                 {
//                                                     PRODUCTID = p.PRODUCTID == null ? String.Empty : p.PRODUCTID,
//                                                     CLIENTCODE = p.CLIENTCODE == null ? String.Empty : p.CLIENTCODE,
//                                                     TOKENUSERNAME = p.TOKENUSERNAME == null ? String.Empty : p.TOKENUSERNAME,
//                                                     UAT_SENDERID = p.UAT_SENDERID == null ? String.Empty : p.UAT_SENDERID,
//                                                     PRO_SENDERID = p.PRO_SENDERID == null ? String.Empty : p.PRO_SENDERID,
//                                                     DEPARTMENT = p.DEPARTMENT == null ? String.Empty : p.DEPARTMENT,
//                                                     DESCRIPTION = p.DESCRIPTION == null ? String.Empty : p.DESCRIPTION,
//                                                     REQUESTER = p.REQUESTER == null ? String.Empty : p.REQUESTER,
//                                                     REQUESTDATETIME = p.REQUESTDATETIME == null ? System.DateTime.Now : p.REQUESTDATETIME,
//                                                     TARGETDATETIME = p.TARGETDATETIME == null ? System.DateTime.Now : p.TARGETDATETIME,
//                                                     APPROVEDDATETIME = p.APPROVEDDATETIME == null ? System.DateTime.Now : p.APPROVEDDATETIME,
//                                                     APPROVEDBY = p.APPROVEDBY == null ? String.Empty : p.APPROVEDBY,
//                                                     CHECKDUPLICATE = p.CHECKDUPLICATE == null ? String.Empty : p.CHECKDUPLICATE,
//                                                     SECRETID = p.SECRETID == null ? String.Empty : p.SECRETID,
//                                                     SECRETKEY = p.SECRETKEY == null ? String.Empty : p.SECRETKEY,
//                                                     ROUTE_TO_ENV = p.ROUTE_TO_ENV == null ? String.Empty : p.ROUTE_TO_ENV,
//                                                     CLIENTID = p.CLIENTID == null ? String.Empty : p.CLIENTID,
//                                                     CLIENTSECRET = p.CLIENTSECRET == null ? String.Empty : p.CLIENTSECRET,
//                                                     GateWay = g.GATEWAY == null ? String.Empty : g.GATEWAY,
//                                                     OPERATOR = m.OPERATOR == null ? String.Empty : m.OPERATOR
//                                                 }).ToList();
//            return config;
//        }
//        public ListData GetFilterdeConfig(ProductFilter productFilter)
//        {
//            ListData listData = new ListData();
//            IList<ProductConfigSearch> Filtered;
//            productFilter.SearchText = productFilter.SearchText.ToUpper();
//            if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//            {
//                Filtered = GetConfigFullVersion();
//            }
//            else
//            {
//                Guid guidOutput = new Guid();
//                bool isValid = Guid.TryParse(productFilter.SearchText, out guidOutput);
//                if (isValid)
//                {
//                    Filtered = GetConfigFullVersion().Where(x =>
//                x.PRODUCTID.Equals(guidOutput.ToString())
//                ).ToList();
//                }
//                else
//                {
//                    //Filtered = GetAll.Where(x =>
//                    //(x.OPERATOR == null ? "": x.OPERATOR.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.CLIENTCODE == null ? "" : x.CLIENTCODE.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.TOKENUSERNAME == null ? "" : x.TOKENUSERNAME.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.UAT_SENDERID == null ? "" : x.UAT_SENDERID.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.PRO_SENDERID == null ? "" : x.PRO_SENDERID.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.DEPARTMENT == null ? "" : x.DEPARTMENT.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.REQUESTER == null ? "" : x.REQUESTER.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.CHECKDUPLICATE == null ? "" : x.CHECKDUPLICATE.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.GateWay == null ? "" : x.GateWay.ToUpper()).Contains(productFilter.SearchText) ||
//                    //(x.ROUTE_TO_ENV == null ? "" : x.ROUTE_TO_ENV.ToUpper()).Contains(productFilter.SearchText)
//                    //).ToList();

//                    Filtered = GetConfigFullVersionFiltered(productFilter);

//                }
//            }
//            int ProductCount = Filtered.Count();
//            listData.Products = Filtered.Skip(productFilter.Pagination.NEXT_INDEX * productFilter.Pagination.PAGE_TOTAL).Take(productFilter.Pagination.PAGE_TOTAL).ToList();
//            listData.ProductCount = ProductCount;
//            return listData;
//        }

//        public List<ListDataByClientCodeOneProduct> GetFilterdeConfigByClientCodeWithPage(Pagination pagination)
//        {
//            var all_data = GetConfigFullVersion().GroupBy(x => x.CLIENTCODE).Select(x1 => new ListDataByClientCode { ClientCode = x1.Key, Product = x1.Select(y => y).DistinctBy(y1 => new { y1.CLIENTCODE, y1.TOKENUSERNAME, y1.GateWay }).ToList() });
//            int ProductCount = all_data.Count();
//            List<ListDataByClientCode> getall = all_data.Skip(pagination.NEXT_INDEX * pagination.PAGE_TOTAL).Take(pagination.PAGE_TOTAL).ToList();
//            List<ListDataByClientCodeOneProduct> ClientCodeProductReturn = new List<ListDataByClientCodeOneProduct>();
//            foreach (var item in getall)
//            {
//                ListDataByClientCodeOneProduct listDataByClientCodeOneProduct = new ListDataByClientCodeOneProduct();
//                listDataByClientCodeOneProduct.Product = new ProductConfigSearch();
//                listDataByClientCodeOneProduct.ClientCode = item.ClientCode;
//                listDataByClientCodeOneProduct.Product = item.Product[0];
//                listDataByClientCodeOneProduct.ProductCount = ProductCount;
//                List<string> Gateway = new List<string>();
//                List<string> TokenUserName = new List<string>();
//                List<string> PROD_Sender_ID = new List<string>();
//                List<string> UAT_Sender_ID = new List<string>();
//                foreach (var item1 in item.Product)
//                {
//                    if (!Gateway.Contains(item1.GateWay))
//                    {
//                        Gateway.Add(item1.GateWay);
//                    }
//                    if (!TokenUserName.Contains(item1.TOKENUSERNAME))
//                    {
//                        TokenUserName.Add(item1.TOKENUSERNAME);
//                    }
//                    if (!PROD_Sender_ID.Contains(item1.PRO_SENDERID))
//                    {
//                        PROD_Sender_ID.Add(item1.PRO_SENDERID);
//                    }
//                    if (!UAT_Sender_ID.Contains(item1.UAT_SENDERID))
//                    {
//                        UAT_Sender_ID.Add(item1.UAT_SENDERID);
//                    }
//                }
//                listDataByClientCodeOneProduct.Product.TOKENUSERNAME = string.Join(",", TokenUserName);
//                listDataByClientCodeOneProduct.Product.GateWay = string.Join(",", Gateway);
//                listDataByClientCodeOneProduct.Product.UAT_SENDERID = string.Join(",", UAT_Sender_ID);
//                listDataByClientCodeOneProduct.Product.PRO_SENDERID = string.Join(",", PROD_Sender_ID);
//                ClientCodeProductReturn.Add(listDataByClientCodeOneProduct);
//            }
//            return ClientCodeProductReturn;
//        }

//        public List<ListDataByClientCodeOneProduct> GetFilterdeConfigByClientCode()
//        {

//            List<ListDataByClientCode> getall = GetConfigFullVersion().GroupBy(x => x.CLIENTCODE).Select(x1 => new ListDataByClientCode { ClientCode = x1.Key, Product = x1.Select(y => y).DistinctBy(y1 => new { y1.CLIENTCODE, y1.TOKENUSERNAME, y1.GateWay }).ToList() }).ToList();
//            List<ListDataByClientCodeOneProduct> ClientCodeProductReturn = new List<ListDataByClientCodeOneProduct>();
//            foreach (var item in getall)
//            {
//                ListDataByClientCodeOneProduct listDataByClientCodeOneProduct = new ListDataByClientCodeOneProduct();
//                listDataByClientCodeOneProduct.Product = new ProductConfigSearch();
//                listDataByClientCodeOneProduct.ClientCode = item.ClientCode;
//                listDataByClientCodeOneProduct.Product = item.Product[0];

//                List<string> Gateway = new List<string>();
//                List<string> TokenUserName = new List<string>();
//                foreach (var item1 in item.Product)
//                {
//                    if (!Gateway.Contains(item1.GateWay))
//                    {
//                        Gateway.Add(item1.GateWay);
//                    }
//                    if (!TokenUserName.Contains(item1.TOKENUSERNAME))
//                    {
//                        TokenUserName.Add(item1.TOKENUSERNAME);
//                    }
//                }
//                listDataByClientCodeOneProduct.Product.TOKENUSERNAME = string.Join(",", TokenUserName);
//                listDataByClientCodeOneProduct.Product.GateWay = string.Join(",", Gateway);
//                ClientCodeProductReturn.Add(listDataByClientCodeOneProduct);
//            }
//            return ClientCodeProductReturn.OrderBy(x => x.ClientCode).ToList();
//        }
//        public IList<ConfigModel> GetConfigByTelco(string TelcoCode)
//        {
//            IList<ConfigModel> config = (from m in _mapping.GetAll()
//                                         join p in _product.GetAll() on m.PRODUCTID equals p.PRODUCTID
//                                         join g in _gateway.GetAll() on m.GATEWAYID equals g.GATEWAYID
//                                         where m.OPERATOR.Equals(TelcoCode)
//                                         select new ConfigModel()
//                                         {
//                                             ProductID = p.PRODUCTID,
//                                             ClientCode = p.CLIENTCODE,
//                                             CheckDuplicate = p.CHECKDUPLICATE,
//                                             TokenUserName = p.TOKENUSERNAME,
//                                             UAT_SenderID = p.UAT_SENDERID,
//                                             Pro_SenderID = p.PRO_SENDERID,
//                                             Requester = p.REQUESTER,
//                                             RequestDateTime = p.REQUESTDATETIME,
//                                             TargetDateTime = p.TARGETDATETIME,
//                                             ApprovedDateTime = p.APPROVEDDATETIME,
//                                             ApprovedBy = p.APPROVEDBY,
//                                             Department = p.DEPARTMENT,
//                                             Description = p.DESCRIPTION,
//                                             Gateway = g.GATEWAY,
//                                             ServiceURL1 = g.SERVICEURL1,
//                                             ServiceURL2 = g.SERVICEURL2,
//                                             ServiceURL3 = g.SERVICEURL3,
//                                             SecretID = p.SECRETID,
//                                             SecretKey = p.SECRETKEY,
//                                             Route_to_env = p.ROUTE_TO_ENV,
//                                             clientid = p.CLIENTID,
//                                             clientsecret = p.CLIENTSECRET
//                                         }).ToList();
//            return config;
//        }
//        public List<Product> GetConfigByClientCode(string clientCode)
//        {
//            List<Product> getall = _product.Query(y => y.CLIENTCODE.Equals(clientCode)).ToList();
//            return getall;
//        }

//        public List<ProductConfigSearch> GetConfigByProductID(ProductFilterByProductID productFilter)
//        {
//            List<ProductConfigSearch> listData = new List<ProductConfigSearch>();
//            List<ProductConfigSearch> Filtered = new List<ProductConfigSearch>();
//            Filtered = GetConfigFullVersion().Where(x => x.PRODUCTID.Equals(productFilter.PRODUCTID) && x.OPERATOR.Equals(productFilter.OPERATOR)).ToList();

//            return Filtered;
//        }

//        public IList<PhonePrefixes> GetPhonePrefixes()
//        {
//            IList<PhonePrefixes> config = (from p in _phonePrefix.GetAll()
//                                           select new PhonePrefixes()
//                                           {
//                                               KEY = p.KEY,
//                                               VALUE = p.VALUE,
//                                           }).ToList();
//            return config;
//        }
//        public Product GetProductByClientCode(string ClientCode, string TokenUserName)
//        {
//            return _product.Query(x => x.CLIENTCODE.Equals(ClientCode) && x.TOKENUSERNAME.Equals(TokenUserName)).FirstOrDefault();
//        }
//        public List<Gateway> GetGateWay()
//        {
//            return _gateway.GetAll().ToList();
//        }
//        public List<MAPPING> GetGateWayMapping()
//        {
//            return _mapping.GetAll().ToList();
//        }
//        public Task<ResponseModel> InsertProduct(ProductConfig productEntity, string RefNo, out HttpStatusCode statusCode)
//        {
//            _logger.LogInformation("Product Insert Start");
//            ResponseModel resp = new ResponseModel();
//            Product entity = JsonConvert.DeserializeObject<Product>(JsonConvert.SerializeObject(productEntity));
//            entity.PRODUCTID = Guid.NewGuid().ToString();
//            resp.RefNo = RefNo;
//            try
//            {
//                if (_product.isExist(entity.CLIENTCODE, entity.TOKENUSERNAME))
//                {
//                    resp.Error = ErrorCode.Duplicate;
//                    statusCode = HttpStatusCode.BadRequest;
//                    return Task.FromResult(resp);
//                }
//                entity.REQUESTDATETIME = DateTime.Now;
//                _product.Insert(entity);
//                IList<PhonePrefixes> phonePrefixes = GetPhonePrefixes();
//                Gateway gate = GetGateWay().Where(x => x.GATEWAY.Equals(productEntity.GateWay)).FirstOrDefault();
//                if (gate == null)
//                {
//                    resp.Error = ErrorCode.GateNotExist;
//                    statusCode = HttpStatusCode.InternalServerError;
//                    return Task.FromResult(resp);
//                }
//                List<MAPPING> gateWayMapList = new List<MAPPING>();
//                foreach (var phonePrefix in phonePrefixes)
//                {
//                    MAPPING gateWayMap = new MAPPING();
//                    gateWayMap.MAPPINGID = Guid.NewGuid().ToString();
//                    gateWayMap.OPERATOR = phonePrefix.KEY;
//                    gateWayMap.GATEWAYID = gate.GATEWAYID;
//                    gateWayMap.CREATEDON = System.DateTime.Now;
//                    gateWayMap.PRODUCTID = entity.PRODUCTID;
//                    gateWayMapList.Add(gateWayMap);
//                    //Created By
//                    //gateWayMap.CREATEDBY

//                }
//                _mapping.InsertRange(gateWayMapList);

//                //_ctx.SaveChanges();

//                if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;

//                    SMS_CONFIG_RECORDS sms_config_records = new SMS_CONFIG_RECORDS();
//                    sms_config_records.CLIENTCODE = entity.CLIENTCODE;
//                    sms_config_records.EMPLOYEE_ID = productEntity.EMPLOYEE_ID;
//                    sms_config_records.EMPLOYEE_NAME = productEntity.EMPLOYEE_NAME;
//                    sms_config_records.RECORD_DATE = System.DateTime.Now;
//                    sms_config_records.GATWEWAY = productEntity.GateWay;
//                    sms_config_records.REQ_CONFIG = JsonConvert.SerializeObject(productEntity);
//                    sms_config_records.RECORD_TYPE = "Create";
//                    sms_config_records.TELCO_CODE = productEntity.OPERATOR;
//                    sms_config_records.STATUS = "Success";
//                    sms_config_records.DEPENDENCY_TYPE = productEntity.Filter.CreateDataWith;
//                    InsertConfigRecord(sms_config_records, RefNo, entity.PRODUCTID);
//                }

//                else
//                {
//                    resp.Error = ErrorCode.NoRowsAffected;
//                    statusCode = HttpStatusCode.InternalServerError;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Product Insert Exception");
//                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
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
//        //public Task<ResponseModel> InsertProductGatewayMapping(Product entity, string RefNo, out HttpStatusCode statusCode)
//        //{
//        //    _logger.LogInformation("Product Insert Start");
//        //    ResponseModel resp = new ResponseModel();
//        //    resp.RefNo = RefNo;
//        //    try
//        //    {
//        //        entity.PRODUCTID = Guid.NewGuid().ToString();


//        //        if (_product.isExist(entity.CLIENTCODE))
//        //        {
//        //            resp.Error = ErrorCode.Duplicate;
//        //            statusCode = HttpStatusCode.BadRequest;
//        //            return Task.FromResult(resp);
//        //        }

//        //        _product.Insert(entity);
//        //        if (Task.FromResult(_product.Commit()).Result > 0)
//        //        {
//        //            resp.Data = new { result = "success" };
//        //            statusCode = HttpStatusCode.OK;
//        //        }
//        //        else
//        //        {
//        //            resp.Error = ErrorCode.NoRowsAffected;
//        //            statusCode = HttpStatusCode.InternalServerError;
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError("Product Insert Exception");
//        //        _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//        //        _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());

//        //        resp.Error = ErrorCode.UnknownException;
//        //        resp.Error.Details.Add(ErrorCode.OperationError);
//        //        statusCode = HttpStatusCode.InternalServerError;
//        //    }
//        //    return Task.FromResult(resp);
//        //}

//        public Task<ResponseModel> UpdateProduct(ProductUpdateConfig entityProductConfig, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            try
//            {
//                Product entity = GetProductByClientCode(entityProductConfig.CLIENTCODE, entityProductConfig.TOKENUSERNAME);
//                IList<ProductConfigSearch> productConfigSearch = GetConfigFullVersionByProductID(entity.PRODUCTID);
//                if (entity == null)
//                {
//                    resp.Error = ErrorCode.ProductNotExist;
//                    statusCode = HttpStatusCode.InternalServerError;
//                    return Task.FromResult(resp);
//                }
//                entityProductConfig.PRODUCTID = entity.PRODUCTID;
//                entity = JsonConvert.DeserializeObject<Product>(JsonConvert.SerializeObject(entityProductConfig));
//                entity.APPROVEDDATETIME = DateTime.Now;
//                entity.APPROVEDBY = entity.REQUESTER;
//                resp.RefNo = RefNo;

//                //_product.UpdatePartial(entity, o => o.CLIENTCODE, o => o.TOKENUSERNAME, o => o.UAT_SENDERID, o => o.PRO_SENDERID, o => o.DEPARTMENT, o => o.DESCRIPTION, o => o.REQUESTER, o => o.REQUESTDATETIME, o => o.TARGETDATETIME, o => o.APPROVEDDATETIME, o => o.APPROVEDBY, o => o.CHECKDUPLICATE, o => o.SECRETID, o => o.SECRETKEY, o => o.ROUTE_TO_ENV);
//                _product.Update(entity);
//                _logger.LogInformation("Product Update");
//                List<MAPPING> gateMappingsUpdate = new List<MAPPING>();
//                if (entityProductConfig.GateWay != null && entityProductConfig.OPERATOR != null)
//                {
//                    Gateway gate = GetGateWay().Where(x => x.GATEWAY.Equals(entityProductConfig.GateWay)).FirstOrDefault();
//                    if (gate == null)
//                    {
//                        resp.Error = ErrorCode.GateNotExist;
//                        statusCode = HttpStatusCode.InternalServerError;
//                        return Task.FromResult(resp);
//                    }
//                    IList<MAPPING> gateMappings = GetGateWayMapping();
//                    gateMappings.Where(x => x.PRODUCTID.Equals(entity.PRODUCTID) && x.OPERATOR.Equals(entityProductConfig.OPERATOR)).FirstOrDefault().GATEWAYID = gate.GATEWAYID;

//                    _mapping.Update(gateMappings.Where(x => x.PRODUCTID.Equals(entity.PRODUCTID) && x.OPERATOR.Equals(entityProductConfig.OPERATOR)).FirstOrDefault());

//                    _logger.LogInformation("Specific Provider Gateway Product Mapping Update");
//                }
//                else if (entityProductConfig.GateWay != null && entityProductConfig.OPERATOR == null)
//                {
//                    Gateway gate = GetGateWay().Where(x => x.GATEWAY.Equals(entityProductConfig.GateWay)).FirstOrDefault();
//                    if (gate == null)
//                    {
//                        resp.Error = ErrorCode.GateNotExist;
//                        statusCode = HttpStatusCode.InternalServerError;
//                        return Task.FromResult(resp);
//                    }
//                    gateMappingsUpdate = GetGateWayMapping();
//                    gateMappingsUpdate.Where(x => x.PRODUCTID.Equals(entity.PRODUCTID)).ToList().ForEach(c => c.GATEWAYID = gate.GATEWAYID);

//                    _mapping.UpdateRange(gateMappingsUpdate);

//                    _logger.LogInformation("All Gateway Product Mapping Update");
//                }

//                if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;

//                    SMS_CONFIG_RECORDS sms_config_records = new SMS_CONFIG_RECORDS();
//                    sms_config_records.CLIENTCODE = entity.CLIENTCODE;
//                    sms_config_records.EMPLOYEE_ID = entityProductConfig.EMPLOYEE_ID;
//                    sms_config_records.EMPLOYEE_NAME = entityProductConfig.EMPLOYEE_NAME;
//                    sms_config_records.RECORD_DATE = System.DateTime.Now;
//                    sms_config_records.GATWEWAY = entityProductConfig.GateWay;
//                    sms_config_records.REQ_CONFIG = JsonConvert.SerializeObject(entityProductConfig);
//                    sms_config_records.RECORD_TYPE = "Update";
//                    sms_config_records.TELCO_CODE = entityProductConfig.OPERATOR;
//                    sms_config_records.STATUS = "Success";
//                    sms_config_records.DEPENDENCY_TYPE = entityProductConfig.Filter.CreateDataWith;
//                    InsertConfigRecord(sms_config_records, RefNo, entity.PRODUCTID);
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

//        public Task<ResponseModel> UpdateProductByTelco(ProductUpdateConfigByTelco entityProductConfig, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            statusCode = HttpStatusCode.Processing;
//            _logger.LogInformation("Update Data by Telco");
//            try
//            {
//                List<MAPPING> gateMappingsUpdate = GetGateWayMapping();
//                Gateway Gateway = _gateway.Query(x => x.GATEWAY.Equals(entityProductConfig.GATEWAY)).FirstOrDefault();
//                if (Gateway != null)
//                {
//                    gateMappingsUpdate.Where(x => x.OPERATOR.Equals(entityProductConfig.TELCOCODE)).ToList().ForEach(c => c.GATEWAYID = Gateway.GATEWAYID);

//                    _mapping.UpdateRange(gateMappingsUpdate);
//                    _logger.LogInformation("All Gateway Product Mapping Update");
//                    if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
//                    {
//                        resp.Data = new { result = "success" };
//                        statusCode = HttpStatusCode.OK;

//                        SMS_CONFIG_RECORDS sms_config_records = new SMS_CONFIG_RECORDS();
//                        sms_config_records.EMPLOYEE_ID = entityProductConfig.EMPLOYEE_ID;
//                        sms_config_records.EMPLOYEE_NAME = entityProductConfig.EMPLOYEE_NAME;
//                        sms_config_records.RECORD_DATE = System.DateTime.Now;
//                        sms_config_records.GATWEWAY = entityProductConfig.GATEWAY;
//                        sms_config_records.REQ_CONFIG = JsonConvert.SerializeObject(entityProductConfig);
//                        sms_config_records.RECORD_TYPE = "Swing By Telco";
//                        sms_config_records.STATUS = "Success";
//                        sms_config_records.TELCO_CODE = entityProductConfig.TELCOCODE;
//                        sms_config_records.DEPENDENCY_TYPE = entityProductConfig.Filter.CreateDataWith;
//                        InsertConfigRecord(sms_config_records, RefNo, null);
//                    }
//                }
//                else
//                {
//                    resp.Error = ErrorCode.GateNotExist;
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

//        public Task<ResponseModel> DeleteProduct(ProductDelete entityProductConfig, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            resp.RefNo = RefNo;
//            try
//            {
//                Product entity = GetProductByClientCode(entityProductConfig.CLIENTCODE, entityProductConfig.TOKENUSERNAME);
//                if (entity == null)
//                {
//                    resp.Error = ErrorCode.ProductNotExist;
//                    statusCode = HttpStatusCode.InternalServerError;
//                    return Task.FromResult(resp);
//                }

//                _product.Delete(entity);
//                List<MAPPING> deleteGateWayMapping = GetGateWayMapping().Where(x => x.PRODUCTID.Equals(entity.PRODUCTID)).ToList();
//                _mapping.DeleteRange(deleteGateWayMapping);
//                if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
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

//        public void InsertConfigRecord(SMS_CONFIG_RECORDS configEntity, string RefNo, string productID)
//        {
//            _logger.LogInformation("Config Insert Start");
//            ResponseModel resp = new ResponseModel();
//            SMS_CONFIG_RECORDS entity = JsonConvert.DeserializeObject<SMS_CONFIG_RECORDS>(JsonConvert.SerializeObject(configEntity));
//            entity.CONFIG_RECORD_ID = Guid.NewGuid().ToString();
//            entity.RECORD_DATE = System.DateTime.Now;
//            //_configRecord
//            resp.RefNo = RefNo;
//            try
//            {
//                _configRecord.Insert(entity);
//                _configRecord.Commit();
//                _logger.LogInformation("Config Insert Success");
//                _logger.LogInformation("Config : " + entity);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError("Config Insert Exception");
//                _logger.LogError("RefNo: " + RefNo + ", Exception: " + ex.ToString());
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("RefNo: " + RefNo + ", Inner Exception: " + ex.InnerException.ToString());
//                }
//            }
//            _logger.LogInformation("Config Insert End");
//        }

//        public Task<ResponseModel> SwingByClientCode(ClientCodeList clientCodeList, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            IList<ProductConfigSearch> entityProductConfigs = new List<ProductConfigSearch>();
//            try
//            {
//                foreach (var clientCode in clientCodeList.CLIENTCODES)
//                {
//                    var test = GetConfigFullVersionByClientCodeList(clientCode).DistinctBy(x => x.TOKENUSERNAME).ToList();
//                    foreach (var item in test)
//                    {
//                        entityProductConfigs.Add(item);
//                    }
//                }

//                foreach (var entityProductConfig in entityProductConfigs)
//                {
//                    if (entityProductConfig == null)
//                    {
//                        resp.Error = ErrorCode.ProductNotExist;
//                        statusCode = HttpStatusCode.InternalServerError;
//                        return Task.FromResult(resp);
//                    }
//                }
//                foreach (var entityProductConfig in entityProductConfigs)
//                {
//                    #region Update Start

//                    Product entity = GetProductByClientCode(entityProductConfig.CLIENTCODE, entityProductConfig.TOKENUSERNAME);
//                    IList<ProductConfigSearch> productConfigSearch = GetConfigFullVersionByProductID(entity.PRODUCTID);
//                    if (entity == null)
//                    {
//                        resp.Error = ErrorCode.ProductNotExist;
//                        statusCode = HttpStatusCode.InternalServerError;
//                        return Task.FromResult(resp);
//                    }
//                    entityProductConfig.PRODUCTID = entity.PRODUCTID;
//                    entity = JsonConvert.DeserializeObject<Product>(JsonConvert.SerializeObject(entityProductConfig));
//                    entity.APPROVEDDATETIME = DateTime.Now;
//                    entity.APPROVEDBY = entity.REQUESTER;
//                    resp.RefNo = RefNo;

//                    //_product.UpdatePartial(entity, o => o.CLIENTCODE, o => o.TOKENUSERNAME, o => o.UAT_SENDERID, o => o.PRO_SENDERID, o => o.DEPARTMENT, o => o.DESCRIPTION, o => o.REQUESTER, o => o.REQUESTDATETIME, o => o.TARGETDATETIME, o => o.APPROVEDDATETIME, o => o.APPROVEDBY, o => o.CHECKDUPLICATE, o => o.SECRETID, o => o.SECRETKEY, o => o.ROUTE_TO_ENV);
//                    _product.Update(entity);
//                    _logger.LogInformation("Product Update");
//                    //List<MAPPING> gateMappingsUpdate = new List<MAPPING>();

//                    Gateway gate = GetGateWay().Where(x => x.GATEWAY.Equals(clientCodeList.GATEWAY)).FirstOrDefault();
//                    if (gate == null)
//                    {
//                        resp.Error = ErrorCode.GateNotExist;
//                        statusCode = HttpStatusCode.InternalServerError;
//                        return Task.FromResult(resp);
//                    }
//                    //gateMappingsUpdate = GetGateWayMapping();
//                    //gateMappingsUpdate.Where(x => x.PRODUCTID.Equals(entity.PRODUCTID)).ToList().ForEach(c => c.GATEWAYID = gate.GATEWAYID);
//                    List<MAPPING> gateMappingsUpdatePartial = new();

//                    gateMappingsUpdatePartial = GetGateWayMapping().Where(x => x.PRODUCTID.Equals(entity.PRODUCTID)).ToList();
//                    if (clientCodeList.TELCOLIST != null)
//                    {
//                        if (clientCodeList.TELCOLIST.Count > 0)
//                        {
//                            gateMappingsUpdatePartial.ForEach(c =>
//                            {
//                                if (clientCodeList.TELCOLIST.Contains(c.OPERATOR))
//                                {
//                                    c.GATEWAYID = gate.GATEWAYID;
//                                }
//                            }
//                            );
//                        }
//                        else
//                        {
//                            gateMappingsUpdatePartial.ForEach(c => c.GATEWAYID = gate.GATEWAYID);
//                        }
//                    }
//                    else
//                    {
//                        gateMappingsUpdatePartial.ForEach(c => c.GATEWAYID = gate.GATEWAYID);
//                    }
//                    //gateMappingsUpdatePartial.ForEach(c => c.GATEWAYID = gate.GATEWAYID);
//                    _mapping.UpdateRange(gateMappingsUpdatePartial);

//                    _logger.LogInformation("All Gateway Product Mapping Update");

//                    #endregion
//                }
//                if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;

//                    SMS_CONFIG_RECORDS sms_config_records = new SMS_CONFIG_RECORDS();
//                    sms_config_records.CLIENTCODE = string.Join(",", clientCodeList.CLIENTCODES);
//                    sms_config_records.EMPLOYEE_ID = clientCodeList.EMPLOYEE_ID;
//                    sms_config_records.EMPLOYEE_NAME = clientCodeList.EMPLOYEE_NAME;
//                    sms_config_records.RECORD_DATE = System.DateTime.Now;
//                    sms_config_records.GATWEWAY = clientCodeList.GATEWAY;
//                    sms_config_records.REQ_CONFIG = JsonConvert.SerializeObject(clientCodeList);
//                    sms_config_records.RECORD_TYPE = "Swing By Client Codes with Telcos";
//                    sms_config_records.TELCO_CODE = "All";
//                    sms_config_records.STATUS = "Success";
//                    sms_config_records.DEPENDENCY_TYPE = clientCodeList.Filter.CreateDataWith;
//                    InsertConfigRecord(sms_config_records, RefNo, null);
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

//        public Task<ResponseModel> UpdateProductInfo(ProductInfoUpdateConfig entityProductConfig, string RefNo, out HttpStatusCode statusCode)
//        {
//            ResponseModel resp = new ResponseModel();
//            try
//            {
//                Product entity = JsonConvert.DeserializeObject<Product>(JsonConvert.SerializeObject(entityProductConfig));
//                //IList<ProductConfigSearch> entity = GetConfigFullVersionByProductID(entityProductConfig.PRODUCTID);
//                if (entity == null)
//                {
//                    resp.Error = ErrorCode.ProductNotExist;
//                    statusCode = HttpStatusCode.InternalServerError;
//                    return Task.FromResult(resp);
//                }
//                //entityProductConfig.PRODUCTID = entity.PRODUCTID;
//                //entity = JsonConvert.DeserializeObject<Product>(JsonConvert.SerializeObject(entityProductConfig));
//                //entity.APPROVEDDATETIME = DateTime.Now;
//                //entity.APPROVEDBY = entity.REQUESTER;
//                resp.RefNo = RefNo;

//                //_product.UpdatePartial(entity, o => o.CLIENTCODE, o => o.TOKENUSERNAME, o => o.UAT_SENDERID, o => o.PRO_SENDERID, o => o.DEPARTMENT, o => o.DESCRIPTION, o => o.REQUESTER, o => o.REQUESTDATETIME, o => o.TARGETDATETIME, o => o.APPROVEDDATETIME, o => o.APPROVEDBY, o => o.CHECKDUPLICATE, o => o.SECRETID, o => o.SECRETKEY, o => o.ROUTE_TO_ENV);
//                _product.Update(entity);
//                _logger.LogInformation("Product Update");

//                if (Task.FromResult(_ctx.SaveChanges()).Result > 0)
//                {
//                    resp.Data = new { result = "success" };
//                    statusCode = HttpStatusCode.OK;

//                    SMS_CONFIG_RECORDS sms_config_records = new SMS_CONFIG_RECORDS();
//                    sms_config_records.CLIENTCODE = entity.CLIENTCODE;
//                    sms_config_records.EMPLOYEE_ID = entityProductConfig.EMPLOYEE_ID;
//                    sms_config_records.EMPLOYEE_NAME = entityProductConfig.EMPLOYEE_NAME;
//                    sms_config_records.RECORD_DATE = System.DateTime.Now;
//                    sms_config_records.GATWEWAY = null;
//                    sms_config_records.REQ_CONFIG = JsonConvert.SerializeObject(entityProductConfig);
//                    sms_config_records.RECORD_TYPE = "Update Product Info";
//                    sms_config_records.TELCO_CODE = null;
//                    sms_config_records.STATUS = "Success";
//                    sms_config_records.DEPENDENCY_TYPE = entityProductConfig.Filter.CreateDataWith;
//                    InsertConfigRecord(sms_config_records, RefNo, entity.PRODUCTID);
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

//        public ListSMSRecordData GetSMSConfigRecords(ProductFilter productFilter)
//        {
//            ListSMSRecordData listData = new ListSMSRecordData();
//            IList<SMS_CONFIG_RECORDS> Filtered = new List<SMS_CONFIG_RECORDS>();

//            if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//            {
//                Filtered = _configRecord.GetAll().OrderByDescending(x => x.RECORD_DATE).Skip(productFilter.Pagination.NEXT_INDEX * productFilter.Pagination.PAGE_TOTAL).Take(productFilter.Pagination.PAGE_TOTAL).ToList();
//            }
//            else
//            {
//                Filtered = _configRecord.GetAll().OrderByDescending(x => x.RECORD_DATE).Where(CreateTextSearch<SMS_CONFIG_RECORDS>(productFilter.SearchText)).OrderByDescending(y => y.RECORD_DATE).Skip(productFilter.Pagination.NEXT_INDEX * productFilter.Pagination.PAGE_TOTAL).Take(productFilter.Pagination.PAGE_TOTAL).ToList();
//            }
//            int ProductCount = _configRecord.GetAll().Count();
//            listData.Records = Filtered.ToList();
//            listData.ProductCount = ProductCount;
//            return listData;
//        }
//        public ListTransactionRecordData GetTrans(TransactionFilter productFilter)
//        {
//            ListTransactionRecordData listData = new ListTransactionRecordData();
//            IList<Transaction> Filtered = new List<Transaction>();
//            DateTime NintyDaysAgo = System.DateTime.Now.AddDays(-91);
//            //int ProductCount = 0;
//            if (productFilter.Date != null)
//            {
//                DateTime searchDate = Convert.ToDateTime(productFilter.Date);
//                //.Cast<Transaction>().ToList();
//                if (searchDate != null)
//                {
//                    if (searchDate.Date.Equals(System.DateTime.Now.Date))
//                    {
//                        if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//                        {
//                            Filtered = _trans.Query(x => x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).ToList();
//                        }
//                        else
//                        {
//                            Filtered = _trans.Query(x => x.CLIENT_CODE.Equals(productFilter.SearchText) && x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).OrderByDescending(y => y.REQUEST_DATETIME).ToList();
//                        }
//                    }
//                    else if (searchDate.Date > NintyDaysAgo.Date && searchDate.Date < System.DateTime.Now.Date)
//                    {
//                        if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//                        {

//                            Filtered = _mapper.Map<List<Transaction>>(_transTemp.Query(x => x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).ToList());
//                        }
//                        else
//                        {

//                            Filtered = _mapper.Map<List<Transaction>>(_transTemp.Query(x => x.CLIENT_CODE.Equals(productFilter.SearchText) && x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).OrderByDescending(y => y.REQUEST_DATETIME).ToList());
//                        }
//                    }
//                    else
//                    {
//                        if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//                        {
//                            Filtered = _mapper.Map<IList<Transaction>>(_transHistory.Query(x => x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).ToList());
//                        }
//                        else
//                        {
//                            Filtered = _mapper.Map<IList<Transaction>>(_transHistory.Query(x => x.CLIENT_CODE.Equals(productFilter.SearchText) && x.REQUEST_DATETIME.Value.Date.Equals(searchDate.Date)).OrderByDescending(y => y.REQUEST_DATETIME).ToList());
//                        }
//                    }
//                }
//            }
//            else
//            {
//                if (String.IsNullOrEmpty(productFilter.SearchText) || String.IsNullOrWhiteSpace(productFilter.SearchText) || productFilter.SearchText == null)
//                {
//                    Filtered = _trans.GetAll().ToList();
//                }
//                else
//                {
//                    Filtered = _trans.Query(x => x.CLIENT_CODE.Equals(productFilter.SearchText)).OrderByDescending(y => y.REQUEST_DATETIME).ToList();
//                }

//            }

//            int ProductCount = Filtered.Count();
//            listData.Records = Filtered.OrderByDescending(x => x.REQUEST_DATETIME).Skip(productFilter.Pagination.NEXT_INDEX * productFilter.Pagination.PAGE_TOTAL).Take(productFilter.Pagination.PAGE_TOTAL).ToList();
//            listData.ProductCount = ProductCount;
//            return listData;
//        }
//        public static Expression<Func<T, bool>> CreateTextSearch<T>(string searchText)
//        {
//            Type t = typeof(T);
//            var props = t.GetProperties().Cast<PropertyInfo>().Where(p => p.PropertyType == typeof(string));

//            var searchTextExpr = Expression.Constant(searchText);
//            var tParameterExpr = Expression.Parameter(typeof(T));

//            Expression expr = null;
//            foreach (var prop in props)
//            {
//                var criteria = Expression.Call(
//                    Expression.Property(tParameterExpr, prop),
//                    typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
//                    searchTextExpr);
//                if (expr == null)
//                    expr = criteria;
//                else
//                    expr = Expression.Or(expr, criteria);
//            }
//            return Expression.Lambda<Func<T, bool>>(expr, tParameterExpr);
//        }

//        public IList<SMSMonthlyCountResponse> GetMonthlySMSCount(SMSMonthlyCountRequest smsCount)
//        {
//            DateTime startDate = smsCount.StartDate;
//            DateTime endDate = smsCount.EndDate;
//            var tran = _trans.Query(x => (x.REQUEST_DATETIME <= endDate && x.REQUEST_DATETIME >= startDate)).Select(x => new { x.CLIENT_CODE, x.MERCHANT_ID });
//            var trantemp = _transTemp.Query(x => (x.REQUEST_DATETIME <= endDate && x.REQUEST_DATETIME >= startDate)).Select(x => new { x.CLIENT_CODE, x.MERCHANT_ID });
//            var tranhistpry = _transHistory.Query(x => (x.REQUEST_DATETIME <= endDate && x.REQUEST_DATETIME >= startDate)).Select(x => new { x.CLIENT_CODE, x.MERCHANT_ID });
//            List<SMSMonthlyCountResponse> test = tran.Concat(trantemp).Concat(tranhistpry).OrderBy(c => c.CLIENT_CODE)
//                        .GroupBy(y => new { y.CLIENT_CODE, y.MERCHANT_ID })
//                        .Select(group => new Gates
//                        {
//                            ClientCode = group.Key.CLIENT_CODE,
//                            Gateway = group.Key.MERCHANT_ID,
//                            Count = group.Count()
//                        }).AsEnumerable<Gates>().GroupBy(x => x.ClientCode)
//                        .Select(gp => new SMSMonthlyCountResponse
//                        {
//                            ClientCode = gp.Key,
//                            Gateways = gp.ToList()
//                        }
//                        ).ToList();

//            return test;
//        }
//    }
//}
