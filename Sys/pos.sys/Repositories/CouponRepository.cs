using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface ICouponRepository : IRepository<coupon>
    {

    }
    public class CouponRepository : Repository<coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDBContext _ctx) : base(_ctx) { }
    }
}
