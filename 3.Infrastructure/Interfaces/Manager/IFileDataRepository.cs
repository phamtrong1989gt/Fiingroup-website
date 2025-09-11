using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IFileDataRepository : IGenericRepository<FileData>
    {
    }
}
