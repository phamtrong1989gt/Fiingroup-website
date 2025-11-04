using PT.Domain.Model;
using PT.Infrastructure.Repositories;

namespace PT.Infrastructure.Interfaces
{
    public interface ISeoSettingRepository : IGenericRepository<SeoSetting>
    {
    }
    public class SeoSettingRepository : BaseRepository<SeoSetting>, ISeoSettingRepository
    {
        public SeoSettingRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
