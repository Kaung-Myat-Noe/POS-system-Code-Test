using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IPhonePrefixRepository : IRepository<PhonePrefixes>
    {

    }
    public class PhonePrefixRepository : Repository<PhonePrefixes>, IPhonePrefixRepository
    {
        public PhonePrefixRepository(ApplicationDBContext _ctx) : base(_ctx) { }

    }
}
