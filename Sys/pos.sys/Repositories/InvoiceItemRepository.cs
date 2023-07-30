using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface IInvoiceItemRepository : IRepository<invoice_items>
    {

    }
    public class InvoiceItemRepository : Repository<invoice_items>, IInvoiceItemRepository
    {
        public InvoiceItemRepository(ApplicationDBContext _ctx) : base(_ctx) { }
    }
}
