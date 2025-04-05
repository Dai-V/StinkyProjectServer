using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StinkyModel;
using StinkyProjectServer.Dto;

namespace StinkyProjectServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController (UserManager<StinkyUser> userManager, JwtHandler jwtHandler) : ControllerBase 
    {
        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(LoginRequest request)
        {   
            StinkyUser user = await userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return Unauthorized("Error: Unknown User");
            }
            bool success = await userManager.CheckPasswordAsync(user,request.Password);
            if (!success)
            {
                return Unauthorized("Error: Wrong Password");
            }
            JwtSecurityToken token = await jwtHandler.GetTokenAsync(user);
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new LoginResult { 
                Success = true,
                Message = "Success",
                Token = tokenString
            });
        }
    }
}
