using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Sevice.Interface;
using WebApi.Sevice.Service;
using WebApi.VNPAYTOKEN;

namespace WebApi.VNpay
{
    public class VnPayServiceToken 
    {
        private readonly IConfiguration _configuration;
        private readonly MyDb _myDb;
        private readonly VnPayLibraryToken _vnPayLibrary;
        private readonly IDiscountService _iDiscountService;
        private readonly IOrderService _iOrderservice;
        public VnPayServiceToken(IConfiguration configuration, MyDb myDb, VnPayLibraryToken vnPayLibrary, IDiscountService discountService, IOrderService orderservice)
        {
            _configuration = configuration;
            _myDb = myDb;
            _vnPayLibrary = vnPayLibrary;
            _iDiscountService = discountService;
            _iOrderservice = orderservice;
        }

        // Dữ kiệu chưa chưa trả thành công thì Discoint đã lưu ( fix xong)
        public string CreateTokenUrl( int userId, CreateTokenRequest request)
        {
            var pay = new VnPayLibraryToken();

            // Add required parameters to the request
            pay.AddRequestData("vnp_version", request.vnp_version);
            pay.AddRequestData("vnp_command", request.vnp_command);
            pay.AddRequestData("vnp_tmn_code", request.vnp_tmn_code);
            pay.AddRequestData("vnp_app_user_id", userId.ToString());
            pay.AddRequestData("vnp_bank_code", request.vnp_bank_code);
            pay.AddRequestData("vnp_locale", request.vnp_locale);
            pay.AddRequestData("vnp_card_type", request.vnp_card_type);
            pay.AddRequestData("vnp_txn_ref", request.vnp_txn_ref);
            pay.AddRequestData("vnp_txn_desc", request.vnp_txn_desc);
            pay.AddRequestData("vnp_return_url", request.vnp_return_url);
            pay.AddRequestData("vnp_cancel_url", request.vnp_cancel_url);
            pay.AddRequestData("vnp_ip_addr", request.vnp_ip_addr);
            pay.AddRequestData("vnp_create_date", request.vnp_create_date);

            // Generate the secure hash
            var vnpSecureHash = pay.GenerateToken(request.vnp_secure_hash);
            pay.AddRequestData("vnp_secure_hash", vnpSecureHash);

            // Create the request URL without orderNo
            var tokenUrl = pay.CreateRequestUrl(request.vnp_base_url, vnpSecureHash);

            return tokenUrl;
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
                        var product = _myDb.Products.FirstOrDefault(p => p.Id == order.ProductId);

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
                        var selectedCartItems = _myDb.CartItems
                        .Where(cartItem => cartItem.UserId == order.UserId)
                        .ToList();

                        foreach (var cartItem in selectedCartItems)
                        {
                            var order1 = orders.FirstOrDefault(o => o.ProductId == order.ProductId);
                            if (order != null)
                            {
                                cartItem.isSelect = true;
                            }
                        }

                        if (order.DiscountId != 0)
                        {
                            _iDiscountService.SaveDiscountByUserId(order.UserId,(int) order.DiscountId);
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


