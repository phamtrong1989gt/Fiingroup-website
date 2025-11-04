using PT.Domain.Model;
using PT.Infrastructure.Repositories;

namespace PT.Infrastructure.Interfaces
{
    public interface IBindContentSettingRepository : IGenericRepository<BindContentSetting>
    {
    }
    public class BindContentSettingRepository : BaseRepository<BindContentSetting>, IBindContentSettingRepository
    {
        public BindContentSettingRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
