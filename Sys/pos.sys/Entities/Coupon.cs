using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("coupon")]
    public class coupon
    {
        public string? Id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public int? amount { get; set; }
        public int? point { get; set; }
        public string? code { get; set; }
        public int? quantity { get; set; }
        public bool? isinfinite { get; set; }
    }
}
