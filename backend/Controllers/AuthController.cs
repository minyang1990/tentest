using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

        /// <summary>
        /// 手写JWT令牌生成 - 完全不使用JWT库
        /// </summary>
        private string GenerateJwtToken(string username)
        {
            Console.WriteLine($"[DEBUG] === 开始手写JWT令牌生成过程 ===");
            Console.WriteLine($"[DEBUG] 目标用户: {username}");
            
            // 步骤1: 创建JWT Header
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };
            
            var headerJson = JsonSerializer.Serialize(header);
            Console.WriteLine($"[DEBUG] 1. Header JSON: {headerJson}");
            
            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            Console.WriteLine($"[DEBUG] 1. Header Base64URL: {headerBase64}");
            
            // 步骤2: 创建JWT Payload
            var currentTime = DateTimeOffset.UtcNow;
            var expirationTime = currentTime.AddHours(1);
            
            var payload = new
            {
                name = username,
                userId = "1",
                role = "admin",
                iat = currentTime.ToUnixTimeSeconds(),  // 签发时间
                exp = expirationTime.ToUnixTimeSeconds() // 过期时间
            };
            
            var payloadJson = JsonSerializer.Serialize(payload);
            Console.WriteLine($"[DEBUG] 2. Payload JSON: {payloadJson}");
            Console.WriteLine($"[DEBUG] 2. 签发时间: {currentTime:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"[DEBUG] 2. 过期时间: {expirationTime:yyyy-MM-dd HH:mm:ss} UTC");
            
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
            Console.WriteLine($"[DEBUG] 2. Payload Base64URL: {payloadBase64}");
            
            // 步骤3: 创建待签名数据
            var dataToSign = $"{headerBase64}.{payloadBase64}";
            Console.WriteLine($"[DEBUG] 3. 待签名数据: {dataToSign}");
            Console.WriteLine($"[DEBUG] 3. 待签名数据长度: {dataToSign.Length} 字符");
            
            // 步骤4: 生成签名
            var signature = GenerateSignature(dataToSign);
            Console.WriteLine($"[DEBUG] 4. 生成的签名: {signature}");
            
            // 步骤5: 组装完整的JWT
            var jwt = $"{headerBase64}.{payloadBase64}.{signature}";
            Console.WriteLine($"[DEBUG] 5. 完整JWT令牌长度: {jwt.Length} 字符");
            Console.WriteLine($"[DEBUG] 5. JWT结构验证:");
            Console.WriteLine($"[DEBUG]    - Header部分长度: {headerBase64.Length}");
            Console.WriteLine($"[DEBUG]    - Payload部分长度: {payloadBase64.Length}");
            Console.WriteLine($"[DEBUG]    - Signature部分长度: {signature.Length}");
            Console.WriteLine($"[DEBUG] === JWT令牌生成完成 ===");
            
            return jwt;
        }
        
        /// <summary>
        /// 手写HMAC-SHA256签名生成
        /// </summary>
        private string GenerateSignature(string dataToSign)
        {
            Console.WriteLine($"[DEBUG] --- 开始生成HMAC-SHA256签名 ---");
            Console.WriteLine($"[DEBUG] 输入数据: {dataToSign}");
            
            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);
            Console.WriteLine($"[DEBUG] 签名密钥长度: {keyBytes.Length} 字节");
            Console.WriteLine($"[DEBUG] 签名密钥前20字符: {_jwtKey.Substring(0, Math.Min(20, _jwtKey.Length))}...");
            
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(dataToSign);
                Console.WriteLine($"[DEBUG] 待签名数据字节长度: {dataBytes.Length}");
                
                var signatureBytes = hmac.ComputeHash(dataBytes);
                Console.WriteLine($"[DEBUG] HMAC-SHA256结果字节长度: {signatureBytes.Length}");
                Console.WriteLine($"[DEBUG] HMAC-SHA256结果(十六进制): {Convert.ToHexString(signatureBytes)}");
                
                var signatureBase64Url = Base64UrlEncode(signatureBytes);
                Console.WriteLine($"[DEBUG] Base64URL编码后的签名: {signatureBase64Url}");
                Console.WriteLine($"[DEBUG] --- 签名生成完成 ---");
                
                return signatureBase64Url;
            }
        }
        
        /// <summary>
        /// Base64URL编码 - 与验证服务中的实现保持一致
        /// </summary>
        private string Base64UrlEncode(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            
            Console.WriteLine($"[DEBUG] Base64编码: {base64}");
            Console.WriteLine($"[DEBUG] Base64URL编码: {base64Url}");
            
            return base64Url;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}