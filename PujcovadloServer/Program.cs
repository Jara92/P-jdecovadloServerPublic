using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Data;
using PujcovadloServer.Data.Repositories;
using NSwag;

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

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();

builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemCategoryService>();

builder.Services.AddScoped<ItemFacade>();
builder.Services.AddScoped<ItemCategoryFacade>();

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
    // Add OpenAPI 3.0 document serving middleware
    // Available at: http://localhost:<port>/swagger/v1/swagger.json
    app.UseOpenApi();
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Route controller actions
app.MapControllers();

app.Run();

namespace PujcovadloServer
{
    public partial class Program {}
}