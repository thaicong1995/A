
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X9;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.TokenConfig;
using WebApi.VNPAYTOKEN;

namespace WebApi.VNpay
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnPayControllerToken : ControllerBase
    {
        private readonly VnPayServiceToken _vnPayService;
        private readonly MyDb _myDb;
        private readonly Token _token;

        [HttpGet("token")]
        public IActionResult CreatePaymentToken([FromQuery] CreateTokenRequest request)
        {
            try
            {
                if (HttpContext.User != null)
                {
                    var userClaims = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (userClaims != null && int.TryParse(userClaims.Value, out int userID))
                    {
                        var tokenStatus = _token.CheckTokenStatus(userID);

                        if (tokenStatus == StatusToken.Expired)
                        {
                            return Unauthorized("The token is no longer valid");
                        }

                        // Assuming orderNo and orderDto are available in your controller
                        var paymentUrl = _vnPayService.CreateTokenUrl(userID, request);

                        return Ok(new { paymentUrl });
                    }
                }

                // Handle the case when HttpContext.User is null or userClaims parsing fails
                return Unauthorized("Invalid user ID claims");
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { error = "Unable to create payment token", message = ex.Message });
            }
        }



    }
}
