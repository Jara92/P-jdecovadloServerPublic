using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;
using NSwag;
using PujcovadloServer;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Image;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.ItemCategory;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.AuthorizationHandlers.PickupProtocol;
using PujcovadloServer.AuthorizationHandlers.Profile;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Factories.State;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Data;
using PujcovadloServer.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey"]);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "Půjčovadlo.cz API",
            Description = "API pro půjčovadlo.cz",
            TermsOfService = "https://pujcovadlo.cz/terms",
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = "https://example.com/contact"
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = "https://example.com/license"
            }
        };
    };
});

// Use controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Keep first letter of properties as uppercase
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    /*.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    })*/;

// Add support for razor pages
builder.Services.AddRazorPages();

// Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEntityFrameworkNpgsql().AddDbContext<PujcovadloServerContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}
// Production environment
else
{
    /* builder.Services.AddDbContext<PujcovadloServerContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionMvcMovieContext")));*/
}

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PujcovadloServerContext>()
    .AddDefaultTokenProviders();

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
// .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    options.User.RequireUniqueEmail = true;
});


builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    options.LoginPath = "/admin/login";
    options.AccessDeniedPath = "/401";
    options.SlidingExpiration = true;
});

// Adding Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    // Adding Jwt Bearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    }).AddCookie("Admin", options =>
    {
        options.Cookie.Name = "PujcovadloCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/admin/login"; // Cesta k přihlašovací stránce
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/403";
    });

// Minio object storage configuration
builder.Services.AddMinio(configureClient => configureClient
    .WithSSL(builder.Configuration.GetValue<bool>("Minio:UseSSL"))
    .WithEndpoint(builder.Configuration["Minio:Endpoint"])
    .WithCredentials(builder.Configuration["Minio:AccessKey"], builder.Configuration["Minio:SecretKey"])
);

// Authorization setup
builder.Services.AddAuthorization(options =>
{
    // Require authenticated users for all requests
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "cs-CZ", "en-US" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    options.DefaultRequestCulture = new RequestCulture(culture: "cs-CZ", uiCulture: "cs-CZ");
});

// Support for flash messages
builder.Services.AddFlashes();

builder.Services.AddMvc()
    // Add support for finding localized views, based on file name suffix, e.g. Index.fr.cshtml
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    // Add support for localizing strings in data annotations (e.g. validation messages) via the
    // IStringLocalizer abstractions.
    /*.AddDataAnnotationsLocalization(options => {
        // Shared resource is used for data annotations localization
        // FIXME: this is not working with class based localization resources
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    })*/
    // Support for class based localization resources
    .AddDataAnnotationsLocalization();

// FileStorage
builder.Services.AddScoped<IFileStorage, MinioFileStorage>();

// Configuration
builder.Services.AddScoped<PujcovadloServerConfiguration>();

// Authentication service
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, ItemAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemGuestAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, ImageAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ImageOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ImageGuestAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ImageTenantAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, ItemCategoryAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemCategoryGuestAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, LoanAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, LoanOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, LoanTenantAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, PickupProtocolAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PickupProtocolOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PickupProtocolTenantAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, ReturnProtocolAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReturnProtocolOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ReturnProtocolTenantAuthorizationHandler>();

builder.Services.AddScoped<IAuthorizationHandler, ProfileAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProfileOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProfileGuestAuthorizationHandler>();

//Factories
builder.Services.AddScoped<LoanStateFactory>();

// Repositories
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
builder.Services.AddScoped<IItemTagRepository, ItemTagRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IPickupProtocolRepository, PickupProtocolRepository>();
builder.Services.AddScoped<IReturnProtocolRepository, ReturnProtocolRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

// Services
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemCategoryService>();
builder.Services.AddScoped<ItemTagService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<PickupProtocolService>();
builder.Services.AddScoped<ReturnProtocolService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ApplicationUserService>();

// Facades
builder.Services.AddScoped<ItemFacade>();
builder.Services.AddScoped<ItemCategoryFacade>();
builder.Services.AddScoped<ImageFacade>();
builder.Services.AddScoped<LoanFacade>();
builder.Services.AddScoped<TenantFacade>();
builder.Services.AddScoped<OwnerFacade>();
builder.Services.AddScoped<PickupProtocolFacade>();
builder.Services.AddScoped<ReturnProtocolFacade>();
builder.Services.AddScoped<ReviewFacade>();
builder.Services.AddScoped<ProfileFacade>();

// Admin facades
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.ItemFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.ItemCategoryFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.ItemTagFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.LoanFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.PickupProtocolFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.ReturnProtocolFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.UserFacade>();
builder.Services.AddScoped<PujcovadloServer.Areas.Admin.Business.Facades.ReviewFacade>();

// Filters
builder.Services.AddScoped<ExceptionFilter>();

// Response generator
builder.Services.AddScoped<ItemResponseGenerator>();
builder.Services.AddScoped<ItemCategoryResponseGenerator>();
builder.Services.AddScoped<ItemTagResponseGenerator>();
builder.Services.AddScoped<LoanResponseGenerator>();
builder.Services.AddScoped<ImageResponseGenerator>();
builder.Services.AddScoped<PickupProtocolResponseGenerator>();
builder.Services.AddScoped<ReturnProtocolResponseGenerator>();
builder.Services.AddScoped<ReviewResponseGenerator>();
builder.Services.AddScoped<ProfileResponseGenerator>();

// File upload service
builder.Services.AddScoped<FileUploadService>();

// Custom authorization service
builder.Services.AddScoped<AuthorizationService>();

// AutoMapper configuration
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
    cfg.AllowNullCollections = true;
});
//config.AssertConfigurationIsValid();
builder.Services.AddSingleton<IMapper>(config.CreateMapper());

var app = builder.Build();

var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();

    // Add OpenAPI 3.0 document serving middleware
    // Available at: http://localhost:<port>/swagger/v1/swagger.json
    app.UseOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Define routes for admin area
app.MapControllerRoute(
    name: "Admin",
    pattern: "admin/{controller=Item}/{action=Index}/{id?}");

// Define routes for api area
app.MapControllerRoute(
    name: "Api",
    pattern: "api/{controller=Item}/{action=Index}/{id?}");

// Route controller actions
//app.MapControllers();
//app.MapRazorPages();

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
};

app.UseCookiePolicy(cookiePolicyOptions);

// Add static files to the request pipeline (see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-8.0)
// https://stackoverflow.com/questions/60433142/how-to-use-css-files-or-js-in-area-on-asp-net-core
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Areas/Admin/wwwroot")),
    RequestPath = "/assets/admin"
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();

namespace PujcovadloServer
{
    // Make the implicit Program class public so test projects can access it
    public partial class Program
    {
    }
}