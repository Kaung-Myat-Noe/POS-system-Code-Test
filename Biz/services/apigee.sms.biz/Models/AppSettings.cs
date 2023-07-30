namespace apigee.sms.biz.Models
{
    public class AppSettings
    {
        public URL URL { get; set; }
        public Jwt JwtConfig { get; set; }
        public string dependency { get; set; }

        public string BUS_IP { get; set; }
        public string BUS_SERVER { get; set; }
        public string APILOG { get; set; }
        public RoutesModel ROUTES { get; set; }
        public LogModel LOG { get; set; }
        public SMS_ServicesModel SMS_SERVICE { get; set; }
        public PREFIXModel PREFIX { get; set; }
        public SettingModel SETTING { get; set; }
        public string SMS_ON { get; set; }
    }

    public class URL
    {
        public string OLDSYS { get; set; }        
        public string SYSTEM { get; set; }
    }

    public class Jwt
    {
        public string thumbprint { get; set; }
        public string issuer { get; set; }
        public string audienceId { get; set; }
        public string ClaimUserNameKey { get; set; }
    }
    public class URLs
    {
        public string System { get; set; }
    }
}
