using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IRepository
    {
        public User GetUserById(int userId);
        User GetUserByActivationToken(string activationToken);
        User GetUserByEmail(string email);
        AcessToken GetValidTokenByUserId(int userId);
        Shop GetShopByUser(int userId);
        Shop GetShopByID(int Id);
        Shop FindShopByKeyword(string keyword);
        bool IsShopActive(int shopId);
        Revenue ReveneuByShop(int shopId);
        List<Product> FindProductsByKeyword(string keyword);
        Product GetProductByID(int productId);
        List<Product> GetProductsByShop(int shopId);
        CartItem GetCart(int productId);
        CartItem GetCartItemByUser(int productId, int userId);
        List<CartItem> GetSelectedCartItemsByUserId(int userId);
        Product GetProductById(int productId);
        List<Order> GetOrdersByOrderNo(string orderNo, int userId);
        Order GetProductInOrder(string orderNo, int productId);
        Wallet GetWalletByUserId(int userId);
    }
}
