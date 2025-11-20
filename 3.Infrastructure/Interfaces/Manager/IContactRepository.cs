using Microsoft.AspNetCore.Mvc.Rendering;
using PT.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PT.Infrastructure.Interfaces
{
    public interface IContactRepository : IGenericRepository<Contact>
    {
        Task<SelectList> ServiesList(string language, int portalId, int? parrentId = null);
        Task<List<ContentPage>> FlowSelectList(string language, int portalId, int parrentId);
    }
}
