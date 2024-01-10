using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class OrderRepo : IOrderRepo
    {
        private readonly MyDb _myDb;
        public OrderRepo (MyDb myDb)
        {
            _myDb = myDb;
        }

        public List<Order> GetOrdersByOrderNo(string orderNo, int userId)
        {
            return _myDb.Orders.Where(o => o.UserId == userId && o.OrderNo == orderNo && o._orderStatus == OrderStatus.WaitPay).ToList();
        }

        public Order GetProductInOrder(string orderNo, int productId)
        {
            return _myDb.Orders.FirstOrDefault(o => o.OrderNo == orderNo && o.ProductId == productId);
        }

        public List<Order> HistoryBuy(int userId)
        {
            return _myDb.Orders
            .Where(order => order.UserId == userId && order._orderStatus == OrderStatus.Success)
            .ToList();
        }

        public List<OrderDetails> ProductSold(int shopId)
        {
            return _myDb.Orders
                        .Where(o => o.ShopId == shopId && o._orderStatus == OrderStatus.Success)
                        .Select(o => new OrderDetails
                        {
                            OrderNo = o.OrderNo,
                            Id = o.ProductId,
                            ProductName = o.ProductName,
                            PriceQuantity = o.PriceQuantity,
                            Quantity = o.Quantity
                        })
                        .ToList();

        }
    }
}
