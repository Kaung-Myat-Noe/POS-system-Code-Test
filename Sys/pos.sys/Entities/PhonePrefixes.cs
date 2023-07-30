using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("SMSPREFIX")]
    public class PhonePrefixes
    {
        public string? KEY { get; set; }
        public string? VALUE { get; set; }
    }
}
