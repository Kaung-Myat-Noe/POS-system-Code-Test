using pos.sys.Entities;
using AspNetCore.ServiceRegistration.Dynamic;

namespace pos.sys.Repositories
{
    [ScopedService]
    public interface ISMSConfigRecordRepository : IRepository<SMS_CONFIG_RECORDS>
    {
    }
    public class SMSConfigRecordRepository : Repository<SMS_CONFIG_RECORDS>, ISMSConfigRecordRepository
    {
        public SMSConfigRecordRepository(ApplicationDBContext _ctx) : base(_ctx) { }
    }
}
