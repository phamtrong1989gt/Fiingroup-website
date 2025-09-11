using System;
using System.Collections.Generic;
using System.Text;
using PT.Domain.Seedwork;
using PT.Domain.Model;
using System.Threading.Tasks;
namespace PT.Infrastructure.Interfaces
{
    public interface ILogRepository : IGenericRepository<Log>
    {
    }
}
