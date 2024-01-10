using WebApi.Models;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class ProductRepo : IProductRepo
    {
        private readonly MyDb _myDb;
        public ProductRepo(MyDb myDb)
        {
            _myDb = myDb;
        }

        public List<Product> FindProductsByKeyword(string keyword)
        {
            return _myDb.Products.Where(product => product.ProductName.ToLower().Contains(keyword)).ToList();
        }

        public Product GetProductByID(int productId)
        {
            return _myDb.Products.FirstOrDefault(s => s.Id == productId);
        }

        public Product GetProductById(int productId)
        {
            return _myDb.Products.FirstOrDefault(p => p.Id == productId);
        }


        public List<Product> GetProductsByShop(int shopId)
        {
            return _myDb.Products.Where(p => p.ShopId == shopId).ToList();
        }


    }
}
