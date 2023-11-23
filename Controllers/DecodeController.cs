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
        

        if (userIdClaim == null)
        {
            return BadRequest("User information is incomplete in the token.");
        }

        int userId = int.Parse(userIdClaim.Value);
        

        // Tạo đối tượng người dùng từ thông tin trong token
        User user = new User
        {
            Id = userId,
           
        };

        return Ok(user);
    }

}
