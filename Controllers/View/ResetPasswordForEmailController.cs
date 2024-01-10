using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers.View
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetPasswordForEmailController : Controller
    {
        private readonly ResetService _resetService;
        public ResetPasswordForEmailController(ResetService resetService)
        {
            _resetService = resetService;
        }
        [HttpGet("Reset-Password")]
        public IActionResult ResetPassword([FromQuery] string token)
        {
            var user = _resetService.GetUserByActivationToken(token);

            if (user != null)
            {
                if (user.ExpLink == null || user.ExpLink > DateTime.Now)
                {
                    return View("ResetPassword");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Liên kết đặt lại mật khẩu đã hết hạn.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Mã thông báo đặt lại không hợp lệ.";
            }
            return View("ResetPassword");
        }


        [HttpPost("Reset-Password")]
        public IActionResult UpdatePassword([FromForm] string newPassword, [FromQuery] string token)
        {
            if (_resetService.IsValidResetToken(token))
            {
                var user = _resetService.GetUserByActivationToken(token);

                if (user != null)
                {
                    bool passwordUpdated = _resetService.UpdatePassword(user.Id, newPassword);

                    if (passwordUpdated)
                    {
                        ViewBag.SuccessMessage = "Mật khẩu đã được đổi thành công.";
                        // Trả về trang ResetPassword với thông báo thành công
                        return View("ResetPassword");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "The reset link has expired.";
                        return View("PPP");
                    }
                }
            }

            return View("ResetPassword");
        }

    }
}


