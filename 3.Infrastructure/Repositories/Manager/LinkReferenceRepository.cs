using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
namespace PT.Infrastructure.Repositories
{
    public class LinkReferenceRepository : BaseRepository<LinkReference>, ILinkReferenceRepository
    {
        private readonly ApplicationContext _context;
        public LinkReferenceRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Link>> SearchByLink(int linkId)
        {
            var query = _context.Links.Where(m => _context.LinkReferences.Any(x => x.LinkId1==linkId &&x.LinkId2==m.Id) && !m.Delete).AsQueryable();
            return await query.ToListAsync();
        }
        public async Task<string> GetLink(string language, int linkId)
        {
            var dl = await _context.Links.FirstOrDefaultAsync(x => _context.LinkReferences.Any(m => m.LinkId1 == linkId && m.LinkId2 == x.Id && x.Language == language));
            if(dl==null)
            {
                return null;
            }
            else
            {
                return PT.Shared.Functions.FormatUrl(dl.Language, dl?.Slug);
            }
        }
        public async Task ReferenceUpdate(SeoModel seoModel)
        {
            var curentReference = await _context.LinkReferences.Where(x => x.LinkId1 == seoModel.LinkId).ToListAsync();

            var dlVi = curentReference.FirstOrDefault(x => x.Language == "vi");
            var dlEn = curentReference.FirstOrDefault(x => x.Language == "en");


            int lastChildrentIdVi =  dlVi==null?0: dlVi.LinkId2;
            int lastChildrentIdEn = dlEn ==null?0: dlEn.LinkId2;

            await UpdateLinkReferenceByLanguage(seoModel.LinkId, seoModel.Language, "vi", dlVi, lastChildrentIdVi, seoModel.ReferenceLinkIdVI ?? 0, seoModel.IsAutoVI);
            await UpdateLinkReferenceByLanguage(seoModel.LinkId, seoModel.Language, "en", dlEn, lastChildrentIdEn, seoModel.ReferenceLinkIdEN??0, seoModel.IsAutoEn);
            
        }
        private async Task UpdateLinkReferenceByLanguage(int parentLinkId, string parentLanguage,string childrentLanguage, LinkReference parentLink, int lastChildrentId, int newChildrentLink, bool isAuto)
        {
            if (lastChildrentId != newChildrentLink)
            {
                // Cập nhật link
                if (parentLink == null)
                {
                    parentLink = new LinkReference
                    {
                        LinkId1 = parentLinkId,
                        LinkId2 = newChildrentLink,
                        Language = childrentLanguage
                    };
                    await _context.LinkReferences.AddAsync(parentLink);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    parentLink.LinkId2 = newChildrentLink;
                    _context.LinkReferences.Update(parentLink);
                    await _context.SaveChangesAsync();
                }

                // Cập nhật ánh xạ theo link mới

                if (isAuto && newChildrentLink > 0)
                {
                    var dlAnhXa = await _context.LinkReferences.FirstOrDefaultAsync(x => x.LinkId1 == newChildrentLink && x.Language == parentLanguage);
                    if (dlAnhXa == null)
                    {
                        dlAnhXa = new LinkReference
                        {
                            Language = parentLanguage,
                            LinkId1 = newChildrentLink,
                            LinkId2 = parentLinkId
                        };
                        await _context.LinkReferences.AddAsync(dlAnhXa);
                        await _context.SaveChangesAsync();
                    }
                    else if (dlAnhXa != null)
                    {
                        dlAnhXa.LinkId2 = parentLinkId;
                        _context.LinkReferences.Update(dlAnhXa);
                        await _context.SaveChangesAsync();
                    }
                }

                // Xóa anh xạ cũ
                if (isAuto && lastChildrentId > 0)
                {
                    var dlAnhXaEn = await _context.LinkReferences.FirstOrDefaultAsync(x => x.LinkId1 == lastChildrentId && x.Language == parentLanguage);
                    if (dlAnhXaEn == null)
                    {
                        dlAnhXaEn = new LinkReference
                        {
                            Language = parentLanguage,
                            LinkId1 = lastChildrentId,
                            LinkId2 = 0
                        };
                        await _context.LinkReferences.AddAsync(dlAnhXaEn);
                        await _context.SaveChangesAsync();
                    }
                    else if (dlAnhXaEn != null)
                    {
                        dlAnhXaEn.LinkId2 = 0;
                        _context.LinkReferences.Update(dlAnhXaEn);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}