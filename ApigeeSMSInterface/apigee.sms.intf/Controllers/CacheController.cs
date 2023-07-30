using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apigee.sms.intf.Services;
using Newtonsoft.Json;

namespace apigee.sms.intf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : BaseController
    {
        public CacheController()
        {
        }

        [HttpPost("TestForElsa")]
        public async Task<IActionResult> TestForElsa()
        {
            var result = new { Msg = "Testing For Elsa" };
            return new ObjectResult(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result)))
            { StatusCode = (200) };
        }
    }
}
