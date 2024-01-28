using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Data;
using PujcovadloServer.Data.Repositories;
using NSwag;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.Authentication;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApiDocument(options => {
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
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
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

// Authentication service
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

// Authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, ItemAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ItemCategoryAuthorizationHandler>();

// Repositories
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();

// Services
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemCategoryService>();

// Facades
builder.Services.AddScoped<ItemFacade>();
builder.Services.AddScoped<ItemCategoryFacade>();

// Filters
builder.Services.AddScoped<ExceptionFilter>();

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
   // app.UseDeveloperExceptionPage();
    
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
    public partial class Program {}
}