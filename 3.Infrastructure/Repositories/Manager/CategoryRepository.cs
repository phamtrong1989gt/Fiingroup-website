using Microsoft.EntityFrameworkCore;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PT.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationContext _context;
        public CategoryRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
        
        public override async Task<BaseSearchModel<List<Category>>> SearchPagedListAsync(int page, int limit, Expression<Func<Category, bool>> predicate = null, Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, Expression<Func<Category, Category>> select = null, params Expression<Func<Category, object>>[] includeProperties)
        {
            
            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
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
                .GroupJoin(_context.Links.Where(x => x.Type == ESlugType.Category && !x.Delete).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsQueryable();
            var list = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();
            return new BaseSearchModel<List<Category>>
            {
                Data = list,
                Limit = limit,
                Page = page,
                TotalRows = await query.CountAsync()
            };
        }

        public async Task<List<Category>> SearchAsync(
           bool asNoTracking = false,
           int skip = 0,
           int Take = 0,
           Expression<Func<Category, bool>> predicate = null,
           Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
           Expression<Func<Category, Category>> select = null,
           bool anyContent = false)
        {
            

            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            if (anyContent)
            {
                query = query.Where(x => _context.ContentPages.Any(m => m.ServiceId == x.Id));
            }

            query = query
                .GroupJoin(_context.Links.Where(x=> x.Type == ESlugType.Category && !x.Delete).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome,
                    PortalId = x.data.PortalId,
                    CategoryType = x.data.CategoryType,
                    SlugType = x.data.SlugType
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public override async Task<List<Category>> SearchAsync(
            bool asNoTracking = false,
            int skip = 0, 
            int Take = 0, 
            Expression<Func<Category, bool>> predicate = null, 
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            Expression<Func<Category, Category>> select = null, 
            params Expression<Func<Category, object>>[] includeProperties)
        {
            

            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
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
                .GroupJoin(_context.Links.Where(x => x.Type == ESlugType.Category && !x.Delete).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order =x.data.Order,
                    ParentId =x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2= x.data.Banner2,
                    IsHome =x.data.IsHome
                }).AsQueryable();
            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public  async Task<List<Category>> FindByLinkReference(int skip = 0, int Take = 0, Expression<Func<Category, bool>> predicate = null, Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null, Expression<Func<Category, Category>> select = null)
        {
            

            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            if (orderBy != null)
            {
                query = orderBy(query).AsQueryable();
            }
          
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == ESlugType.Category && !x.Delete).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome,
                    PortalId = x.data.PortalId,
                    CategoryType = x.data.CategoryType,
                    SlugType = x.data.SlugType
                }).AsQueryable();

            if (Take > 0)
            {
                query = query.Skip(skip < 0 ? 0 : skip).Take(Take).AsQueryable();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }

            var list = await query.ToListAsync(); 
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
                item.LinkReferences = listReferences.Where(x => x.LinkId1 == item.Link?.Id).ToList();
            }
            if (select != null)
            {
                query = query.Select(select).AsQueryable();
            }
            return list;
        }
        public int MaxOrder(Expression<Func<Category, bool>> predicate = null)
        {
            var querry = _context.Categorys.AsQueryable();
            if(querry!=null)
            {
                querry = querry.Where(predicate).AsQueryable();
            }
            if(querry.Any())
            {
                return querry.Max(x => x.Order);
            }
            else
            {
                return 0;
            }
        }

        public async override Task<Category> SingleOrDefaultAsync(bool asNoTracking = false, Expression<Func<Category, bool>> predicate = null, params Expression<Func<Category, object>>[] includeProperties)
        {
            

            IQueryable<Category> query = _context.Categorys.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }
            query = query
                .GroupJoin(_context.Links.Where(x => x.Type == ESlugType.Category && !x.Delete).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome,
                    PortalId = x.data.PortalId,
                    CategoryType    = x.data.CategoryType,
                    SlugType        = x.data.SlugType,
                    ExCategoryIds = x.data.ExCategoryIds,
                    ReferentCategoryId = x.data.ReferentCategoryId,
                    
                }).AsQueryable();
           
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public List<PT.Domain.Model.CategoryTreeModel> GetCategoryAncestors(CategoryTreeModel category, List<PT.Domain.Model.CategoryTreeModel> allCategories)
        {
            var ancestors = new List<CategoryTreeModel>();
            var current = category;
            while (current?.ParentId != null && current.ParentId != 0)
            {
                var parent = allCategories.FirstOrDefault(x => x.CategoryId == current.ParentId);
                if (parent == null) break;
                ancestors.Add(parent);
                current = parent;
            }
            ancestors.Reverse();
            return ancestors;
        }

        public async Task<List<CategoryTreeModel>> GetAll(string language, int portalId)
        {
            var query = await (
                from  cat in _context.Categorys
                join link in _context.Links on cat.Id equals link.ObjectId
                where link.Type == ESlugType.Category && !link.Delete && cat.Language == language && cat.PortalId == portalId 
                select new CategoryTreeModel
                {
                    CategoryId = cat.Id,
                    ParentId = cat.ParentId,
                    Language = cat.Language,
                    CategoryName = cat.Name,
                    SlugLink = link.Slug,
                    Type = cat.Type
                }
            ).AsNoTracking().ToListAsync();

            foreach (var item in query)
            {
                item.Path = Functions.FormatUrl(item.Language, item.SlugLink);
            }
            var result = new List<CategoryTreeModel>();
            void AddChildren(int parentId)
            {
                var children = query.Where(x => x.ParentId == parentId).ToList();
                foreach (var child in children)
                {
                    result.Add(child);
                    AddChildren(child.CategoryId);
                }
            }
            AddChildren(0);
            return query;
        }

        public async Task<List<ContentPageCategoryTreeModel>> CurrentTreeContent(int contentPageId, int portalId)
        {
            

            var query = await (
                from cpc in _context.ContentPageCategorys
                join cat in _context.Categorys on cpc.CategoryId equals cat.Id
                join link in _context.Links on cat.Id equals link.ObjectId
                where cpc.ContentPageId == contentPageId && link.Type == ESlugType.Category && !link.Delete && cat.PortalId == portalId
                select new ContentPageCategoryTreeModel
                {
                    CategoryId = cat.Id,
                    ParentId = cat.ParentId,
                    Language = cat.Language,
                    CategoryName = cat.Name,
                    SlugLink = link.Slug,
                    Type = cat.Type
                }
            ).AsNoTracking().ToListAsync();

            foreach(var item in query)
            {
                item.Path = Functions.FormatUrl(item.Language, item.SlugLink);
            }
            // Sắp xếp danh mục theo Level 1 => Level 2 => Level 3 Id cha có ParentId = 0
            var result = new List<ContentPageCategoryTreeModel>();
            void AddChildren(int parentId)
            {
                var children = query.Where(x => x.ParentId == parentId).ToList();
                foreach (var child in children)
                {
                    result.Add(child);
                    AddChildren(child.CategoryId);
                }
            }
            AddChildren(0);
            return query;
        }

        //private List<Category> CurrentTreeChildrentGetParrent(List<Category> list, int currentCategoryId,int currentOrder)
        //{
        //    var listData = new List<Category>();
        //    var getParent = list.FirstOrDefault(x => x.Id == currentCategoryId);
        //    if(getParent!=null)
        //    {
        //        if(getParent.Id == getParent.ParentId)
        //        {
        //            return null;
        //        }    
        //        getParent.Order = currentOrder;
        //        currentOrder--;
        //        listData.Add(getParent);
        //        var nextData = CurrentTreeChildrentGetParrent(list, getParent.ParentId, currentOrder);
        //        if(nextData!=null)
        //        {
        //            listData.AddRange(nextData);
        //        }
        //        return listData;
        //    }
        //    return listData;
        //}

        /// <summary>
        /// Trả về danh sách các giá trị enum CategoryType có tiền tố "Category".
        /// Tham số đầu vào không được sử dụng nhưng giữ để tương thích API.
        /// </summary>
        public List<ECategoryType> GetCategoryPrefixedTypes()
        {
            return Enum.GetValues(typeof(ECategoryType))
                .Cast<ECategoryType>()
                .Where(t => t.ToString().StartsWith("ContentPage", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public CategoryType CategoryTypeCategoryToContentPage(CategoryType categoryType)
        {
            return CategoryType.CategoryBlog;
        }
    }
}