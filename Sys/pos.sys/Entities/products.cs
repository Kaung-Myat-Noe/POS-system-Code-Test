using System.ComponentModel.DataAnnotations.Schema;

namespace pos.sys.Entities
{
    [Table("products")]
    public class products
    {
        public string? Id { get; set; }
        public string? name { get; set; }
        public int? quantity { get; set; }
        public decimal? price { get; set; }
        public DateTime? created_date { get; set; }
        public string? created_userid { get; set; }
        public DateTime? updated_date { get; set; }
        public string? updated_userid { get; set; }
        public DateTime? deleted_date { get; set; }
        public string? deleted_userid { get; set; }
        public int? status { get; set; }
        public bool? isdeleted { get; set; }
        public bool? isAlcohol { get; set; }
    }
}
