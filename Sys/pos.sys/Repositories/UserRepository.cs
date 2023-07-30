using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IUserRepository : IRepository<user>
    {

    }
    public class UserRepository : Repository<user>, IUserRepository
    {
        public UserRepository(ApplicationDBContext _ctx) : base(_ctx) { }
    }
}
