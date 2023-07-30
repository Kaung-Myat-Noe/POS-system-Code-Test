using apigee.sms.intf.Services;
using AspNetCore.ServiceRegistration.Dynamic;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;

namespace apigee.sms.intf.Utility
{
    public class Common
    {
        public static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine); // comment
                                                                         //var x509Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + "/publickey.cer"); // used docker
            try
            {
                store.Open(OpenFlags.ReadOnly); // comment
                X509Certificate2Collection certCollection = store.Certificates;
                //X509Certificate2Collection certCollection = new X509Certificate2Collection();  // when used docker
                //certCollection.Add(x509Certificate); // when used docker
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (signingCert.Count == 0)
                    return null;
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }
        public static string GetClaimUsername(string parameter)
        {
            string claimUser = null;
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
            return claimUser;
        }
    }

    [ScopedService]
    public class Redis
    {
        public static IConnectionMultiplexer _redis;
        private static IConfiguration _configuration;
        private static ILogger<Redis> _logger;
        public Redis(IConfiguration configuration, ILogger<Redis> logger) {
            _configuration = configuration;
            _logger = logger;
        }
        public static void RedisConnection()
        {
            try
            {
                _redis = new Lazy<ConnectionMultiplexer>(() =>
                                ConnectionMultiplexer.Connect(_configuration.GetConnectionString("RedisCacheAWSConnection"))).Value;
            }
            catch (Exception ex) {
                _logger.LogError(ex.ToString());

            }
        }
    }
}
