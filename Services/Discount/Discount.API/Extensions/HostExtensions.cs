using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> MigrateDatabaseAsync<TContext>(this IHost host)
        {
            return await MigrateDatabaseAsync<TContext>(host, 50);
        }
        private static async Task<IHost> MigrateDatabaseAsync<TContext>(this IHost host, int retriesLeft)
        {
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating PostgreSQL...");

                var connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString");
                await using NpgsqlConnection connection = new(connectionString);

                await connection.OpenAsync();

                await using NpgsqlCommand command = new()
                {
                    Connection = connection,
                    CommandText = "DROP TABLE IF EXISTS Coupon",
                };

                await command.ExecuteNonQueryAsync();

                command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                await command.ExecuteNonQueryAsync();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                await command.ExecuteNonQueryAsync();

                command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                await command.ExecuteNonQueryAsync();

                logger.LogInformation("Migrating PostgreSQL...Done!");

            }
            catch (Exception e)
            {
                if (retriesLeft > 0)
                {
                    logger.LogError($"Migrating had error: {e}. Delay before retry...");

                    await Task.Delay(TimeSpan.FromSeconds(2));
                    
                    logger.LogDebug($"Before Retry (Retries left: {retriesLeft})");

                    return await MigrateDatabaseAsync<TContext>(host, retriesLeft - 1);
                }

                logger.LogCritical($"Migrating had error: {e}. retries done!");

            }

            return host;
        }
    }
}