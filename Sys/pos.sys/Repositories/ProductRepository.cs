using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IProductRepository : IRepository<Product>
    {
        bool isExist(string clientcode, string tokenusername);
    }
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDBContext _ctx) : base(_ctx) { }
        public bool isExist(string clientcode, string tokenusername)
        {
            if (base.Query(o => o.CLIENTCODE == clientcode && o.TOKENUSERNAME == tokenusername).Count() > 0)
                return true;
            else
                return false;
        }
    }    
}
