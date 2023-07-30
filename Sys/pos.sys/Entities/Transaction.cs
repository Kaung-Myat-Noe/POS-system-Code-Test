using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace pos.sys.Entities
{
    [Table("invoice")]
    public class invoice
    {
        public string? Id { get; set; }
        public string invoice_number { get; set; }
        public string? code { get; set; }
        public string? billing_address { get; set; }
        public string? user_id { get; set; }
        public int? totalquantity { get; set; }
        public decimal? totalprice { get; set; }
        public decimal? discount_amount { get; set; }
        public string? cupons_ids { get; set; }
        public DateTime? created_date { get; set; }
    }
    [Table("invoice_items")]
    public class invoice_items
    {
        public string Id { get; set; }
        public string? invoice_id { get; set; }
        public string? product_id { get; set; }
        public int? quantity { get; set; }
        public string? coupon_ids { get; set; }
        public decimal? discount_amount { get; set; }
    }
}
