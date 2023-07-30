using Microsoft.EntityFrameworkCore;
using pos.sys.Entities;
namespace pos.sys.Repositories
{
    public class ConfigDBContext :DbContext
    {
        public ConfigDBContext(DbContextOptions<ConfigDBContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Gateway>().HasKey(o => new { o.GATEWAYID });
            modelBuilder.Entity<Gateway>().Property(p => p.MAXSUBSCRIBERS).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<Gateway>().Property(p => p.TIMEOUT).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<Product>().HasKey(o => new { o.PRODUCTID });
            modelBuilder.Entity<MAPPING>().HasKey(o => new { o.MAPPINGID });
            modelBuilder.Entity<PhonePrefixes>().HasKey(o => new { o.KEY });
            
        }

        public virtual DbSet<Gateway> SMSGATEWAY { get; set; }
        public virtual DbSet<Product> SMSPRODUCT { get; set; }
        public virtual DbSet<MAPPING> SMSGATEWAY_PRODUCT_MAPPING { get; set; }
        public virtual DbSet<PhonePrefixes> SMSPREFIX { get; set; }
    }
}
