# 手动JWT验证实现详解

## 🎯 学习目标

通过手动实现JWT验证，深入理解JWT认证的每个步骤，而不依赖.NET Core的`[Authorize]`特性。

## 🔄 实现对比

### 之前：使用.NET Core自动验证
```csharp
[Authorize]  // 自动验证JWT
public class UserController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value; // 直接获取
        return Ok(new { username });
    }
}
```

### 现在：手动验证JWT
```csharp
public class UserController : ControllerBase
{
    private readonly JwtValidationService _jwtValidationService;

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // 手动验证JWT令牌
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var validationResult = _jwtValidationService.ValidateToken(authHeader);
        
        if (!validationResult.IsValid)
        {
            return Unauthorized(new { error = validationResult.ErrorMessage });
        }
        
        return Ok(new { username = validationResult.Username });
    }
}
```

## 🔍 手动验证的详细步骤

### 步骤1：提取Authorization头
```csharp
var authHeader = Request.Headers["Authorization"].FirstOrDefault();
// 示例: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 步骤2：验证Bearer格式
```csharp
if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
{
    return "Authorization头格式错误，应为 'Bearer {token}'";
}
```

### 步骤3：提取JWT令牌
```csharp
var token = authorizationHeader.Substring("Bearer ".Length).Trim();
// 提取纯JWT令牌字符串
```

### 步骤4：验证JWT结构
```csharp
var tokenParts = token.Split('.');
if (tokenParts.Length != 3)
{
    return "JWT格式错误，应包含3个部分";
}
// JWT必须有Header.Payload.Signature三部分
```

### 步骤5：配置验证参数
```csharp
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,  // 验证签名密钥
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,           // 不验证发行者
    ValidateAudience = false,         // 不验证受众
    ValidateLifetime = true,          // 验证过期时间
    ClockSkew = TimeSpan.Zero         // 不允许时间偏差
};
```

### 步骤6：执行JWT验证
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
```

### 步骤7：提取用户信息
```csharp
var username = principal.FindFirst(ClaimTypes.Name)?.Value;
var userId = principal.FindFirst("userId")?.Value;
var role = principal.FindFirst("role")?.Value;
```

## 🛠️ JwtValidationService 核心功能

### 验证结果类
```csharp
public class JwtValidationResult
{
    public bool IsValid { get; set; }           // 验证是否成功
    public string? ErrorMessage { get; set; }   // 错误信息
    public ClaimsPrincipal? Principal { get; set; } // 用户主体
    public string? Username { get; set; }       // 用户名
    public string? UserId { get; set; }         // 用户ID
    public string? Role { get; set; }           // 用户角色
}
```

### 验证方法
```csharp
public JwtValidationResult ValidateToken(string? authorizationHeader)
{
    // 1. 检查Authorization头
    // 2. 验证Bearer格式
    // 3. 提取JWT令牌
    // 4. 验证JWT结构
    // 5. 配置验证参数
    // 6. 执行签名和过期时间验证
    // 7. 提取Claims信息
    // 8. 返回验证结果
}
```

## 🔐 安全验证机制

### 1. 签名验证
```csharp
// 使用相同的密钥验证签名
IssuerSigningKey = new SymmetricSecurityKey(key)
```
- 确保令牌未被篡改
- 验证令牌确实由我们的服务器签发

### 2. 过期时间验证
```csharp
ValidateLifetime = true,
ClockSkew = TimeSpan.Zero
```
- 检查令牌是否在有效期内
- 不允许时间偏差

### 3. 结构验证
```csharp
var tokenParts = token.Split('.');
if (tokenParts.Length != 3) // Header.Payload.Signature
```
- 确保JWT格式正确
- 包含所有必需部分

## 🚨 错误处理

### 常见错误类型
```csharp
catch (SecurityTokenExpiredException ex)
{
    // JWT令牌已过期
}
catch (SecurityTokenInvalidSignatureException ex)
{
    // JWT签名无效
}
catch (SecurityTokenException ex)
{
    // 其他JWT令牌错误
}
```

### 错误响应
```csharp
return Unauthorized(new { 
    message = "访问被拒绝", 
    error = validationResult.ErrorMessage 
});
```

## 📊 调试日志

### 详细的验证过程日志
```csharp
Console.WriteLine("[DEBUG] 开始手动验证JWT令牌");
Console.WriteLine($"[DEBUG] 提取到的JWT令牌长度: {token.Length}");
Console.WriteLine($"[DEBUG] 使用密钥长度: {key.Length} 字节");
Console.WriteLine("[DEBUG] 开始验证JWT签名和过期时间...");
Console.WriteLine("[DEBUG] JWT令牌验证成功！");
```

## 🎓 学习收获

### 通过手动实现，您将理解：

1. **JWT结构解析**
   - Header、Payload、Signature三部分
   - Base64编码和解码过程

2. **签名验证原理**
   - HMAC-SHA256算法
   - 密钥的重要性

3. **时间验证机制**
   - 过期时间检查
   - 时钟偏差处理

4. **Claims提取过程**
   - 用户身份信息
   - 自定义声明

5. **错误处理策略**
   - 不同类型的验证失败
   - 安全的错误响应

## 🔧 调试建议

### 在Rider中设置断点：
1. `JwtValidationService.ValidateToken()` - 验证入口
2. `tokenHandler.ValidateToken()` - 核心验证逻辑
3. `principal.FindFirst()` - Claims提取
4. 各种异常捕获块 - 错误处理

### 观察变量：
- `authorizationHeader` - 原始Authorization头
- `token` - 提取的JWT字符串
- `tokenParts` - JWT三个部分
- `principal` - 验证后的用户主体
- `validationResult` - 最终验证结果

通过这种手动实现方式，您可以完全掌握JWT验证的每个细节，为深入理解JWT安全机制打下坚实基础！