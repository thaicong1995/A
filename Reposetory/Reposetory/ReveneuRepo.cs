using WebApi.Models;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class ReveneuRepo : IReveneuRepo
    {
        private readonly MyDb _myDb;

        public ReveneuRepo(MyDb myDb)
        {
            _myDb = myDb;
        }

        public Revenue ReveneuByShop(int shopId)
        {
            return _myDb.Revenues.FirstOrDefault(r => r.ShopId == shopId);
        }
    }
}
