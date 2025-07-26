# 手写JWT生成详解

## 🎯 目标

完全手写JWT令牌生成逻辑，不使用任何JWT库，深入理解JWT生成的每个步骤，并确保与手写验证逻辑完美配合。

## 🔧 JWT生成的5个核心步骤

### 步骤1：创建JWT Header
```csharp
var header = new
{
    alg = "HS256",  // 签名算法
    typ = "JWT"     // 令牌类型
};

var headerJson = JsonSerializer.Serialize(header);
// 结果: {"alg":"HS256","typ":"JWT"}

var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
// 结果: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
```

### 步骤2：创建JWT Payload
```csharp
var currentTime = DateTimeOffset.UtcNow;
var expirationTime = currentTime.AddHours(1);

var payload = new
{
    name = username,                              // 用户名
    userId = "1",                                // 用户ID
    role = "admin",                              // 用户角色
    iat = currentTime.ToUnixTimeSeconds(),       // 签发时间
    exp = expirationTime.ToUnixTimeSeconds()     // 过期时间
};

var payloadJson = JsonSerializer.Serialize(payload);
// 结果: {"name":"admin","userId":"1","role":"admin","iat":1640995200,"exp":1640998800}

var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
// 结果: eyJuYW1lIjoiYWRtaW4iLCJ1c2VySWQiOiIxIiwicm9sZSI6ImFkbWluIiwiaWF0IjoxNjQwOTk1MjAwLCJleHAiOjE2NDA5OTg4MDB9
```

### 步骤3：创建待签名数据
```csharp
var dataToSign = $"{headerBase64}.{payloadBase64}";
// 结果: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4iLCJ1c2VySWQiOiIxIiwicm9sZSI6ImFkbWluIiwiaWF0IjoxNjQwOTk1MjAwLCJleHAiOjE2NDA5OTg4MDB9
```

### 步骤4：生成HMAC-SHA256签名
```csharp
private string GenerateSignature(string dataToSign)
{
    var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);
    
    using (var hmac = new HMACSHA256(keyBytes))
    {
        var dataBytes = Encoding.UTF8.GetBytes(dataToSign);
        var signatureBytes = hmac.ComputeHash(dataBytes);
        return Base64UrlEncode(signatureBytes);
    }
}
```

### 步骤5：组装完整JWT
```csharp
var jwt = $"{headerBase64}.{payloadBase64}.{signature}";
// 最终结果: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4i...._signature_part_
```

## 🔄 生成与验证的完美配合

### 生成端（AuthController）
```csharp
// 1. 创建Header
var header = new { alg = "HS256", typ = "JWT" };
var headerBase64 = Base64UrlEncode(JsonBytes(header));

// 2. 创建Payload
var payload = new { name = "admin", userId = "1", role = "admin", exp = expTime };
var payloadBase64 = Base64UrlEncode(JsonBytes(payload));

// 3. 生成签名
var dataToSign = $"{headerBase64}.{payloadBase64}";
var signature = HMAC_SHA256(dataToSign, secret_key);

// 4. 组装JWT
var jwt = $"{headerBase64}.{payloadBase64}.{signature}";
```

### 验证端（JwtValidationService）
```csharp
// 1. 分割JWT
var parts = jwt.Split('.');
var headerBase64 = parts[0];
var payloadBase64 = parts[1];
var receivedSignature = parts[2];

// 2. 重新计算签名
var dataToSign = $"{headerBase64}.{payloadBase64}";
var expectedSignature = HMAC_SHA256(dataToSign, secret_key);

// 3. 验证签名
if (expectedSignature == receivedSignature) {
    // 验证通过
}
```

## 🔐 Base64URL编码详解

### 为什么使用Base64URL而不是标准Base64？

标准Base64使用的字符：`A-Z`, `a-z`, `0-9`, `+`, `/`, `=`
Base64URL使用的字符：`A-Z`, `a-z`, `0-9`, `-`, `_`（无填充）

```csharp
private string Base64UrlEncode(byte[] bytes)
{
    var base64 = Convert.ToBase64String(bytes);
    var base64Url = base64
        .Replace('+', '-')    // + 替换为 -
        .Replace('/', '_')    // / 替换为 _
        .TrimEnd('=');        // 移除填充字符 =
    
    return base64Url;
}
```

**原因**：
- `+` 和 `/` 在URL中有特殊含义
- `=` 用作填充，在URL中可能引起问题
- Base64URL是URL安全的编码方式

## 🕐 时间戳处理

### Unix时间戳
```csharp
var currentTime = DateTimeOffset.UtcNow;
var unixTimestamp = currentTime.ToUnixTimeSeconds();

// 示例：
// 2024-01-01 12:00:00 UTC = 1704110400
```

### 过期时间设置
```csharp
var expirationTime = currentTime.AddHours(1);  // 1小时后过期
var exp = expirationTime.ToUnixTimeSeconds();
```

### 验证时的时间检查
```csharp
var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(payload.exp).DateTime;
if (DateTime.UtcNow > expirationDateTime) {
    // 令牌已过期
}
```

## 🔍 详细调试日志

### 生成过程日志
```
[DEBUG] === 开始手写JWT令牌生成过程 ===
[DEBUG] 目标用户: admin
[DEBUG] 1. Header JSON: {"alg":"HS256","typ":"JWT"}
[DEBUG] 1. Header Base64URL: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
[DEBUG] 2. Payload JSON: {"name":"admin","userId":"1","role":"admin","iat":1640995200,"exp":1640998800}
[DEBUG] 2. 签发时间: 2024-01-01 12:00:00 UTC
[DEBUG] 2. 过期时间: 2024-01-01 13:00:00 UTC
[DEBUG] 2. Payload Base64URL: eyJuYW1lIjoiYWRtaW4i...
[DEBUG] 3. 待签名数据: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4i...
[DEBUG] 4. 生成的签名: xyz123abc...
[DEBUG] 5. 完整JWT令牌长度: 245 字符
[DEBUG] === JWT令牌生成完成 ===
```

### 签名生成日志
```
[DEBUG] --- 开始生成HMAC-SHA256签名 ---
[DEBUG] 输入数据: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4i...
[DEBUG] 签名密钥长度: 73 字节
[DEBUG] 签名密钥前20字符: SuperSecureJwtKeyFor...
[DEBUG] 待签名数据字节长度: 180
[DEBUG] HMAC-SHA256结果字节长度: 32
[DEBUG] HMAC-SHA256结果(十六进制): A1B2C3D4E5F6789012345678901234567890ABCDEF...
[DEBUG] Base64URL编码后的签名: xyz123abc...
[DEBUG] --- 签名生成完成 ---
```

## 🧪 测试验证

### 生成的JWT结构
```
Header:    eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
Payload:   eyJuYW1lIjoiYWRtaW4iLCJ1c2VySWQiOiIxIiwicm9sZSI6ImFkbWluIiwiaWF0IjoxNjQwOTk1MjAwLCJleHAiOjE2NDA5OTg4MDB9
Signature: xyz123abc456def789ghi012jkl345mno678pqr901stu234vwx567yz
```

### 验证兼容性
1. **Header解码**：`{"alg":"HS256","typ":"JWT"}`
2. **Payload解码**：`{"name":"admin","userId":"1","role":"admin","iat":1640995200,"exp":1640998800}`
3. **签名验证**：使用相同算法和密钥重新计算签名进行比较

## 🎓 学习成果

通过手写JWT生成，您现在完全理解了：

1. **JWT的内部构造过程** - 每个部分如何创建
2. **JSON序列化** - 如何将对象转换为JSON字符串
3. **Base64URL编码** - 为什么使用URL安全的编码
4. **HMAC-SHA256签名** - 如何使用密钥生成安全签名
5. **时间戳处理** - Unix时间戳的使用和转换
6. **生成与验证的对应关系** - 确保两端逻辑一致

### 关键优势：
- ✅ **完全控制** - 每个步骤都在您的掌控之中
- ✅ **深度理解** - 理解JWT的每个细节
- ✅ **调试友好** - 详细的日志帮助排查问题
- ✅ **安全透明** - 清楚地知道安全机制如何工作

这种深度理解将使您在处理JWT相关问题时更加自信和专业！