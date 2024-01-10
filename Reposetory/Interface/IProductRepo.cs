using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IProductRepo
    {
        Product GetProductById(int productId);
        List<Product> FindProductsByKeyword(string keyword);
        Product GetProductByID(int productId);
        List<Product> GetProductsByShop(int shopId);
    }
}
