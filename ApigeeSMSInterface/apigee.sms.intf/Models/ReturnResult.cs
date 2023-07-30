using System.ComponentModel.DataAnnotations;
using static apigee.sms.intf.Models.ReturnResultEnum;

namespace apigee.sms.intf.Models
{
    public class ReturnResult
    {
        public returnResultEnum status{get;set;}
        public string message {get;set;} = String.Empty;

        public dynamic? returnResult {get;set;} = null;
    }
}