using WebApi.Models;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class CartRepo : ICartRepo
    {

        private readonly MyDb _myDb;

        public CartRepo(MyDb myDb)
        {
            _myDb = myDb;
        }
        public CartItem GetCart(int productId)
        {
            return _myDb.CartItems.FirstOrDefault(c => c.ProductId == productId && !c.isSelect);
        }

        public List<CartItem> GetCartByUerId(int userId)
        {
            return _myDb.CartItems.Where(c => c.UserId == userId && !c.isSelect).ToList();
        }

        public CartItem GetCartItemByUser(int productId, int userId)
        {
            return _myDb.CartItems.FirstOrDefault(c => c.ProductId == productId && c.UserId == userId && !c.isSelect);
        }

        public List<CartItem> GetSelectedCartItemsByUserId(int userId)
        {
            return _myDb.CartItems.Where(cartItem => cartItem.UserId == userId).ToList(); ;
        }


    }
}
