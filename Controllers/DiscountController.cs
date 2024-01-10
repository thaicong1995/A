using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Sevice.Interface;
using WebApi.Sevice.Service;
using WebApi.TokenConfig;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly Token _token;
        private readonly IDiscountService _iDiscountService;
        public DiscountController(IDiscountService discountService, Token token)
        {
            _iDiscountService = discountService;
            _token = token;
        }
        [HttpPost("CreateDiscount")]
        public IActionResult CreateDiscount([FromBody] DiscountDTo discount)
        {
            try
            {
                if (discount == null)
                {
                    return BadRequest("Discount information is missing.");
                }

                _iDiscountService.CreateDiscount(discount);

                return Ok("Discount created successfully");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetAllDiscount")]
        public IActionResult GetAllByIdShop()
        {
            try
            {
                var userClaims = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaims != null && int.TryParse(userClaims.Value, out int userId))
                {
                    var tokenStatus = _token.CheckTokenStatus(userId);

                    if (tokenStatus == StatusToken.Expired)
                    {
                        // Token không còn hợp lệ, từ chối yêu cầu
                        return Unauthorized("Token has expired. Please log in again.");
                    }
                    var list= _iDiscountService.GetDiscounts(userId);

                    if (list != null )
                    {
                        return Ok(list);
                    }
                    else
                    {
                        return NotFound("No have distcount for user.");
                    }

                }
                else
                {
                    return BadRequest("Invalid user ID.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
