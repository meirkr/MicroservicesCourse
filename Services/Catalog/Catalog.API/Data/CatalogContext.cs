using Catalog.API.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    class CatalogContext : ICatalogContext
    {
        #region Implementation of ICatalogContext

        public IMongoCollection<Product> ProductsCollection { get; }

        #endregion

        public CatalogContext(IConfiguration config)
        {
            var conString = config.GetValue<string>("DatabaseSettings.ConnectionString");
            var dvNAme = config.GetValue<string>("DatabaseSettings.DatabaseName");
            var collectionName = config.GetValue<string>("DatabaseSettings.CollectionName");
            var client = new MongoClient(conString);
            var database = client.GetDatabase(dvNAme);
            
            ProductsCollection = database.GetCollection<Product>(collectionName);
            CatalogContextSeed.SeedData(ProductsCollection);
        }
    }
}