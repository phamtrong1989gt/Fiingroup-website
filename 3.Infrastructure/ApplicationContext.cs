using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PT.Domain.Model;
using PT.Domain.Seedwork;

namespace PT.Infrastructure
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IUnitOfWork
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
             : base(options)
        {
        }

        // System User
        public DbSet<RoleController> RoleControllers { get; set; }
        public DbSet<RoleAction> RoleActions { get; set; }
        public DbSet<RoleDetail> RoleDetails { get; set; }
        public DbSet<RoleGroup> RoleGroups { get; set; }
        public DbSet<RoleArea> RoleAreas { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DbSet<Category> Categorys { get; set; }
        public DbSet<Link> Links { get; set; }

        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<ContentPageCategory> ContentPageCategorys { get; set; }
        public DbSet<ContentPageRelated> ContentPageRelateds { get; set; }
        public DbSet<ContentPageTag> ContentPageTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<StaticInformation> StaticInformations { get; set; }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<BannerItem> BannerItems { get; set; }

        public DbSet<ServicePrice> ServicePrices { get; set; }

        public DbSet<ImageGallery> ImageGallerys { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<LinkReference> LinkReferences { get; set; }
        public DbSet<ContentPageReference> ContentPageReferences { get; set; }
        public DbSet<Country> Countrys { get; set; }
        public DbSet<RedirectLink> RedirectLinks { get; set; }
        public DbSet<TourDay> TourDays { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourCategory> TourCategorys { get; set; }
        public DbSet<TourDayGallery> TourDayGallerys { get; set; }
        public DbSet<TourGallery> TourGallerys { get; set; }
        public DbSet<TourType> TourTypes { get; set; }
        public DbSet<FileData> FileDatas { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategorys { get; set; }
        public DbSet<Portal> Portals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            base.OnModelCreating(builder);

            builder.Entity<RoleDetail>().ToTable("RoleDetail", "adm").HasKey(c => new { c.ActionId, c.RoleId });
            builder.Entity<RoleController>().ToTable("RoleController", "adm");
            builder.Entity<RoleAction>().ToTable("RoleAction", "adm");
            builder.Entity<RoleGroup>().ToTable("RoleGroup", "adm");
            builder.Entity<RoleArea>().ToTable("RoleArea", "adm");
            builder.Entity<ApplicationUser>().ToTable("User", "adm");
            builder.Entity<ApplicationRole>().ToTable("Role", "adm");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRole", "adm");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaim", "adm");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogin", "adm");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserToken", "adm");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaim", "adm");
            builder.Entity<Log>().ToTable("Log", "adm").HasIndex(x => new { x.Type, x.ObjectType, x.ActionTime});
            builder.Entity<Category>().ToTable("Category").HasIndex(x => new { x.Type, x.Delete, x.Status, x.Language });
            builder.Entity<Link>().ToTable("Link").HasIndex(x => new { x.Type, x.Delete, x.Status, x.Language,x.ObjectId,x.IsStatic,x.IncludeSitemap });
            builder.Entity<ContentPage>().ToTable("ContentPage").HasIndex(x => new { x.Type, x.Delete, x.Status, x.Language, x.DatePosted });
            builder.Entity<ContentPageCategory>().ToTable("ContentPageCategory").HasIndex(x => new { x.CategoryId,x.ContentPageId});
            builder.Entity<ContentPageRelated>().ToTable("ContentPageRelated").HasIndex(x => new { x.ContentPageId });
            builder.Entity<ContentPageTag>().ToTable("ContentPageTag").HasIndex(x => new { x.ContentPageId });
            builder.Entity<Tag>().ToTable("Tag").HasIndex(x => new { x.Delete, x.Status, x.Language });
            builder.Entity<Employee>().ToTable("Employee").HasIndex(x => new { x.Delete, x.Status, x.Language });
            builder.Entity<Customer>().ToTable("Customer").HasIndex(x => new {  x.Delete, x.Status});
            builder.Entity<Contact>().ToTable("Contact").HasIndex(x => new { x.Type, x.Delete, x.Status, x.Language,x.IsHome });
            builder.Entity<StaticInformation>().ToTable("StaticInformation").HasIndex(x => new { x.Delete, x.Status }); ;
            builder.Entity<Menu>().ToTable("Menu");
            builder.Entity<MenuItem>().ToTable("MenuItem");
            builder.Entity<Banner>().ToTable("Banner");
            builder.Entity<BannerItem>().ToTable("BannerItem");
            builder.Entity<ServicePrice>().ToTable("ServicePrice").HasIndex(x => new { x.Delete, x.Status });
            builder.Entity<ImageGallery>().ToTable("ImageGallery").HasIndex(x => new { x.Delete, x.Status });
            builder.Entity<Image>().ToTable("Image").HasIndex(x => new {  x.Status,x.ImageGalleryId });
            builder.Entity<LinkReference>().ToTable("LinkReference");
            builder.Entity<ContentPageReference>().ToTable("ContentPageReference").HasIndex(x => new { x.ContentPageId});
            builder.Entity<Country>().ToTable("Country");
            builder.Entity<RedirectLink>().ToTable("RedirectLink");
            builder.Entity<TourDay>().ToTable("TourDay");
            builder.Entity<Tour>().ToTable("Tour");
            builder.Entity<TourCategory>().ToTable("TourCategory");
            builder.Entity<TourDayGallery>().ToTable("TourDayGallery");
            builder.Entity<TourGallery>().ToTable("TourGallery");
            builder.Entity<TourType>().ToTable("TourType");
            builder.Entity<FileData>().ToTable("FileData");
            builder.Entity<PaymentTransaction>().ToTable("PaymentTransaction");
            builder.Entity<Product>().ToTable("Product");
            builder.Entity<ProductCategory>().ToTable("ProductCategory");
            builder.Entity<Portal>().ToTable("Portal");
        }
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            _ = await base.SaveChangesAsync();
            return true;
        }
    }
}
