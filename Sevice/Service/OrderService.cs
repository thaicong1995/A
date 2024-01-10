
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;
using WebApi.Sevice.Interface;

namespace WebApi.Sevice.Service
{
    public class OrderService : IOrderService
    {
        private readonly MyDb _myDb;
        private readonly IReveneuService _iRevenueService;
        private readonly IDiscountService _iDiscountService;
        private readonly IOrderRepo _IOrderRepo;
        private readonly IProductRepo _IProductRepo;
        private readonly ICartRepo _ICartRepo;
        private readonly IWalletRepo _IWalletRepo;

        public OrderService(MyDb myDb, IReveneuService iRevenueService, IDiscountService iDiscountService,
            IOrderRepo repository, ICartRepo cartRepo, IProductRepo productRepo, IWalletRepo walletRepo)
        {
            _myDb = myDb;
            _IOrderRepo = repository;
            _iRevenueService = iRevenueService;
            _iDiscountService = iDiscountService;
            _IProductRepo = productRepo;
            _ICartRepo = cartRepo;
            _IWalletRepo = walletRepo;
        }

        public string GenerateOrderNo(int userId)
        {
            DateTime now = DateTime.Now;
            string orderNo = $"ORD-{now:yyyyMMddHHss}UID{userId}";

            return orderNo;
        }


        // Loi: Nhung san pham duoc create Oder == true van duoc tao moi khi da thanh toan (xong)
        // nhung san pham == true nhung chua duoc thanh toan. ma khach muon chon san pham khac thi van bi create??

        // cach 2. Tren front-end. se click chon. Nhung san pham duoc chon thi moi tao order. khi thanh toan xong trang thai sp da Pay in cart == true.
        // Get cart = cach goi nhun san pham == false de nguoi dung tiep tuc
        public List<Order> CreateOrders(List<int> selectedProductIds, int userId)
        {
            try
            {
                string orderNo = GenerateOrderNo(userId);
                List<Order> orders = new List<Order>();

                foreach (var productId in selectedProductIds)
                {
                    // Kiểm tra xem sản phẩm đã được chọn trong giỏ hàng hay chưa
                    CartItem cartItem = _ICartRepo.GetCartItemByUser(productId, userId);
                    if (cartItem != null)
                    {
                        // Kiểm tra số lượng sản phẩm có đủ không
                        Product product = _IProductRepo.GetProductById(productId);

                        if (product != null && product.Quantity >= cartItem.Quantity && product._productStatus == ProductStatus.InOfStock)
                        {
                            Order newOrder = new Order
                            {
                                ShipName = null,
                                ShipAddress = null,
                                ShipPhone = null,
                                ProductId = cartItem.ProductId,
                                ProductName = cartItem.ProductName,
                                Price = cartItem.Price,
                                Quantity = cartItem.Quantity,
                                PriceQuantity = cartItem.PriceQuantity,
                                TotalPrice = cartItem.PriceQuantity,
                                UserId = cartItem.UserId,
                                ShopId = cartItem.ShopId,
                                OrderNo = orderNo,

                                OrderDate = DateTime.Now,
                                _orderStatus = OrderStatus.WaitPay
                            };

                            orders.Add(newOrder);

                        }
                        else
                        {
                            throw new InvalidOperationException($"Not enough quantity for product : {product.ProductName}");
                        }
                    }
                }

                if (orders.Count == 0)
                {
                    throw new ArgumentNullException("No selected items in the cart.");
                }

                _myDb.Orders.AddRange(orders);
                _myDb.SaveChanges();

                return orders;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }


        // Nếu xử lý đơn hàng fail.
        // Dùng thuật cắm cờ để biểu diễn trạng thái và gán status (lười chưa làm)
        public List<Order> PayOrder(string OrderNo, OrderDto orderDto, int userId)
        {

            try
            {
                if (string.IsNullOrEmpty(orderDto.ShipName) || string.IsNullOrEmpty(orderDto.ShipAddress) || string.IsNullOrEmpty(orderDto.ShipPhone))
                {
                    throw new ArgumentException("Please fill in all information completely");
                }

                List<Order> orders = _IOrderRepo.GetOrdersByOrderNo(OrderNo, userId);
                var selectedCartItems = _ICartRepo.GetSelectedCartItemsByUserId(userId);

                var wallet = _IWalletRepo.GetWalletByUserId(userId);
                CheckOrderStatus(orders, userId);

                // Check discount tồn tại hay k
                ProcessOrderDiscount(orders, userId, orderDto.DiscountId, orderDto);
                decimal totalOrderPrice = 0.0m;
                if (wallet == null)
                {
                    throw new InvalidOperationException("User's wallet not found.");
                }

                // xử lý trừ quantity của product khi order
                foreach (var order in orders)
                {
                    order.ShipName = orderDto.ShipName;
                    order.ShipAddress = orderDto.ShipAddress;
                    order.ShipPhone = orderDto.ShipPhone;
                    order.OrderDate = DateTime.Now;
                    order.RefunTime = DateTime.Now.AddMinutes(3);
                    order.payMethod = PayMethod.Wallet;

                    UpdatQuantityProduct(order);

                    order._orderStatus = OrderStatus.Success;

                    UpdateWalletBalance(order, wallet);

                    foreach (var cartItem in selectedCartItems.Where(c => c.ProductId == order.ProductId))
                    {
                        cartItem.isSelect = true;
                    }

                }

                if (wallet != null && orderDto.DiscountId.HasValue)
                {
                    _iDiscountService.SaveDiscountByUserId(userId, orderDto.DiscountId.Value);
                }

                _myDb.SaveChanges();

                return orders;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        //check tinh dung dan cua Order
        private void CheckOrderStatus(List<Order> orders, int userId)
        {
            if (orders == null || orders.Count == 0)
            {
                throw new ArgumentException("Order not found!");
            }

            foreach (var order in orders)
            {
                if (order.UserId != userId)
                {
                    throw new UnauthorizedAccessException("There is no payment authorization for this order!");
                }

                if (order._orderStatus == OrderStatus.Success)
                {
                    throw new InvalidOperationException("The order has been paid!");
                }
            }
        }

        //Trừ số lượng sau khi thanh toán
        private void UpdatQuantityProduct(Order order)
        {

            var product = _IProductRepo.GetProductById(order.ProductId);

            if (product != null)
            {

                if (order._orderStatus == OrderStatus.WaitPay)
                {
                    product.Quantity -= order.Quantity;

                    if (product.Quantity == 0)
                    {
                        product._productStatus = ProductStatus.OutOfStock;
                    }
                }

            }
        }


        // Xử lý dícount
        private void ProcessOrderDiscount(List<Order> orders, int userId, int? discountId, OrderDto orderDto)
        {
            if (discountId.HasValue)
            {
                // Check discount sử dụng hay chưa
                if (_iDiscountService.IsDiscountUsedByUser(userId, discountId.Value))
                {
                    throw new InvalidOperationException("User has already used this discount.");
                }
                // gọi hàm sử lý sử dụng discount
                CalculateTotalPriceAndDiscount(orders, orderDto, userId);
            }
            else
            {
                // gọi hàm sử lý k sử dụng discount
                CalculateTotalPrice(orders);
            }
        }


        // check va tru wallet khi co va khong co discount
        public void UpdateWalletBalance(Order order, Wallet wallet)
        {
            decimal totalPrice = order.TotalPrice;

            // Check and deduct based on discount
            if (order.DiscountId.HasValue)
            {
                totalPrice = _iDiscountService.ApplyDiscount(order, order.DiscountId.Value, order.UserId);
            }

            // Check if there's enough money in the wallet
            if (wallet.Monney < totalPrice)
            {
                throw new InvalidOperationException("Not enough money in the wallet.");
            }

            // Deduct the amount from the wallet
            wallet.Monney -= totalPrice;
        }



        // Tính tổng giá trị của đơn hàng với giảm giá
        public void CalculateTotalPriceAndDiscount(List<Order> orders, OrderDto orderDto, int userId)
        {
            foreach (var order in orders)
            {
                decimal discountAmount = _iDiscountService.ApplyDiscount(order, orderDto.DiscountId.Value, userId);
                order.DiscountId = orderDto.DiscountId;
                order.TotalPrice = order.Price * order.Quantity - discountAmount;
            }
        }



        // Tính tổng giá trị của đơn hàng không có giảm giá
        public void CalculateTotalPrice(List<Order> orders)
        {
            foreach (var order in orders)
            {
                order.DiscountId = null;
                order.TotalPrice = order.Price * order.Quantity;
            }
        }



        //Sản phẩm k có trong order - refun (fix xong)
        //Cùng sản phẩm khác order nhưng đã bị refun, Order hiện tại k refun được (fix xong)
        // K refun được nhiều sản phẩm trong 1 order (fix xong)
        public Order RefundProduct(string OrderNo, int userId, int productId)
        {
            try
            {
                DateTime currentTime = DateTime.Now;
                // Kiểm tra xem sản phẩm có tồn tại trong đơn hàng không
                var productInOrder = _IOrderRepo.GetProductInOrder(OrderNo, productId);

                if (productInOrder == null)
                {
                    throw new InvalidOperationException("The product does not exist in this order.!!");
                }

                if (productInOrder._orderStatus == OrderStatus.Refunded)
                {
                    throw new InvalidOperationException("Refund Success!.");
                }

                if (currentTime > productInOrder.RefunTime)
                {
                    throw new InvalidOperationException("The refund time has passed, and you cannot return this product.");
                }
                // Thực hiện hoàn trả cho sản phẩm
                var product = _IProductRepo.GetProductById(productId);

                if (product != null)
                {
                    product.Quantity += productInOrder.Quantity;

                    if (product._productStatus == ProductStatus.OutOfStock)
                    {
                        product._productStatus = ProductStatus.InOfStock;
                    }
                }

                // Cập nhật trạng thái của sản phẩm
                productInOrder._orderStatus = OrderStatus.Refunded;

                // Cập nhật ví 
                var wallet = _IWalletRepo.GetWalletByUserId(userId);
                if (wallet != null)
                {
                    wallet.Monney += productInOrder.TotalPrice;
                }

                _myDb.SaveChanges();

                return productInOrder;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        public List<Order> HistoryBuy(int userId)
        {
            try
            {
                var history = _IOrderRepo.HistoryBuy(userId);

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }
    }
}

