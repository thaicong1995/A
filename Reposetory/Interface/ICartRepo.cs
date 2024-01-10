using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface ICartRepo
    {
        CartItem GetCart(int productId);
        CartItem GetCartItemByUser(int productId, int userId);
        List<CartItem> GetSelectedCartItemsByUserId(int userId);
        List<CartItem> GetCartByUerId(int userId);
    }
}
