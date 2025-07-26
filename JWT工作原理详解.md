# JWT 工作原理详解

基于我们创建的前后端项目，详细解释JWT（JSON Web Token）的工作机制。

## 1. JWT 基本概念

JWT是一种开放标准（RFC 7519），用于在各方之间安全地传输信息。它是一个紧凑的、URL安全的令牌，包含了用户的身份信息和权限声明。

### JWT 结构

JWT由三部分组成，用点号(.)分隔：

```
Header.Payload.Signature
```

#### 1.1 Header（头部）
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```
- `alg`: 签名算法，我们项目中使用 HMAC SHA256
- `typ`: 令牌类型，固定为 JWT

#### 1.2 Payload（载荷）
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "admin",
  "userId": "1",
  "role": "admin",
  "exp": 1703123456,
  "iat": 1703119856
}
```
- 包含用户信息和声明（Claims）
- `exp`: 过期时间（Unix时间戳）
- `iat`: 签发时间

#### 1.3 Signature（签名）
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```
- 用于验证令牌的完整性
- 防止令牌被篡改

## 2. 我们项目中的JWT实现

### 2.1 后端实现 (.NET Core)

#### JWT配置 (Program.cs)
```csharp
var jwtKey = "SuperSecureJwtKeyForDemoApplication2024WithAtLeast256BitsLength!@#$%^&*()";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
```

#### JWT生成 (AuthController.cs)
```csharp
private string GenerateJwtToken(string username)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("userId", "1"),
            new Claim("role", "admin")
        }),
        Expires = DateTime.UtcNow.AddHours(1), // 1小时后过期
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### 2.2 前端实现 (JavaScript)

#### 令牌存储
```javascript
// 登录成功后存储令牌
this.token = data.token;
localStorage.setItem('jwt_token', this.token);
```

#### API请求携带令牌
```javascript
const response = await fetch(`${this.apiBaseUrl}/user/profile`, {
    method: 'GET',
    headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json',
    }
});
```

#### 令牌解码
```javascript
decodeToken() {
    const parts = this.token.split('.');
    const header = JSON.parse(atob(parts[0]));
    const payload = JSON.parse(atob(parts[1]));
    // 显示解码结果
}
```

## 3. JWT 工作流程

### 3.1 完整认证流程

1. **用户登录**
   - 用户输入用户名和密码
   - 前端发送POST请求到 `/api/auth/login`

2. **服务器验证**
   - 后端验证用户凭据
   - 验证成功后生成JWT令牌

3. **令牌返回**
   - 服务器将JWT令牌返回给客户端
   - 前端存储令牌到localStorage

4. **访问受保护资源**
   - 前端在请求头中携带JWT令牌
   - 服务器验证令牌有效性
   - 验证通过后返回受保护数据

### 3.2 令牌验证过程

1. **提取令牌**
   - 从Authorization头中提取Bearer令牌

2. **验证签名**
   - 使用相同的密钥和算法验证签名
   - 确保令牌未被篡改

3. **检查过期时间**
   - 验证令牌是否在有效期内

4. **提取用户信息**
   - 从Payload中提取用户身份和权限信息

## 4. 安全特性

### 4.1 防篡改
- 任何对Header或Payload的修改都会导致签名验证失败
- 攻击者无法在不知道密钥的情况下伪造有效令牌

### 4.2 时效性
- 令牌包含过期时间，超时后自动失效
- 我们项目中设置为1小时过期

### 4.3 无状态
- 服务器不需要存储会话信息
- 所有必要信息都包含在令牌中

## 5. 项目中的实际应用

### 5.1 登录场景
```
用户: admin / password
↓
后端验证成功
↓
生成JWT: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
↓
前端存储令牌
```

### 5.2 访问受保护API
```
GET /api/user/profile
Headers: Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
↓
后端验证令牌
↓
返回用户信息
```

## 6. 优势与注意事项

### 6.1 优势
- **无状态**: 服务器不需要存储会话
- **可扩展**: 适合分布式系统
- **跨域**: 支持跨域认证
- **移动友好**: 适合移动应用

### 6.2 注意事项
- **令牌大小**: JWT比传统session ID大
- **密钥安全**: 签名密钥必须保密
- **令牌泄露**: 一旦泄露，在过期前都有效
- **无法撤销**: 无法主动使令牌失效（除非实现黑名单）

## 7. 生产环境建议

1. **使用HTTPS**: 防止令牌在传输过程中被截获
2. **短过期时间**: 减少令牌泄露的风险
3. **刷新令牌**: 实现令牌刷新机制
4. **密钥轮换**: 定期更换签名密钥
5. **输入验证**: 严格验证所有输入
6. **日志监控**: 记录认证相关的操作

通过我们的演示项目，您可以直观地看到JWT在实际应用中的工作方式，理解其安全机制和使用场景。