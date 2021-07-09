using System;
using System.Threading.Tasks;
using Dapper;
using Discount.API.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.API.Repositories
{
    class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
        }
        private NpgsqlConnection GetDbConnection()
        {
            var connectionString = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");
            return new(connectionString);
        }


        #region Implementation of IDiscountRepository

        public async Task<Coupon> GetDiscountAsync(string productName)
        {
            await using var connection = GetDbConnection();

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(
                "SELECT * FROM Coupon WHERE ProductName = @ProductName",
                new { ProductName = productName });

            return coupon ??
                   new Coupon()
                   {
                       ProductName = "No Discount",
                       Amount = 0,
                       Description = "No Discount Desc",
                   };
        }

        public async Task<bool> CreateDiscountAsync(Coupon coupon)
        {
            await using var connection = GetDbConnection();

            var affectedRows = await connection.ExecuteAsync(
                "INSERT INTO Coupon (ProductName, Amount, Description) " +
                "VALUES (@ProductName, @Amount ,@Description)",
                //new { coupon }); // TODO: different with the above?
                new {
                    coupon.ProductName,
                    coupon.Amount,
                    coupon.Description,
                });

            return affectedRows == 1;
        }

        public async Task<bool> UpdateDiscountAsync(Coupon coupon)
        {
            await using var connection = GetDbConnection();

            var affectedRows = await connection.ExecuteAsync(
                "UPDATE Coupon " +
                "SET ProductName = @ProductName, Amount = @Amount ,Description = @Description " +
                "WHERE Id = @Id",
                new
                {
                    coupon.Id,
                    coupon.ProductName,
                    coupon.Amount,
                    coupon.Description,
                });
            
            return affectedRows == 1;
        }

        public async Task<bool> DeleteDiscountAsync(string productName)
        {
            await using var connection = GetDbConnection();

            var executeResult = await connection.ExecuteAsync(
                $"DELETE FROM Coupon WHERE ProductName = @productName",
                new
                {
                    ProductName = productName,
                });

            if (executeResult > 1)
            {
                throw new InvalidOperationException(
                    $"Expected to delete only one row while actually deleted: {executeResult}");
            }

            return executeResult == 1;
        }

        #endregion
    }
}