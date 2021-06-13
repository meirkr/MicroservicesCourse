using System;
using System.Threading.Tasks;
using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Basket.API.Repositories
{
    class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            _redisCache = redisCache ?? throw new ArgumentException(nameof(redisCache));
        }

        #region Implementation of IBasketRepository

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var resString = await _redisCache.GetStringAsync(userName);
            if (string.IsNullOrEmpty(resString))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ShoppingCart>(resString);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart shoppingCart)
        {
            await _redisCache.SetStringAsync(shoppingCart.UserName, 
                JsonConvert.SerializeObject(shoppingCart));

            return await GetBasket(shoppingCart.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
        }

        #endregion
    }
}