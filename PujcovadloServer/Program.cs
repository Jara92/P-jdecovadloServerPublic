using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PujcovadloServer.data;
using PujcovadloServer.Facades;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories;
using PujcovadloServer.Repositories.Interfaces;
using PujcovadloServer.Services;
using PujcovadloServer.Services.Interfaces;

//using PujcovadloServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<ItemCategoriesFacade>();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Route controller actions
app.MapControllers();

app.Run();