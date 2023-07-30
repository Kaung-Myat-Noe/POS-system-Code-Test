using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IInvoiceRepository : IRepository<invoice>
    {

    }
    public class InvoiceRepository : Repository<invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDBContext _ctx) : base(_ctx) { }
    }
}
