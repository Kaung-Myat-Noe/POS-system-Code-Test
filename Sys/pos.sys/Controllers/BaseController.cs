using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using pos.sys.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;

namespace pos.sys.Controllers
{
    public class BaseController : ControllerBase
    {
        public static string? RefNo;
        [ApiExplorerSettings(IgnoreApi = true)]
        public void AssignLogID()
        {
            if (string.IsNullOrEmpty(RefNo))
            {
                Request.Headers.TryGetValue("REF_NO", out var LOGID);
                if (Request.Headers.ContainsKey("REF_NO"))
                    RefNo = ((IList<String>)LOGID)[0].ToString();
                else
                    RefNo = System.Guid.NewGuid().ToString();
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public string GenerateToken(UserModel entity, string issuer, string audienceId, int expirationdate, string Key)
        {
            var now = DateTime.UtcNow;

            var claim = new[]
            {
                    new Claim(ClaimTypes.Name, "POS"),
                    new Claim("email", entity.email),
                    new Claim("name", entity.name),
                    new Claim("Id", entity.Id.ToString())

            };
            var secretKey = Guid.NewGuid().ToString().Replace("-", "");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var key = Encoding.ASCII.GetBytes
        (Key);
            var tokeOptions = new JwtSecurityToken(
            issuer: issuer,
                audience: audienceId,
                claims: claim,
                notBefore: now,
                expires: now.AddMinutes(expirationdate),
                signingCredentials: new SigningCredentials
            (new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }
    }
}
