using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class FileDataRepository : BaseRepository<FileData>, IFileDataRepository
    {
        public FileDataRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
