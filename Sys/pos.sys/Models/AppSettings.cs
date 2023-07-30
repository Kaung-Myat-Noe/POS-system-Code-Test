namespace pos.sys.Models
{
    public class AppSettings
    {
        public URL URL { get; set; }
        public Jwt JwtConfig { get; set; }

        public string dependency { get; set; }
    }

    public class URL
    {
        public string CALLBACK { get; set; }
        public string Log { get; set; }
        public MGate MGate { get; set; }
        public MGateBulk MGateBulk { get; set; }
        public SMSBrix SMSBrix { get; set; }
        public string INTF { get; set; }
        public string BIZ { get; set; }

    }
    public class MGate
    {
        public string BIZ { get; set; }
    }
    public class MGateBulk
    {
        public string BIZ { get; set; }
    }
    public class SMSBrix
    {
        public string BIZ { get; set; }
    }
    public class Jwt
    {
        public string thumbprint { get; set; }
        public string issuer { get; set; }
        public string audienceId { get; set; }
        public int expiration { get; set; }
        public string Key{ get; set; }
    }
    public class URLs
    {
        public string System { get; set; }
    }
}
