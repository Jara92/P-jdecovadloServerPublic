using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using PujcovadloServer;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Api.Services;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Image;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.AuthorizationHandlers.ItemCategory;
using PujcovadloServer.AuthorizationHandlers.Loan;
using PujcovadloServer.AuthorizationHandlers.PickupProtocol;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Factories.State;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Data;
using PujcovadloServer.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddControllers();

// Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<PujcovadloServerContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
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
    });

// Authorization setup
builder.Services.AddAuthorization(options =>
{
    // Require authenticated users for all requests
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configuration
builder.Services.AddScoped<PujcovadloServerConfiguration>();

// Authentication service
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, ItemAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemHasRoleOwnerAuthorizationHandler>();
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

// Services
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemCategoryService>();
builder.Services.AddScoped<ItemTagService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<PickupProtocolService>();
builder.Services.AddScoped<ReturnProtocolService>();

// Facades
builder.Services.AddScoped<ItemFacade>();
builder.Services.AddScoped<ItemCategoryFacade>();
builder.Services.AddScoped<ImageFacade>();
builder.Services.AddScoped<LoanFacade>();
builder.Services.AddScoped<TenantFacade>();
builder.Services.AddScoped<OwnerFacade>();
builder.Services.AddScoped<PickupProtocolFacade>();
builder.Services.AddScoped<ReturnProtocolFacade>();

// Filters
builder.Services.AddScoped<ExceptionFilter>();

// Response generator
builder.Services.AddScoped<ItemResponseGenerator>();
builder.Services.AddScoped<ItemCategoryResponseGenerator>();
builder.Services.AddScoped<LoanResponseGenerator>();
builder.Services.AddScoped<ImageResponseGenerator>();
builder.Services.AddScoped<PickupProtocolResponseGenerator>();
builder.Services.AddScoped<ReturnProtocolResponseGenerator>();

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


// Route controller actions
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

namespace PujcovadloServer
{
    public partial class Program
    {
    }
}