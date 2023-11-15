using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using WebApi.Sevice.Service;
using WebApi.TokenConfig;
using WebApi.Models;

[Route("api")]
[ApiController]
public class DecodeController : ControllerBase
{
    
    private readonly Token _token;

    public DecodeController( Token token)
    {
        
        _token = token;
    }


    [HttpGet("decode")]
    public ActionResult<User> DecodeToken(string token)
    {
        var principal = _token.DecodeToken(token);
        if (principal == null)
        {
            return BadRequest("Invalid token");
        }

        // Lấy thông tin người dùng từ các claim
        var userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        var emailClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        var nameClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        var phoneClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone);
        var addressClaim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.StreetAddress);

        if (userIdClaim == null || emailClaim == null || nameClaim == null || phoneClaim == null || addressClaim == null)
        {
            return BadRequest("User information is incomplete in the token.");
        }

        int userId = int.Parse(userIdClaim.Value);
        string email = emailClaim.Value;
        string name = nameClaim.Value;
        string phone = phoneClaim.Value;
        string address = addressClaim.Value;

        // Tạo đối tượng người dùng từ thông tin trong token
        User user = new User
        {
            Id = userId,
            Email = email,
            Name = name,
            Phone = phone,
            Address = address
        };

        return Ok(user);
    }

}
