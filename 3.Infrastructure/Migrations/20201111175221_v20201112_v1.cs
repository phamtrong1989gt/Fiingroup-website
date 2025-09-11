using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20201112_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adm");

            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ClassActive = table.Column<string>(nullable: true),
                    Template = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannerItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BannerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Href = table.Column<string>(nullable: true),
                    Target = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Template = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    Banner2 = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    IsHome = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    Age = table.Column<int>(nullable: false),
                    CountryId = table.Column<int>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    PhoneCode = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    AppointmentDate = table.Column<DateTime>(nullable: true),
                    AppointmentDateTo = table.Column<DateTime>(nullable: true),
                    AppointmentStatus = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Delete = table.Column<bool>(nullable: false),
                    Rating = table.Column<double>(nullable: false),
                    IsHome = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ServiceId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 1000, nullable: true),
                    Summary = table.Column<string>(maxLength: 2000, nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(maxLength: 1000, nullable: true),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    DatePosted = table.Column<DateTime>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    Price = table.Column<double>(nullable: true),
                    IsHome = table.Column<bool>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPageCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentPageId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPageCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPageReference",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentPageId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Href = table.Column<string>(nullable: true),
                    Target = table.Column<string>(nullable: true),
                    Rel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPageReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPageRelated",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(nullable: false),
                    ContentPageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPageRelated", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPageTag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentPageId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPageTag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    PhoneCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Gender = table.Column<int>(nullable: false),
                    Birthday = table.Column<DateTime>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: false),
                    Phone = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Facebook = table.Column<string>(nullable: true),
                    Job = table.Column<int>(nullable: false),
                    Degrees = table.Column<string>(nullable: true),
                    Office = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    EmployeeMappingId = table.Column<int>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    Endodontics = table.Column<bool>(nullable: false),
                    GeneralDentistry = table.Column<bool>(nullable: false),
                    OralMedicine = table.Column<bool>(nullable: false),
                    OralSurgery = table.Column<bool>(nullable: false),
                    Orthodontics = table.Column<bool>(nullable: false),
                    Periodontics = table.Column<bool>(nullable: false),
                    Prosthodontics = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageGalleryId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Image1 = table.Column<string>(nullable: true),
                    Image2 = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageGallery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Keywords = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageGallery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Link",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Slug = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ObjectId = table.Column<int>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Lastmod = table.Column<DateTime>(nullable: false),
                    Changefreq = table.Column<string>(maxLength: 10, nullable: true),
                    Priority = table.Column<double>(nullable: false),
                    IsStatic = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Keywords = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    FocusKeywords = table.Column<string>(nullable: true),
                    MetaRobotsIndex = table.Column<string>(nullable: true),
                    MetaRobotsFollow = table.Column<string>(nullable: true),
                    MetaRobotsAdvance = table.Column<string>(nullable: true),
                    IncludeSitemap = table.Column<bool>(nullable: false),
                    Redirect301 = table.Column<string>(nullable: true),
                    FacebookDescription = table.Column<string>(nullable: true),
                    FacebookBanner = table.Column<string>(nullable: true),
                    GooglePlusDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkReference",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LinkId1 = table.Column<int>(nullable: false),
                    LinkId2 = table.Column<int>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Template = table.Column<string>(nullable: true),
                    Template1 = table.Column<string>(nullable: true),
                    Template2 = table.Column<string>(nullable: true),
                    Template3 = table.Column<string>(nullable: true),
                    HasChildrentClass1 = table.Column<string>(nullable: true),
                    HasChildrentClass2 = table.Column<string>(nullable: true),
                    HasChildrentClass3 = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MenuId = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Href = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    Target = table.Column<string>(nullable: true),
                    Class = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedirectLink",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    From = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    Code = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePrice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    FromPrice = table.Column<long>(nullable: false),
                    ToPrice = table.Column<long>(nullable: true),
                    Visits = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Delete = table.Column<bool>(nullable: false),
                    SubParent = table.Column<bool>(nullable: true),
                    ContactPrice = table.Column<bool>(nullable: true),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticInformation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Delete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Delete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tour",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    Days = table.Column<int>(nullable: false),
                    Nights = table.Column<int>(nullable: false),
                    Style = table.Column<int>(nullable: false),
                    TourType = table.Column<int>(nullable: false),
                    Overview = table.Column<string>(nullable: true),
                    Trips = table.Column<string>(nullable: true),
                    DetailDifference = table.Column<string>(nullable: true),
                    DetailServicesInclusion = table.Column<string>(nullable: true),
                    DetailServicesExclusion = table.Column<string>(nullable: true),
                    DetailNote = table.Column<string>(nullable: true),
                    Photos = table.Column<string>(nullable: true),
                    IsTop = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUser = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tour", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TourId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourDay",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TourId = table.Column<int>(nullable: false),
                    Day = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    IsCar = table.Column<bool>(nullable: false),
                    IsCycling = table.Column<bool>(nullable: false),
                    IsCruising = table.Column<bool>(nullable: false),
                    IsFlight = table.Column<bool>(nullable: false),
                    IsLocalBoat = table.Column<bool>(nullable: false),
                    IsLocalTouch = table.Column<bool>(nullable: false),
                    IsHotel = table.Column<bool>(nullable: false),
                    Photos = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDay", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourDayGallery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TourId = table.Column<int>(nullable: false),
                    TourDayId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Image1 = table.Column<string>(nullable: true),
                    Image2 = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDayGallery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourGallery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TourId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Image1 = table.Column<string>(nullable: true),
                    Image2 = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourGallery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ObjectId = table.Column<int>(nullable: false),
                    Object = table.Column<string>(maxLength: 50, nullable: true),
                    ObjectType = table.Column<string>(maxLength: 50, nullable: true),
                    ActionTime = table.Column<DateTime>(nullable: false),
                    AcctionUser = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleArea",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 100, nullable: false),
                    Name = table.Column<string>(maxLength: 500, nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleArea", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleGroup",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 500, nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Avatar = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(maxLength: 100, nullable: true),
                    IsLock = table.Column<bool>(nullable: false),
                    NoteLock = table.Column<string>(maxLength: 1000, nullable: true),
                    IsReLogin = table.Column<bool>(nullable: false),
                    IsSuperAdmin = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    CreatedUserId = table.Column<int>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserId = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    NumberWrongPasswords = table.Column<int>(nullable: true),
                    ExpirationWrongPassword = table.Column<DateTime>(nullable: true),
                    ExpirationResetPassword = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaim",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaim_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "adm",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleController",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 100, nullable: false),
                    AreaId = table.Column<string>(nullable: true),
                    GroupId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleController", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleController_RoleArea_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "adm",
                        principalTable: "RoleArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleController_RoleGroup_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "adm",
                        principalTable: "RoleGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserClaim",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaim_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "adm",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                schema: "adm",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogin_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "adm",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                schema: "adm",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "adm",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "adm",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserToken",
                schema: "adm",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserToken_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "adm",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleAction",
                schema: "adm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ControllerId = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    ActionName = table.Column<string>(maxLength: 500, nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAction_RoleController_ControllerId",
                        column: x => x.ControllerId,
                        principalSchema: "adm",
                        principalTable: "RoleController",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleDetail",
                schema: "adm",
                columns: table => new
                {
                    RoleId = table.Column<int>(nullable: false),
                    ActionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDetail", x => new { x.ActionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleDetail_RoleAction_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "adm",
                        principalTable: "RoleAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleDetail_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "adm",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_Type_Delete_Status_Language",
                table: "Category",
                columns: new[] { "Type", "Delete", "Status", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Type_Delete_Status_Language_IsHome",
                table: "Contact",
                columns: new[] { "Type", "Delete", "Status", "Language", "IsHome" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPage_Type_Delete_Status_Language_DatePosted",
                table: "ContentPage",
                columns: new[] { "Type", "Delete", "Status", "Language", "DatePosted" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPageCategory_CategoryId_ContentPageId",
                table: "ContentPageCategory",
                columns: new[] { "CategoryId", "ContentPageId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPageReference_ContentPageId",
                table: "ContentPageReference",
                column: "ContentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPageRelated_ContentPageId",
                table: "ContentPageRelated",
                column: "ContentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPageTag_ContentPageId",
                table: "ContentPageTag",
                column: "ContentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Delete_Status",
                table: "Customer",
                columns: new[] { "Delete", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Delete_Status_Language",
                table: "Employee",
                columns: new[] { "Delete", "Status", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Image_Status_ImageGalleryId",
                table: "Image",
                columns: new[] { "Status", "ImageGalleryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageGallery_Delete_Status",
                table: "ImageGallery",
                columns: new[] { "Delete", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Link_Type_Delete_Status_Language_ObjectId_IsStatic_IncludeSitemap",
                table: "Link",
                columns: new[] { "Type", "Delete", "Status", "Language", "ObjectId", "IsStatic", "IncludeSitemap" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePrice_Delete_Status",
                table: "ServicePrice",
                columns: new[] { "Delete", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaticInformation_Delete_Status",
                table: "StaticInformation",
                columns: new[] { "Delete", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Delete_Status_Language",
                table: "Tag",
                columns: new[] { "Delete", "Status", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Log_Type_ObjectType_ActionTime",
                schema: "adm",
                table: "Log",
                columns: new[] { "Type", "ObjectType", "ActionTime" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "adm",
                table: "Role",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAction_ControllerId",
                schema: "adm",
                table: "RoleAction",
                column: "ControllerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RoleId",
                schema: "adm",
                table: "RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleController_AreaId",
                schema: "adm",
                table: "RoleController",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleController_GroupId",
                schema: "adm",
                table: "RoleController",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleDetail_RoleId",
                schema: "adm",
                table: "RoleDetail",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "adm",
                table: "User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "adm",
                table: "User",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserId",
                schema: "adm",
                table: "UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                schema: "adm",
                table: "UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                schema: "adm",
                table: "UserRole",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banner");

            migrationBuilder.DropTable(
                name: "BannerItem");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "ContentPage");

            migrationBuilder.DropTable(
                name: "ContentPageCategory");

            migrationBuilder.DropTable(
                name: "ContentPageReference");

            migrationBuilder.DropTable(
                name: "ContentPageRelated");

            migrationBuilder.DropTable(
                name: "ContentPageTag");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "ImageGallery");

            migrationBuilder.DropTable(
                name: "Link");

            migrationBuilder.DropTable(
                name: "LinkReference");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "MenuItem");

            migrationBuilder.DropTable(
                name: "RedirectLink");

            migrationBuilder.DropTable(
                name: "ServicePrice");

            migrationBuilder.DropTable(
                name: "StaticInformation");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Tour");

            migrationBuilder.DropTable(
                name: "TourCategory");

            migrationBuilder.DropTable(
                name: "TourDay");

            migrationBuilder.DropTable(
                name: "TourDayGallery");

            migrationBuilder.DropTable(
                name: "TourGallery");

            migrationBuilder.DropTable(
                name: "Log",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleClaim",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleDetail",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "UserClaim",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "UserLogin",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "UserRole",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "UserToken",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleAction",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "User",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleController",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleArea",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "RoleGroup",
                schema: "adm");
        }
    }
}
