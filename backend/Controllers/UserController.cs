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
            Console.WriteLine("[DEBUG] 收到获取用户资料请求");
            Console.WriteLine($"[DEBUG] 请求时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst("userId")?.Value;
            var role = User.FindFirst("role")?.Value;
            
            Console.WriteLine($"[DEBUG] 从JWT令牌中提取的用户信息:");
            Console.WriteLine($"[DEBUG] - 用户名: {username}");
            Console.WriteLine($"[DEBUG] - 用户ID: {userId}");
            Console.WriteLine($"[DEBUG] - 角色: {role}");

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
            Console.WriteLine("[DEBUG] 收到获取机密数据请求");
            Console.WriteLine($"[DEBUG] 请求时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine($"[DEBUG] 当前认证用户: {username}");
            
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