using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IDiscountRepo
    {
        Discounts GetbyIdDiscount(int id);
        List<Discounts> GetUnusedDiscountsByUserId(int userId);
    }
}
