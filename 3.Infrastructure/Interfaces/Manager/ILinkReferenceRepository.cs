using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface ILinkReferenceRepository : IGenericRepository<LinkReference>
    {
        Task<List<Link>> SearchByLink(int linkId);
        Task<string> GetLink(string language, int linkId);
        Task ReferenceUpdate(SeoModel seoModel);
    }
}
