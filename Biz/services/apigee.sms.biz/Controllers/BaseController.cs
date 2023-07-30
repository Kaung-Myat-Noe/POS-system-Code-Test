using apigee.sms.biz.Models;
using apigee.sms.biz.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace apigee.sms.biz.Controllers
{

    public class BaseController : ControllerBase
    {
        public static string? KBZRefNo { get; set; }
        public static string? KBZ_REF_NO { get; set; }
        public static string? scheme { get; set; }
        public static string? parameter { get; set; }
        public static string? claimUser { get; set; }
        internal static HttpClient client = new HttpClient();
        internal static CancellationTokenSource cts = new CancellationTokenSource();
        static ConfigModel c;
        static SubscribersModel l;
        public static ConfigModel CONFIG
        {
            get { return c; }
            set { c = value; }
        }
        public BaseController() {
            CONFIG = JsonConvert.DeserializeObject<ConfigModel>(Cache.RetrieveFileContents("CONFIG"));
        }
        public static SubscribersModel SUBSCRIBERS
        {
            get { return l; }
            set { l = value; }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void AssignLogID()
        {
            if (string.IsNullOrEmpty(KBZRefNo))
            {
                Request.Headers.TryGetValue("KBZ_REF_NO", out var LOGID);
                if (Request.Headers.ContainsKey("KBZ_REF_NO"))
                {
                    KBZRefNo = ((IList<String>)LOGID)[0].ToString();
                    KBZ_REF_NO = KBZRefNo;
                }
                else
                {
                    KBZRefNo = System.Guid.NewGuid().ToString();
                    KBZ_REF_NO = KBZRefNo;
                }
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public void GetClaimUsername()
        {
            try
            {
                //string usernameKey = ClaimUserKey;
                //var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
                //string username = identity.Claims.SingleOrDefault(f => f.Type == usernameKey).Value;
                //return username;
                
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(parameter);
                var tokenS = jsonToken as JwtSecurityToken;
                claimUser = tokenS.Claims.First(claim => claim.Type == "user_name").Value;
            }
            catch (Exception)
            {
                claimUser = null;
            }

        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public string logsAudit(AuditLogModel logs)
        {
            logs.AuditLogID = Guid.NewGuid().ToString();
            logs.LoggedBy = claimUser;
            if (String.IsNullOrEmpty(logs.KBZMessageID)) logs.KBZMessageID = Guid.NewGuid().ToString();
            //logs.LoggedBy = GetClaimUsername();
            logs.LoggedDate = DateTime.Now;
            AuditThread(logs);
            return logs.KBZMessageID;
        }
        void AuditThread(AuditLogModel logs)
        {
            var content = new StringContent(JsonConvert.SerializeObject(logs), Encoding.UTF8, "application/json");
            HttpRequestMessage requestMsg = new HttpRequestMessage { Method = HttpMethod.Post, Content = content, RequestUri = new Uri(Convert.ToString(CONFIG.LOG.Business.API_URL) + "AuditLog") };
            client.SendAsync(requestMsg);
        }
        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }


        public static bool ValidateSubscriberNum(string mobile, string compare)
        {
            string[] val = compare.Split(',');
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] == mobile.Substring(0, Convert.ToInt16(val[i].Length)))
                    return true;
            }
            return false;
        }


    }
}
