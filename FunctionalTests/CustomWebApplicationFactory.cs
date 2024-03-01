using System.Data.Common;
using FunctionalTests.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PujcovadloServer.Data;

namespace FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
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
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<PujcovadloServerContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
                options.UseLazyLoadingProxies();
                options.EnableSensitiveDataLogging();
            });

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
}