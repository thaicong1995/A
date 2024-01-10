using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IReveneuRepo
    {
        Revenue ReveneuByShop(int shopId);
    }
}
