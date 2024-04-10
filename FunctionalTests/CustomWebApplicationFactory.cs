using System.Data.Common;
using FunctionalTests.Helpers;
using FunctionalTests.Mocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Data;

namespace FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private IConfiguration? _config;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IConfiguration>(Configuration);

            // Remove the app's ApplicationDbContext registration.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<PujcovadloServerContext>));

            services.Remove(dbContextDescriptor);

            // Remove the app's DbConnection registration.
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            /*services.AddSingleton<DbConnection>(container =>
            {
                //var connection = new SqliteConnection("DataSource=:memory:");
                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                var connection = new NpgsqlConnection(Configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                /*var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                dataSourceBuilder.UseNetTopologySuite();
                var connection = dataSourceBuilder.Build();
                connection.OpenConnection();#1#

                return connection;
            });*/

            services.AddDbContext<PujcovadloServerContext>((container, options) =>
            {
                //var connection = container.GetRequiredService<DbConnection>();
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                //var connection = new NpgsqlConnection(Configuration.GetConnectionString("DefaultConnection"));

                options.UseNpgsql(connectionString, x =>
                {
                    //x.MigrationsAssembly(typeof(PujcovadloServerContext).Assembly.FullName);
                    //x.MigrationsHistoryTable(schemaService.MigrationsTableName, schemaService.Name);
                    x.UseNetTopologySuite();
                });
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.UseLazyLoadingProxies();
                options.EnableSensitiveDataLogging();
            });

            // Remove the apps filestorage 
            var fileStorage = services.SingleOrDefault(
                d => d.ServiceType == typeof(IFileStorage));

            services.Remove(fileStorage);

            // Add filestorage mock
            services.AddScoped<IFileStorage, MockFileStorage>();

            // Add FakeJwtBearer
            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = TestJwtTokenProvider.Issuer,
                    };
                    // ValidIssuer and ValidAudience is not required, but it helps to define them as otherwise they can be overriden by for example the `user-jwts` tool which will cause the validation to fail
                    options.TokenValidationParameters.ValidIssuer = TestJwtTokenProvider.Issuer;
                    options.TokenValidationParameters.ValidAudience = TestJwtTokenProvider.Issuer;
                    options.Configuration.SigningKeys.Add(TestJwtTokenProvider.SecurityKey);
                }
            );
        });

        builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole(options => options.IncludeScopes = true);
        });

        builder.UseEnvironment("Development");
    }

    public IConfiguration Configuration
    {
        get
        {
            if (_config == null)
            {
                var builder = new ConfigurationBuilder().AddJsonFile($"testappsettings.json", optional: false);
                _config = builder.Build();
            }

            return _config;
        }
    }
}