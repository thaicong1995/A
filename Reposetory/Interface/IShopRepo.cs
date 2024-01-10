using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IShopRepo
    {
        Shop GetShopByUser(int userId);
        Shop GetShopByID(int Id);
        Shop FindShopByKeyword(string keyword);
        bool IsShopActive(int shopId);
        bool CheckshopByUserId(int userId, int shopId);
    }
}
