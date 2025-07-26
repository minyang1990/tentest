using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst("role")?.Value;

            return Ok(new
            {
                username = username,
                userId = userId,
                role = role,
                message = "这是受保护的用户信息，需要有效的JWT令牌才能访问"
            });
        }

        [HttpGet("data")]
        public IActionResult GetSecureData()
        {
            return Ok(new
            {
                data = new[]
                {
                    new { id = 1, name = "机密数据1", value = "重要信息A" },
                    new { id = 2, name = "机密数据2", value = "重要信息B" },
                    new { id = 3, name = "机密数据3", value = "重要信息C" }
                },
                message = "这些是需要认证才能获取的敏感数据"
            });
        }
    }
}