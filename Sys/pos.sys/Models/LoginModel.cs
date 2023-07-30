using System.Security.Policy;

namespace pos.sys.Models
{
    public class LoginModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string? phone{ get; set; }
    }
}
