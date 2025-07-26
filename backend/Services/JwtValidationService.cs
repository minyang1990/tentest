using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtDemo.Services
{
    public class JwtValidationService
    {
        private readonly string _jwtKey = "SuperSecureJwtKeyForDemoApplication2024WithAtLeast256BitsLength!@#$%^&*()";

        public class JwtValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public ClaimsPrincipal? Principal { get; set; }
            public string? Username { get; set; }
            public string? UserId { get; set; }
            public string? Role { get; set; }
        }

        public JwtValidationResult ValidateToken(string? authorizationHeader)
        {
            Console.WriteLine("[DEBUG] 开始手动验证JWT令牌");
            
            var result = new JwtValidationResult();

            // 步骤1: 检查Authorization头是否存在
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                Console.WriteLine("[DEBUG] 验证失败: 缺少Authorization头");
                result.ErrorMessage = "缺少Authorization头";
                return result;
            }

            // 步骤2: 检查Bearer格式
            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[DEBUG] 验证失败: Authorization头格式错误");
                result.ErrorMessage = "Authorization头格式错误，应为 'Bearer {token}'";
                return result;
            }

            // 步骤3: 提取JWT令牌
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            Console.WriteLine($"[DEBUG] 提取到的JWT令牌长度: {token.Length}");
            Console.WriteLine($"[DEBUG] 令牌前50个字符: {token.Substring(0, Math.Min(50, token.Length))}...");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[DEBUG] 验证失败: JWT令牌为空");
                result.ErrorMessage = "JWT令牌为空";
                return result;
            }

            try
            {
                // 步骤4: 解析JWT令牌结构
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    Console.WriteLine($"[DEBUG] 验证失败: JWT格式错误，部分数量: {tokenParts.Length}");
                    result.ErrorMessage = "JWT格式错误，应包含3个部分";
                    return result;
                }

                Console.WriteLine("[DEBUG] JWT令牌结构验证通过");
                Console.WriteLine($"[DEBUG] - Header长度: {tokenParts[0].Length}");
                Console.WriteLine($"[DEBUG] - Payload长度: {tokenParts[1].Length}");
                Console.WriteLine($"[DEBUG] - Signature长度: {tokenParts[2].Length}");

                // 步骤5: 配置验证参数
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtKey);
                Console.WriteLine($"[DEBUG] 使用密钥长度: {key.Length} 字节");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // 验证过期时间
                    ClockSkew = TimeSpan.Zero // 不允许时间偏差
                };

                Console.WriteLine("[DEBUG] JWT验证参数配置完成");

                // 步骤6: 验证JWT令牌
                Console.WriteLine("[DEBUG] 开始验证JWT签名和过期时间...");
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                Console.WriteLine("[DEBUG] JWT令牌验证成功！");

                // 步骤7: 提取用户信息
                var jwtToken = validatedToken as JwtSecurityToken;
                if (jwtToken != null)
                {
                    Console.WriteLine($"[DEBUG] JWT令牌详细信息:");
                    Console.WriteLine($"[DEBUG] - 算法: {jwtToken.Header.Alg}");
                    Console.WriteLine($"[DEBUG] - 类型: {jwtToken.Header.Typ}");
                    Console.WriteLine($"[DEBUG] - 签发时间: {jwtToken.IssuedAt}");
                    Console.WriteLine($"[DEBUG] - 过期时间: {jwtToken.ValidTo}");
                    Console.WriteLine($"[DEBUG] - Claims数量: {jwtToken.Claims.Count()}");
                }

                // 步骤8: 提取Claims信息
                result.Username = principal.FindFirst(ClaimTypes.Name)?.Value;
                result.UserId = principal.FindFirst("userId")?.Value;
                result.Role = principal.FindFirst("role")?.Value;
                result.Principal = principal;
                result.IsValid = true;

                Console.WriteLine($"[DEBUG] 提取的用户信息:");
                Console.WriteLine($"[DEBUG] - 用户名: {result.Username}");
                Console.WriteLine($"[DEBUG] - 用户ID: {result.UserId}");
                Console.WriteLine($"[DEBUG] - 角色: {result.Role}");

                return result;
            }
            catch (SecurityTokenExpiredException ex)
            {
                Console.WriteLine($"[DEBUG] 验证失败: JWT令牌已过期 - {ex.Message}");
                result.ErrorMessage = "JWT令牌已过期";
                return result;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                Console.WriteLine($"[DEBUG] 验证失败: JWT签名无效 - {ex.Message}");
                result.ErrorMessage = "JWT签名无效";
                return result;
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"[DEBUG] 验证失败: JWT令牌无效 - {ex.Message}");
                result.ErrorMessage = $"JWT令牌无效: {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] 验证失败: 未知错误 - {ex.Message}");
                result.ErrorMessage = $"令牌验证时发生错误: {ex.Message}";
                return result;
            }
        }
    }
}