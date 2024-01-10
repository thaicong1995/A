using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IOrderRepo
    {
        List<Order> GetOrdersByOrderNo(string orderNo, int userId);
        Order GetProductInOrder(string orderNo, int productId);
        List<OrderDetails> ProductSold(int shopId);
        List<Order> HistoryBuy(int shopId);

    }
}
