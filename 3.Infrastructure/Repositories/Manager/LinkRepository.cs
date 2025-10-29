using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PT.Infrastructure.Repositories
{
    public class LinkRepository : BaseRepository<Link>, ILinkRepository
    {
        private readonly ApplicationContext _context;
        public LinkRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async override Task<BaseSearchModel<List<Link>>> SearchPagedListAsync(int page, int limit, Expression<Func<Link, bool>> predicate = null, Func<IQueryable<Link>, IOrderedQueryable<Link>> orderBy = null, Expression<Func<Link, Link>> select = null, params Expression<Func<Link, object>>[] includeProperties)
        {
            IQueryable<Link> query = _context.Set<Link>().AsNoTracking();

            if (predicate != null)
            {
                query = _context.Set<Link>().Where(predicate).AsQueryable();
            }
            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            var listLink = list.Select(x => x.Id);

            // List Phiên Bản
            var listReferences = await _context.LinkReferences
                .Where(x => listLink.Contains(x.LinkId1))
                .GroupJoin(_context.Links, x => x.LinkId2, y => y.Id, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new { x.data, link = y })
                .Select(x => new LinkReference
                {
                    Language = x.data.Language,
                    Id = x.data.Id,
                    LinkId2 = x.data.LinkId2,
                    LinkId1 = x.data.LinkId1,
                    Link2 = x.link
                }).ToListAsync();

            foreach (var item in list)
            {
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Id).ToList();
            }

            return new BaseSearchModel<List<Link>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }

        public async Task<LinkHomeModel> FindObject(string slug, string language)
        {
            if(slug=="")
            {
                var objHome = await _context.Links.FirstOrDefaultAsync(x => x.Slug == "" && x.Language == language);
                if (objHome == null)
                {
                    return null;
                }
             
                return new LinkHomeModel {
                    Link = objHome,
                    Controller = "Home",
                    Acction = "Index"
                };
            }

            var obj = await _context.Links.FirstOrDefaultAsync(x => x.Slug == slug && x.Language == language);
            if(obj==null)
            {
                return null;
            }

            var data = new LinkHomeModel
            {
               Link = obj
            };

            data.Controller = obj.Controller;
            data.Acction = obj.Acction;

            //switch (obj.Type)
            //{
            //    case CategoryType.Static:

            //        if(obj.ObjectId==3 || obj.ObjectId == 4)
            //        {
            //            data.Controller = "Home";
            //            data.Acction = "About";
            //        }
            //        else if (obj.ObjectId == 5 || obj.ObjectId == 6)
            //        {
            //            data.Controller = "ServicePriceHome";
            //            data.Acction = "Details";
            //        }
            //        else if (obj.ObjectId == 7 || obj.ObjectId == 8)
            //        {
            //            data.Controller = "ContactHome";
            //            data.Acction = "Contact";
            //        }
            //        else if (obj.ObjectId == 9 || obj.ObjectId == 10)
            //        {
            //            data.Controller = "ContactHome";
            //            data.Acction = "Testimonials";
            //        }
            //        else if (obj.ObjectId == 11 || obj.ObjectId == 12)
            //        {
            //            data.Controller = "ContentPageHome";
            //            data.Acction = "FAQ";
            //        }
            //        else if (obj.ObjectId == 13 || obj.ObjectId == 14)
            //        {
            //            data.Controller = "EmployeeHome";
            //            data.Acction = "Employees";
            //        }

            //        else if (obj.ObjectId == 15 || obj.ObjectId == 16)
            //        {
            //            data.Controller = "Home";
            //            data.Acction = "Search";
            //        }

            //        else if (obj.ObjectId == 17 || obj.ObjectId == 18)
            //        {
            //            data.Controller = "Home";
            //            data.Acction = "Clinic1";
            //        }

            //        else if (obj.ObjectId == 19 || obj.ObjectId == 20)
            //        {
            //            data.Controller = "Home";
            //            data.Acction = "Clinic2";
            //        }
            //        else if (obj.ObjectId == 21 || obj.ObjectId == 22)
            //        {
            //            data.Controller = "Home";
            //            data.Acction = "Page404";
            //        }
            //        else if (obj.ObjectId == 23 || obj.ObjectId == 24)
            //        {
            //            data.Controller = "ContactHome";
            //            data.Acction = "FamousPeople";
            //        }
            //        break;
            //}
            return data;
        }

        public async Task<Link> FindObject404(string language)
        {
            return await _context.Links.FirstOrDefaultAsync(x => (x.ObjectId == 21 || x.ObjectId == 22) && x.Language ==language && x.Type==CategoryType.Static);
        }
    }
}