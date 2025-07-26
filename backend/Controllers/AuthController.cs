using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _jwtKey = "SuperSecureJwtKeyForDemoApplication2024WithAtLeast256BitsLength!@#$%^&*()";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"[DEBUG] 收到登录请求 - 用户名: {request.Username}");
            Console.WriteLine($"[DEBUG] 请求时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            // 简单的用户验证（实际项目中应该查询数据库）
            if (request.Username == "admin" && request.Password == "password")
            {
                Console.WriteLine("[DEBUG] 用户验证成功，开始生成JWT令牌");
                var token = GenerateJwtToken(request.Username);
                Console.WriteLine($"[DEBUG] JWT令牌生成成功，长度: {token.Length}");
                Console.WriteLine($"[DEBUG] 令牌前50个字符: {token.Substring(0, Math.Min(50, token.Length))}...");
                
                return Ok(new { 
                    token = token,
                    message = "登录成功",
                    username = request.Username
                });
            }

            Console.WriteLine("[DEBUG] 用户验证失败 - 用户名或密码错误");
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        private string GenerateJwtToken(string username)
        {
            Console.WriteLine($"[DEBUG] 开始为用户 '{username}' 生成JWT令牌");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);
            Console.WriteLine($"[DEBUG] 密钥长度: {key.Length} 字节");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("userId", "1"),
                    new Claim("role", "admin")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            Console.WriteLine($"[DEBUG] 令牌过期时间: {tokenDescriptor.Expires:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[DEBUG] 添加的Claims: Name={username}, UserId=1, Role=admin");
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            Console.WriteLine($"[DEBUG] JWT令牌生成完成");
            return tokenString;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}