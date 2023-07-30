using pos.sys.Entities;
using Microsoft.EntityFrameworkCore;

namespace pos.sys.Repositories
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(180);
        }
        public virtual DbSet<user> user { get; set; }
        public virtual DbSet<invoice> invoice { get; set; }
        public virtual DbSet<invoice_items> invoice_items { get; set; }
        public virtual DbSet<coupon> coupon { get; set; }

        //public virtual DbSet<Transaction_Temp> TELCO_SMS_TRAN_TEMP { get; set; }
        //public virtual DbSet<Transaction_History> TELCO_SMS_TRAN_HISTORY { get; set; }
        //public virtual DbSet<TransactionBulk> TELCO_SMS_TRAN_BULK { get; set; }
        //public virtual DbSet<SMS_CONFIG_RECORDS> SMS_CONFIG_RECORDS { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Transaction>().Property(o => new { o.CLIENT_CODE, o.TRN_REF_NO }).IsConcurrencyToken();
            modelBuilder.Entity<user>().HasKey(o => new { o.Id });
            modelBuilder.Entity<invoice>().HasKey(o => new { o.Id });
            modelBuilder.Entity<invoice_items>().HasKey(o => new { o.Id });
            modelBuilder.Entity<coupon>().HasKey(o => new { o.Id });
            //modelBuilder.Entity<Transaction_Temp>().HasKey(o => new { o.CLIENT_CODE, o.TRN_REF_NO });
            //modelBuilder.Entity<Transaction_History>().HasKey(o => new { o.CLIENT_CODE, o.TRN_REF_NO });
            //modelBuilder.Entity<TransactionBulk>().HasKey(o => new { o.CLIENT_CODE, o.TRN_REF_NO });
            //modelBuilder.Entity<SMS_CONFIG_RECORDS>().HasKey(o => new
            //{
            //    o.CONFIG_RECORD_ID
            //});
            base.OnModelCreating(modelBuilder);


            //modelBuilder.Ignore<Transaction>();

        }


    }
}
