using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apigee.sms.intf.Controllers
{    
    public class BaseController : ControllerBase
    {
        public static string? KBZRefNo { get; set; }
        public static string? KBZ_REF_NO { get; set; }
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
    }
}
