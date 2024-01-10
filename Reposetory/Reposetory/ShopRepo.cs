using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class ShopRepo : IShopRepo
    {
        private readonly MyDb _myDb;

        public ShopRepo(MyDb myDb)
        {
            _myDb = myDb;
        }
        public Shop GetShopByUser(int userId)
        {
            return _myDb.Shops.FirstOrDefault(s => s.UserId == userId);
        }

        public Shop GetShopByID(int ShopId)
        {
            return _myDb.Shops.FirstOrDefault(s => s.Id == ShopId);
        }

        public Shop FindShopByKeyword(string keyword)
        {
            return _myDb.Shops.FirstOrDefault(shop => shop.ShopName.ToLower() == keyword);
        }

        public bool IsShopActive(int shopId)
        {
            var shop = _myDb.Shops.FirstOrDefault(s => s.Id == shopId);

            return shop != null && shop._shopStatus == ShopStatus.Active;
        }

        public bool CheckshopByUserId(int userId, int shopId)
        {
            return _myDb.Shops.Any(s => s.UserId == userId && s.Id == shopId);
        }
    }
}
