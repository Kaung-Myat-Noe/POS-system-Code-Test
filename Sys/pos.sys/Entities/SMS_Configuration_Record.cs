using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("SMS_CONFIG_RECORDS")]
    public class SMS_CONFIG_RECORDS
    {
        public string CONFIG_RECORD_ID { get; set; }
        public string? RECORD_TYPE { get; set; }
        public string? GATWEWAY { get; set; }
        public string? CLIENTCODE { get; set; }
        public string? TELCO_CODE { get; set; }
        public string? EMPLOYEE_ID { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public string? REQ_CONFIG { get; set; }
        public DateTime? RECORD_DATE { get; set; }
        public string? STATUS { get; set; }
        public string? DEPENDENCY_TYPE { get; set; }
    }
}
