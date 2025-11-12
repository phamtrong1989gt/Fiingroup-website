using PT.Domain.Model;
using PT.Infrastructure.Repositories;

namespace PT.Infrastructure.Interfaces
{
    public interface IEmailSettingRepository : IGenericRepository<EmailSetting>
    {
    }
    public class EmailSettingRepository : BaseRepository<EmailSetting>, IEmailSettingRepository
    {
        public EmailSettingRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
