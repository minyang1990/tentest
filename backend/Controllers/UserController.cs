using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JwtDemo.Services;

namespace JwtDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly JwtValidationService _jwtValidationService;

        public UserController()
        {
            _jwtValidationService = new JwtValidationService();
        }
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            Console.WriteLine("[DEBUG] 收到获取用户资料请求");
            Console.WriteLine($"[DEBUG] 请求时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            // 手动验证JWT令牌
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"[DEBUG] Authorization头: {authHeader ?? "未提供"}");
            
            var validationResult = _jwtValidationService.ValidateToken(authHeader);
            
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"[DEBUG] JWT验证失败: {validationResult.ErrorMessage}");
                return Unauthorized(new { 
                    message = "访问被拒绝", 
                    error = validationResult.ErrorMessage 
                });
            }
            
            Console.WriteLine($"[DEBUG] JWT验证成功，从令牌中提取的用户信息:");
            Console.WriteLine($"[DEBUG] - 用户名: {validationResult.Username}");
            Console.WriteLine($"[DEBUG] - 用户ID: {validationResult.UserId}");
            Console.WriteLine($"[DEBUG] - 角色: {validationResult.Role}");

            return Ok(new
            {
                username = validationResult.Username,
                userId = validationResult.UserId,
                role = validationResult.Role,
                message = "这是受保护的用户信息，通过手动JWT验证获取"
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