using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PujcovadloServer.data;
using PujcovadloServer.Facades;
using PujcovadloServer.Repositories;

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

builder.Services.AddScoped<ItemsFacade>();
builder.Services.AddScoped<ItemsRepository>();

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