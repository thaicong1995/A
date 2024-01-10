using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;
using WebApi.Sevice.Interface;
using WebApi.Sevice.Service;

namespace WebApi.Helper.VNpay
{
    public class VnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly MyDb _myDb;
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly IDiscountService _iDiscountService;
        private readonly IOrderService _iOrderservice;
        private readonly IOrderRepo _IOrderRepo;
        private readonly IProductRepo _IProductRepo;
        private readonly ICartRepo _ICartRepo;
        public VnPayService(IConfiguration configuration, MyDb myDb, VnPayLibrary vnPayLibrary,
                                IDiscountService discountService, IOrderService orderservice, IOrderRepo iOrderRepo, IProductRepo product, ICartRepo cartRepo)
        {
            _configuration = configuration;
            _myDb = myDb;
            _vnPayLibrary = vnPayLibrary;
            _iDiscountService = discountService;
            _iOrderservice = orderservice;
            _IOrderRepo = iOrderRepo;
            _IProductRepo = product;
            _ICartRepo = cartRepo;
        }

        // Dữ kiệu chưa chưa trả thành công thì Discoint đã lưu ( fix xong)
        public string CreatePaymentUrl(int userId, string orderNo, HttpContext context, OrderDto orderDto)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();

            var pay = new VnPayLibrary();
            List<Order> orders = _IOrderRepo.GetOrdersByOrderNo(orderNo, userId);

            decimal totalOrderPrice = 0.0m;

            // Check discount tồn tại hay k
            if (orderDto.DiscountId.HasValue)
            {
                // Check discount sử dụng hay chưa
                if (_iDiscountService.IsDiscountUsedByUser(userId, orderDto.DiscountId.Value))
                {
                    throw new InvalidOperationException("User has already used this discount.");
                }
                // gọi hàm sử lý sử dụng discount
                _iOrderservice.CalculateTotalPriceAndDiscount(orders, orderDto, userId);
                totalOrderPrice = orders.Sum(o => o.TotalPrice);
            }
            else
            {
                // gọi hàm sử lý k sử dụng discount
                _iOrderservice.CalculateTotalPrice(orders);
                totalOrderPrice = orders.Sum(o => o.TotalPrice);
            }

            foreach (var order in orders)
            {
                if (order.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Not user");
                }

                order.ShipName = orderDto.ShipName;
                order.ShipAddress = orderDto.ShipAddress;
                order.ShipPhone = orderDto.ShipPhone;
                order.OrderDate = DateTime.Now;
                order.RefunTime = DateTime.Now.AddMinutes(3);
                order.payMethod = PayMethod.VnPay;
            }

            _myDb.SaveChanges();

            if (orders.Count == 0)
            {
                throw new ArgumentException("orders not found---------- .", nameof(orderNo));
            }

            int totalAmount = (int)(totalOrderPrice * 100);
            int discountIdValue = orderDto.DiscountId.HasValue ? orderDto.DiscountId.Value : 0;
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", totalAmount.ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_BankCode", "NCB");
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{orderNo}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }




        public void UpdateOrderAndCartStatus(string orderNo)
        {
            try
            {
                List<Order> orders = _myDb.Orders.Where(o => o.OrderNo == orderNo && o._orderStatus == OrderStatus.WaitPay).ToList();
                Console.WriteLine($" order:--------------------- {orderNo}");

                if (orders.Any())
                {
                    foreach (var order in orders)

                    {
                        var product = _IProductRepo.GetProductByID(order.ProductId);

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

                        if (order._orderStatus == OrderStatus.WaitPay)
                        {
                            order._orderStatus = OrderStatus.Success;
                            order.OrderDate = DateTime.Now;
                            _myDb.Entry(order).State = EntityState.Modified;
                            Console.WriteLine($"Updated order: {order.OrderNo}");
                        }

                        //------
                        _ICartRepo.GetSelectedCartItemsByUserId(order.UserId);
                        if (order.DiscountId != 0 && order.DiscountId != null)
                        {
                            _iDiscountService.SaveDiscountByUserId(order.UserId, (int)order.DiscountId);
                        }
                        _myDb.SaveChanges();

                    }
                }
                else
                {
                    Console.WriteLine("orders not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }

    }
}


