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
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
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
                query = query.Where(x => _context.ContentPages.Any(m => m.ServiceId == x.Id && m.Type==CategoryType.FAQ));
            }

            query = query
                .GroupJoin(_context.Links.Where(x=> (x.Type == CategoryType.CategoryProduct || x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService || x.Type == CategoryType.CategoryTour)).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
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
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
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
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService || x.Type == CategoryType.CategoryTour).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome,
                    PortalId = x.data.PortalId
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
                .GroupJoin(_context.Links.Where(x => x.Type == CategoryType.CategoryBlog || x.Type == CategoryType.CategoryService).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Content = x.data.Content,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    Order = x.data.Order,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome,
                    PortalId = x.data.PortalId
                }).AsQueryable();
           
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public List<PT.Domain.Model.CategoryTreeModel> GetCategoryAncestors(PT.Domain.Model.CategoryTreeModel category, List<PT.Domain.Model.CategoryTreeModel> allCategories)
        {
            var ancestors = new List<PT.Domain.Model.CategoryTreeModel>();
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

        public async Task<List<CategoryTreeModel>> GetAll(string language, IEnumerable<CategoryType> linkTypes)
        {
            var types = linkTypes?.ToList() ?? new List<CategoryType> {  };
            var query = await (
                from  cat in _context.Categorys
                join link in _context.Links on cat.Id equals link.ObjectId
                where types.Contains(link.Type) && cat.Language == language
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

        public async Task<List<ContentPageCategoryTreeModel>> CurrentTreeContent(int contentPageId, IEnumerable<CategoryType> linkTypes)
        {
            var types = linkTypes?.ToList() ?? new List<CategoryType> { CategoryType.CategoryBlog, CategoryType.CategoryService };

            var query = await (
                from cpc in _context.ContentPageCategorys
                join cat in _context.Categorys on cpc.CategoryId equals cat.Id
                join link in _context.Links on cat.Id equals link.ObjectId
                where cpc.ContentPageId == contentPageId && types.Contains(link.Type)
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

        public async Task<List<Category>> CurrentTreeChildrent(int currentCategoryId, string language, CategoryType type)
        {
            var newList = new List<Category>();

            var query = await _context.Categorys.Where(x=>x.Language== language && x.Type==type)
                .GroupJoin(_context.Links.Where(x => x.Type == type).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsNoTracking().ToListAsync();

            query = query.Where(x =>  x.Link != null).ToList();
            newList = CurrentTreeChildrentGetParrent(query, currentCategoryId, 99);
            return newList.OrderBy(x=>x.Order).ToList();
        }

        private List<Category> CurrentTreeChildrentGetParrent(List<Category> list, int currentCategoryId,int currentOrder)
        {
            var listData = new List<Category>();
            var getParent = list.FirstOrDefault(x => x.Id == currentCategoryId);
            if(getParent!=null)
            {
                if(getParent.Id == getParent.ParentId)
                {
                    return null;
                }    
                getParent.Order = currentOrder;
                currentOrder--;
                listData.Add(getParent);
                var nextData = CurrentTreeChildrentGetParrent(list, getParent.ParentId, currentOrder);
                if(nextData!=null)
                {
                    listData.AddRange(nextData);
                }
                return listData;
            }
            return listData;
        }

        public async Task<Category> GetLink(int objectId, string language, CategoryType type)
        {
            return await _context.Categorys.Where(x => x.Language == language && x.Type == type && x.Id == objectId)
                     .GroupJoin(_context.Links.Where(x => x.Type == type).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                     .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                     {
                         Link = y,
                         Id = x.data.Id,
                         Banner = x.data.Banner,
                         Delete = x.data.Delete,
                         Name = x.data.Name,
                         Language = x.data.Language,
                         Status = x.data.Status,
                         ParentId = x.data.ParentId,
                         Type = x.data.Type,
                         Summary = x.data.Summary,
                         Banner2 = x.data.Banner2,
                         IsHome = x.data.IsHome
                     }).AsNoTracking().FirstOrDefaultAsync();
        }


        public async Task<List<Category>> GetChildrent(int parentId, string language, CategoryType type)
        {
            var newList = new List<Category>();

            var query = await _context.Categorys.Where(x => x.Language == language && x.Type == type)
                .GroupJoin(_context.Links.Where(x => x.Type == type).AsQueryable(), x => x.Id, y => y.ObjectId, (x, y) => new { data = x, links = y })
                .SelectMany(x => x.links.DefaultIfEmpty(), (x, y) => new Category
                {
                    Link = y,
                    Id = x.data.Id,
                    Banner = x.data.Banner,
                    Delete = x.data.Delete,
                    Name = x.data.Name,
                    Language = x.data.Language,
                    Status = x.data.Status,
                    ParentId = x.data.ParentId,
                    Type = x.data.Type,
                    Summary = x.data.Summary,
                    Banner2 = x.data.Banner2,
                    IsHome = x.data.IsHome
                }).AsNoTracking().ToListAsync();

            query = query.Where(x => x.Link != null).ToList();

            if(parentId==0)
            {
                return query;
            }    

            var listItem = GetAllChildrent(query, parentId);
            if(listItem.Count > 0)
            {
                newList.AddRange(listItem);
            }
            return newList.OrderBy(x => x.Order).ToList();
        }

        private List<Category> GetAllChildrent(List<Category> list, int parentId)
        {
            var newList = new List<Category>();
            var parent = list.FirstOrDefault(x => x.Id == parentId);
            if(parent==null)
            {
                return newList;
            }
            if(parent.Id == parent.ParentId)
            {
                return newList;
            }
            foreach (var item in list.Where(x => x.ParentId == parent.Id))
            {
                newList.Add(item);
                var listRage = GetAllChildrent(list, item.Id);
                if(listRage.Count() > 0)
                {
                    newList.AddRange(listRage);
                }
            }
            return newList;
        }
    }
}