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
            // //
            // builder.Entity<ApplicationRole>().HasData(
            //     new ApplicationRole { Id = 1, Name = "Supper admin", Type = RoleManagerType.Default, NormalizedName = "Supper admin".ToUpper(), Description = "Tất cả các quyền bao gồm quyền quản lý tài khoản, quản lý quyền." },
            //     new ApplicationRole { Id = 2, Name = "Admin", Type = RoleManagerType.Default, NormalizedName = "Admin".ToUpper(), Description = "Tất cả quyền không bao gồm quyền quản lý tài khoản, quản lý quyền." }
            // );
            // // Quyền Area
            // builder.Entity<RoleArea>().HasData(
            //  new RoleArea() { Id = "Base", Name = "Base", Order = 1, Status = true },
            //  new RoleArea() { Id = "User", Name = "User", Order = 2, Status = true },
            //  new RoleArea() { Id = "Null", Name = "Null", Order = 3, Status = true },
            //  new RoleArea() { Id = "Log", Name = "Log", Order = 4, Status = true },
            //  new RoleArea() { Id = "Setting", Name = "Setting", Order = 5, Status = true }
            // );
            // // Quyền nhóm
            // builder.Entity<RoleGroup>().HasData(
            //     new RoleGroup() { Id = 1, Name = "Điều khiển", Status = true, Order = 1 },
            //     new RoleGroup() { Id = 2, Name = "Hệ thống", Status = true, Order = 2 }
            //);
            // // Quyền controller
            // builder.Entity<RoleController>().HasData(
            //     new RoleController { Id = "Home", Name = "Trang điều khiển", AreaId = "Null", GroupId = 1, Status = true, Order = 1 },
            //     new RoleController { Id = "UserManager", Name = "Quản lý tài khoản", AreaId = "User", GroupId = 2, Status = true, Order = 1 },
            //     new RoleController { Id = "RoleManager", Name = "Quản lý nhóm quyền", AreaId = "User", GroupId = 2, Status = true, Order = 2 },
            //     new RoleController { Id = "SettingManager", Name = "Cấu hình hệ thống", AreaId = "Setting", GroupId = 2, Status = true, Order = 1 },
            //     new RoleController { Id = "LogManager", Name = "Log người dùng", AreaId = "Log", GroupId = 2, Status = true, Order = 1 }
            // );
            // // Quyền acction
            // builder.Entity<RoleAction>().HasData(

            //     new RoleAction() { Id = 1, ActionName = "Danh sách", Name = "Index", ControllerId = "RoleManager", Status = true, Order = 1 },
            //     new RoleAction() { Id = 2, ActionName = "Thêm mới", Name = "Create", ControllerId = "RoleManager", Status = true, Order = 2 },
            //     new RoleAction() { Id = 3, ActionName = "Cập nhật", Name = "Update", ControllerId = "RoleManager", Status = true, Order = 3 },
            //     new RoleAction() { Id = 4, ActionName = "Xóa", Name = "Delete", ControllerId = "RoleManager", Status = true, Order = 4 },

            //     new RoleAction() { Id = 5, ActionName = "Danh sách", Name = "Index", ControllerId = "UserManager", Status = true, Order = 1 },
            //     new RoleAction() { Id = 6, ActionName = "Thêm mới", Name = "Create", ControllerId = "UserManager", Status = true, Order = 2 },
            //     new RoleAction() { Id = 7, ActionName = "Cập nhật", Name = "Update", ControllerId = "UserManager", Status = true, Order = 3 },
            //     new RoleAction() { Id = 8, ActionName = "Xóa", Name = "Delete", ControllerId = "UserManager", Status = true, Order = 4 },

            //     new RoleAction() { Id = 9, ActionName = "Trang điều khiển", Name = "Index", ControllerId = "Home", Status = true, Order = 1 },

            //     new RoleAction() { Id = 10, ActionName = "Danh sách", Name = "Index", ControllerId = "LogManager", Status = true, Order = 1 },
            //     new RoleAction() { Id = 11, ActionName = "Chi tiết", Name = "Details", ControllerId = "LogManager", Status = true, Order = 2 },

            //     new RoleAction() { Id = 12, ActionName = "Cấu hình hệ thống", Name = "Index", ControllerId = "SettingManager", Status = true, Order = 1 }
            //     //
            // );
            // // Add Chi tiết quyền
            // builder.Entity<RoleDetail>().HasData(
            //     new RoleDetail() { ActionId = 1, RoleId = 1},
            //     new RoleDetail() { ActionId = 2, RoleId = 1 },
            //     new RoleDetail() { ActionId = 3, RoleId = 1 },
            //     new RoleDetail() { ActionId = 4, RoleId = 1 },
            //     new RoleDetail() { ActionId = 5, RoleId = 1 },
            //     new RoleDetail() { ActionId = 6, RoleId = 1 },
            //     new RoleDetail() { ActionId = 7, RoleId = 1 },
            //     new RoleDetail() { ActionId = 8, RoleId = 1 },
            //     new RoleDetail() { ActionId = 9, RoleId = 1 },
            //     new RoleDetail() { ActionId = 10, RoleId = 1 },
            //     new RoleDetail() { ActionId = 11, RoleId = 1 },
            //     new RoleDetail() { ActionId = 12, RoleId = 1  },

            //     new RoleDetail() { ActionId = 1, RoleId = 2 },
            //     new RoleDetail() { ActionId = 2, RoleId =2 },
            //     new RoleDetail() { ActionId = 3, RoleId = 2 },
            //     new RoleDetail() { ActionId = 4, RoleId = 2 },
            //     new RoleDetail() { ActionId = 9, RoleId = 2 },
            //     new RoleDetail() { ActionId = 10, RoleId = 2 },
            //     new RoleDetail() { ActionId = 11, RoleId = 2 },
            //     new RoleDetail() { ActionId = 12, RoleId = 2 }
            // );
            // // Add user
            // builder.Entity<ApplicationUser>().HasData(
            //     new ApplicationUser()
            //     {
            //         Id = 1,
            //         UserName = "administrator",
            //         NormalizedUserName= "administrator",
            //         CreatedDate = DateTime.Now,
            //         Email ="phamtrongit1989@gmail.com",
            //         NormalizedEmail = "phamtrongit1989@gmail.com".ToUpper(),
            //         CreatedUserId = 1,
            //         EmailConfirmed = true,
            //         DisplayName = "Quản trị hệ thống cao cấp",
            //         IsSuperAdmin = true,
            //         IsLock = false,
            //         IsReLogin = false,
            //         PasswordHash = "AQAAAAEAACcQAAAAEHWyCT5aMJYwXa6RyYOa+UZqyQvYSLZYVwfX4jGuVhokBim0S2Wpg0SFJ50M9tlWHQ==",
            //         SecurityStamp = "FLTQJCHSBBNGSXLYRVUOBXZHVBNJYK26",
            //         ConcurrencyStamp = "883f2448-b8c4-4762-af30-3ec42520c676"
            //     }
            // );
            // // Add user
            // builder.Entity<ApplicationUser>().HasData(
            //     new ApplicationUser()
            //     {
            //         Id = 2,
            //         UserName = "supperadmin",
            //         NormalizedUserName = "supperadmin",
            //         CreatedDate = DateTime.Now,
            //         CreatedUserId = 1,
            //         EmailConfirmed=true,
            //         DisplayName="Quản trị hệ thống cao cấp",
            //         IsSuperAdmin=false,
            //         IsLock=false,
            //         IsReLogin=false,
            //         PasswordHash = "AQAAAAEAACcQAAAAEPcb9Buzbxp8mXOkRCb6A13bLwPr7T/c3RbH26MgkJp22P60DIDm+u4d3gaZgKV9Iw==",
            //         SecurityStamp= "N6OVCSMBNYHCHLDCMOJDZ2JVA4SEMCOO",
            //         ConcurrencyStamp= "1bec2241-27a6-4378-982e-291042231434"
            //     }
            // );
            // builder.Entity<ApplicationUser>().HasData(
            //    new ApplicationUser()
            //    {
            //        Id = 3,
            //        UserName = "admin168",
            //        NormalizedUserName = "admin168",
            //        CreatedDate = DateTime.Now,
            //        CreatedUserId = 1,
            //        EmailConfirmed = true,
            //        DisplayName = "Quản trị hệ thống",
            //        IsSuperAdmin = false,
            //        IsLock = false,
            //        IsReLogin = false,
            //        PasswordHash = "AQAAAAEAACcQAAAAEPcb9Buzbxp8mXOkRCb6A13bLwPr7T/c3RbH26MgkJp22P60DIDm+u4d3gaZgKV9Iw==",
            //        SecurityStamp = "N6OVCSMBNYHCHLDCMOJDZ2JVA4SEMCOO",
            //        ConcurrencyStamp = "1bec2241-27a6-4378-982e-291042231434"
            //    }
            //);
        }
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            _ = await base.SaveChangesAsync();
            return true;
        }
    }
}
