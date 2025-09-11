using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
    public class TourRepository : BaseRepository<Tour>, ITourRepository
    {
        private readonly ApplicationContext _context;
        public TourRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task RemoveCategory(int tourId)
        {
            _context.TourCategorys.RemoveRange(_context.TourCategorys.Where(x => x.TourId == tourId));
            await _context.SaveChangesAsync();
        }

        public async Task<BaseSearchModel<List<Tour>>> SearchPagedListAsync(int page, int limit, int? categoryId, Expression<Func<Tour, bool>> predicate = null, Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null, Expression<Func<Tour, Tour>> select = null, params Expression<Func<Tour, object>>[] includeProperties)
        {
            var query = _context.Tours.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (categoryId != null)
            {
                query = query.Where(x => _context.TourCategorys.Any(m => m.TourId == x.Id && m.CategoryId == categoryId)).AsQueryable();
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

            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Tour).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Tour
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    CreatedDate = x.data.CreatedDate,
                    CreatedUser = x.data.CreatedUser,
                    Days = x.data.Days,
                    DetailDifference = x.data.DetailDifference,
                    DetailNote = x.data.DetailNote,
                    DetailServicesExclusion = x.data.DetailServicesExclusion,
                    DetailServicesInclusion = x.data.DetailServicesInclusion,
                    IsTop = x.data.IsTop,
                    Nights = x.data.Nights,
                    Overview = x.data.Overview,
                    Photos = x.data.Photos,
                    Style = x.data.Style,
                    Trips = x.data.Trips
                }).AsQueryable();

            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            var tourIds = list.Select(x => x.Id).ToList();

            var listCategory = await _context.TourCategorys
                .Where(x => tourIds.Contains(x.TourId))
                .GroupJoin(_context.Categorys, x => x.CategoryId, y => y.Id, (x, y) => new { tourId = x.TourId, categorys = y })
                .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new { x.tourId, category = y })
                .Select(x => new TourCategory { TourId = x.tourId, CategoryId = x.category == null ? 0 : x.category.Id, Category = x.category }).ToListAsync();

            var listLink = list.Select(x => x.Link?.Id);

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
                item.Categorys = listCategory.Where(x => x.TourId == item.Id).Select(x => x.Category).ToList();
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }

            return new BaseSearchModel<List<Tour>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }

        public async Task<List<Tour>> SearchAsync(int skip, int take, int? categoryId, Expression<Func<Tour, bool>> predicate = null, Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null, Expression<Func<Tour, Tour>> select = null)
        {
            var query = _context.Tours.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (categoryId != null)
            {
                query = query.Where(x => _context.TourCategorys.Any(m => m.TourId == x.Id && m.CategoryId == categoryId)).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }

            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Tour).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Tour
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    CreatedDate = x.data.CreatedDate,
                    CreatedUser = x.data.CreatedUser,
                    Days = x.data.Days,
                    DetailDifference = x.data.DetailDifference,
                    DetailNote = x.data.DetailNote,
                    DetailServicesExclusion = x.data.DetailServicesExclusion,
                    DetailServicesInclusion = x.data.DetailServicesInclusion,
                    IsTop = x.data.IsTop,
                    Nights = x.data.Nights,
                    Overview = x.data.Overview,
                    Photos = x.data.Photos,
                    Style = x.data.Style,
                    Trips = x.data.Trips,
                    InfantPrice = x.data.InfantPrice,
                    ElderlyPrice = x.data.ElderlyPrice,
                    ChildrenPrice = x.data.ChildrenPrice,
                    AdultPrice  = x.data.AdultPrice,
                    From = x.data.From,
                    To = x.data.To,
                    PickOut = x.data.PickOut,
                    PickUp = x.data.PickUp,
                    BannerHeader = x.data.BannerHeader,
                    Order = x.data.Order
                }).AsQueryable();

            var list = await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();

            var tourIds = list.Select(x => x.Id).ToList();

            var listCategory = await _context.TourCategorys
                .Where(x => tourIds.Contains(x.TourId))
                .GroupJoin(_context.Categorys, x => x.CategoryId, y => y.Id, (x, y) => new { tourId = x.TourId, categorys = y })
                .SelectMany(x => x.categorys.DefaultIfEmpty(), (x, y) => new { x.tourId, category = y })
                .Select(x => new TourCategory { TourId = x.tourId, CategoryId = x.category == null ? 0 : x.category.Id, Category = x.category }).ToListAsync();

            foreach (var item in list)
            {
                item.Categorys = listCategory.Where(x => x.TourId == item.Id).Select(x => x.Category).ToList();
            }

            return list;
        }

        public class TourMinMaxModel
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }

        public async Task<BaseSearchModel<List<Tour>>> SearchPagedListAsync(
            int page, 
            int limit, 
            int[] categoryIds,
            string[] days,
            int[] tourTypes,
            Expression<Func<Tour, bool>> predicate = null, 
            Func<IQueryable<Tour>, IOrderedQueryable<Tour>> orderBy = null, 
            Expression<Func<Tour, Tour>> select = null)
        {
            var query = _context.Tours.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (categoryIds != null && categoryIds.Count() > 0)
            {
                query = query.Where(x => _context.TourCategorys.Any(m => m.TourId == x.Id && categoryIds.Contains(m.CategoryId))).AsQueryable();
            }

            if (days != null && days.Count() > 0)
            {
                var listMinMax = new List<TourMinMaxModel>();
                foreach(var item in days)
                {
                    try
                    {
                        listMinMax.Add(new TourMinMaxModel
                        {
                            Min = Convert.ToInt32(item.Split('-')[0]),
                            Max = Convert.ToInt32(item.Split('-')[1])
                        });
                    }
                    catch {}
                }
                Expression<Func<Tour, bool>> querryOr = null;
                if (listMinMax.Count == 1)
                {
                    querryOr = x => x.Days >= listMinMax[0].Min && x.Days <= listMinMax[0].Max;
                }
                else if (listMinMax.Count == 2)
                {
                    querryOr = x => (x.Days >= listMinMax[0].Min && x.Days <= listMinMax[0].Max) || (x.Days >= listMinMax[1].Min && x.Days <= listMinMax[1].Max);
                }
                else if (listMinMax.Count == 3)
                {
                    querryOr = x => (x.Days >= listMinMax[0].Min && x.Days <= listMinMax[0].Max) || (x.Days >= listMinMax[1].Min && x.Days <= listMinMax[1].Max) || (x.Days >= listMinMax[2].Min && x.Days <= listMinMax[2].Max);
                }
                else if (listMinMax.Count == 4)
                {
                    querryOr = x => (x.Days >= listMinMax[0].Min && x.Days <= listMinMax[0].Max) || (x.Days >= listMinMax[1].Min && x.Days <= listMinMax[1].Max) || (x.Days >= listMinMax[2].Min && x.Days <= listMinMax[2].Max) || (x.Days >= listMinMax[3].Min && x.Days <= listMinMax[3].Max); ;
                }
                query = query.Where(querryOr).AsQueryable();
            }

            if (tourTypes != null && tourTypes.Count() > 0)
            {
                query = query.Where(x => tourTypes.Contains(x.TourTypeId)).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }

            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.Tour).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Tour
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    CreatedDate = x.data.CreatedDate,
                    CreatedUser = x.data.CreatedUser,
                    Days = x.data.Days,
                    DetailDifference = x.data.DetailDifference,
                    DetailNote = x.data.DetailNote,
                    DetailServicesExclusion = x.data.DetailServicesExclusion,
                    DetailServicesInclusion = x.data.DetailServicesInclusion,
                    IsTop = x.data.IsTop,
                    Nights = x.data.Nights,
                    Overview = x.data.Overview,
                    Photos = x.data.Photos,
                    Style = x.data.Style,
                    Trips = x.data.Trips
                }).AsQueryable();

            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();

            return new BaseSearchModel<List<Tour>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }
    }
}

