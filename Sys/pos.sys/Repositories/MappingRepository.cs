using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IMappingRepository : IRepository<MAPPING>
    {

    }
    public class MappingRepository : Repository<MAPPING>, IMappingRepository
    {
        public MappingRepository(ApplicationDBContext _ctx) : base(_ctx) { }

    }
}
