using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JwtDemo.Services
{
    public class JwtValidationService
    {
        private readonly string _jwtKey = "SuperSecureJwtKeyForDemoApplication2024WithAtLeast256BitsLength!@#$%^&*()";

        public class JwtValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public string? Username { get; set; }
            public string? UserId { get; set; }
            public string? Role { get; set; }
            public DateTime? ExpirationTime { get; set; }
            public string? DecodedHeader { get; set; }
            public string? DecodedPayload { get; set; }
        }

        public class JwtHeader
        {
            public string alg { get; set; } = "";
            public string typ { get; set; } = "";
        }

        public class JwtPayload
        {
            public string? name { get; set; }
            public string? userId { get; set; }
            public string? role { get; set; }
            public long exp { get; set; }
            public long iat { get; set; }
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

                // 步骤5: 手动解码JWT的Header和Payload
                Console.WriteLine("[DEBUG] 开始手动解码JWT各部分...");
                
                var headerJson = Base64UrlDecode(tokenParts[0]);
                var payloadJson = Base64UrlDecode(tokenParts[1]);
                var signature = tokenParts[2];
                
                Console.WriteLine($"[DEBUG] 解码后的Header: {headerJson}");
                Console.WriteLine($"[DEBUG] 解码后的Payload: {payloadJson}");
                Console.WriteLine($"[DEBUG] 原始Signature: {signature}");

                // 步骤6: 解析Header和Payload JSON
                JwtHeader? header;
                JwtPayload? payload;
                
                try
                {
                    header = JsonSerializer.Deserialize<JwtHeader>(headerJson);
                    payload = JsonSerializer.Deserialize<JwtPayload>(payloadJson);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[DEBUG] JSON解析失败: {ex.Message}");
                    result.ErrorMessage = "JWT内容格式错误";
                    return result;
                }

                if (header == null || payload == null)
                {
                    Console.WriteLine("[DEBUG] Header或Payload解析为null");
                    result.ErrorMessage = "JWT内容解析失败";
                    return result;
                }

                Console.WriteLine($"[DEBUG] Header解析结果:");
                Console.WriteLine($"[DEBUG] - 算法: {header.alg}");
                Console.WriteLine($"[DEBUG] - 类型: {header.typ}");
                Console.WriteLine($"[DEBUG] Payload解析结果:");
                Console.WriteLine($"[DEBUG] - 用户名: {payload.name}");
                Console.WriteLine($"[DEBUG] - 用户ID: {payload.userId}");
                Console.WriteLine($"[DEBUG] - 角色: {payload.role}");
                Console.WriteLine($"[DEBUG] - 过期时间戳: {payload.exp}");

                // 步骤7: 验证算法
                if (header.alg != "HS256")
                {
                    Console.WriteLine($"[DEBUG] 不支持的算法: {header.alg}");
                    result.ErrorMessage = $"不支持的签名算法: {header.alg}";
                    return result;
                }

                // 步骤8: 手动验证签名（核心防伪造逻辑）
                Console.WriteLine("[DEBUG] 开始手动验证JWT签名...");
                var isSignatureValid = ValidateSignatureManually(tokenParts[0], tokenParts[1], signature);
                
                if (!isSignatureValid)
                {
                    Console.WriteLine("[DEBUG] 签名验证失败！JWT可能被篡改");
                    result.ErrorMessage = "JWT签名验证失败，令牌可能被篡改";
                    return result;
                }
                
                Console.WriteLine("[DEBUG] 签名验证成功！JWT未被篡改");

                // 步骤9: 验证过期时间
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(payload.exp).DateTime;
                var currentTime = DateTime.UtcNow;
                
                Console.WriteLine($"[DEBUG] 当前时间: {currentTime:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"[DEBUG] 令牌过期时间: {expirationTime:yyyy-MM-dd HH:mm:ss} UTC");
                
                if (currentTime > expirationTime)
                {
                    Console.WriteLine("[DEBUG] JWT令牌已过期");
                    result.ErrorMessage = "JWT令牌已过期";
                    return result;
                }
                
                Console.WriteLine("[DEBUG] 过期时间验证通过");

                // 步骤10: 提取用户信息
                result.Username = payload.name;
                result.UserId = payload.userId;
                result.Role = payload.role;
                result.ExpirationTime = expirationTime;
                result.DecodedHeader = headerJson;
                result.DecodedPayload = payloadJson;
                result.IsValid = true;

                Console.WriteLine($"[DEBUG] JWT验证完全成功！提取的用户信息:");
                Console.WriteLine($"[DEBUG] - 用户名: {result.Username}");
                Console.WriteLine($"[DEBUG] - 用户ID: {result.UserId}");
                Console.WriteLine($"[DEBUG] - 角色: {result.Role}");
                Console.WriteLine($"[DEBUG] - 过期时间: {result.ExpirationTime}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] 验证失败: 未知错误 - {ex.Message}");
                result.ErrorMessage = $"令牌验证时发生错误: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// 手动验证JWT签名 - 核心防伪造逻辑
        /// </summary>
        private bool ValidateSignatureManually(string header, string payload, string signature)
        {
            Console.WriteLine("[DEBUG] === 开始手动签名验证过程 ===");
            
            try
            {
                // 1. 重新构建待签名的数据（Header.Payload）
                var dataToSign = $"{header}.{payload}";
                Console.WriteLine($"[DEBUG] 待签名数据: {dataToSign}");
                
                // 2. 使用相同的密钥和算法生成期望的签名
                var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);
                Console.WriteLine($"[DEBUG] 签名密钥长度: {keyBytes.Length} 字节");
                Console.WriteLine($"[DEBUG] 签名密钥前20字符: {_jwtKey.Substring(0, Math.Min(20, _jwtKey.Length))}...");
                
                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var dataBytes = Encoding.UTF8.GetBytes(dataToSign);
                    var expectedSignatureBytes = hmac.ComputeHash(dataBytes);
                    
                    Console.WriteLine($"[DEBUG] 计算出的签名字节长度: {expectedSignatureBytes.Length}");
                    Console.WriteLine($"[DEBUG] 计算出的签名字节: {Convert.ToHexString(expectedSignatureBytes)}");
                    
                    // 3. 将期望签名转换为Base64URL格式
                    var expectedSignature = Base64UrlEncode(expectedSignatureBytes);
                    Console.WriteLine($"[DEBUG] 期望的Base64URL签名: {expectedSignature}");
                    Console.WriteLine($"[DEBUG] 实际收到的签名: {signature}");
                    
                    // 4. 比较签名
                    var isMatch = expectedSignature == signature;
                    Console.WriteLine($"[DEBUG] 签名比较结果: {(isMatch ? "匹配" : "不匹配")}");
                    
                    if (!isMatch)
                    {
                        Console.WriteLine("[DEBUG] ⚠️  签名不匹配！这表明JWT可能被篡改");
                        Console.WriteLine($"[DEBUG] 期望签名长度: {expectedSignature.Length}");
                        Console.WriteLine($"[DEBUG] 实际签名长度: {signature.Length}");
                        
                        // 逐字符比较找出差异
                        var minLength = Math.Min(expectedSignature.Length, signature.Length);
                        for (int i = 0; i < minLength; i++)
                        {
                            if (expectedSignature[i] != signature[i])
                            {
                                Console.WriteLine($"[DEBUG] 第一个不匹配字符位置: {i}");
                                Console.WriteLine($"[DEBUG] 期望: '{expectedSignature[i]}', 实际: '{signature[i]}'");
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] ✅ 签名验证成功！JWT未被篡改");
                    }
                    
                    return isMatch;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] 签名验证过程中发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Base64URL解码
        /// </summary>
        private string Base64UrlDecode(string base64Url)
        {
            Console.WriteLine($"[DEBUG] Base64URL解码输入: {base64Url}");
            
            // Base64URL转换为标准Base64
            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            
            // 添加必要的填充
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            
            Console.WriteLine($"[DEBUG] 转换为标准Base64: {base64}");
            
            var bytes = Convert.FromBase64String(base64);
            var result = Encoding.UTF8.GetString(bytes);
            
            Console.WriteLine($"[DEBUG] 解码结果: {result}");
            return result;
        }

        /// <summary>
        /// Base64URL编码
        /// </summary>
        private string Base64UrlEncode(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            
            Console.WriteLine($"[DEBUG] Base64URL编码: {Convert.ToBase64String(bytes)} -> {base64Url}");
            return base64Url;
        }
    }
}