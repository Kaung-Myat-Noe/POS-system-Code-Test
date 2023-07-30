using Newtonsoft.Json;

namespace apigee.sms.intf.Models
{
    public class PhonePrefix
    {
        /// <summary>
        /// Operator
        /// </summary>
        public string Operator { get; set; } = String.Empty;

        /// <summary>
        /// Phone Number Prefixes
        /// </summary>
        public string Prefixes { get; set; } = String.Empty;
    }
}
