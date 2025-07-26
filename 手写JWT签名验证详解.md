# 手写JWT签名验证详解

## 🎯 核心目标

完全手写JWT签名验证逻辑，不使用任何JWT库，深入理解JWT防伪造的核心机制。

## 🔍 JWT签名验证的本质

JWT的安全性完全依赖于签名验证。签名的作用是：
1. **防止篡改** - 任何对Header或Payload的修改都会导致签名不匹配
2. **验证来源** - 只有拥有正确密钥的服务器才能生成有效签名

## 🛠️ 手写实现的核心步骤

### 步骤1：JWT结构解析
```csharp
// JWT格式：Header.Payload.Signature
var tokenParts = token.Split('.');
var header = tokenParts[0];      // eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
var payload = tokenParts[1];     // eyJuYW1lIjoiYWRtaW4iLCJ1c2VySWQiOiIxIi...
var signature = tokenParts[2];   // 4f_sl9aa8dkjfh2kj3h4kj5h6kj7h8kj9h0
```

### 步骤2：Base64URL解码
```csharp
private string Base64UrlDecode(string base64Url)
{
    // Base64URL转换为标准Base64
    var base64 = base64Url.Replace('-', '+').Replace('_', '/');
    
    // 添加必要的填充
    switch (base64.Length % 4)
    {
        case 2: base64 += "=="; break;
        case 3: base64 += "="; break;
    }
    
    var bytes = Convert.FromBase64String(base64);
    return Encoding.UTF8.GetString(bytes);
}
```

### 步骤3：JSON解析
```csharp
// 解码后得到JSON字符串
var headerJson = Base64UrlDecode(tokenParts[0]);
// {"alg":"HS256","typ":"JWT"}

var payloadJson = Base64UrlDecode(tokenParts[1]);
// {"name":"admin","userId":"1","role":"admin","exp":1640995200}

// 解析为强类型对象
var header = JsonSerializer.Deserialize<JwtHeader>(headerJson);
var payload = JsonSerializer.Deserialize<JwtPayload>(payloadJson);
```

### 步骤4：手动签名验证（核心防伪造逻辑）
```csharp
private bool ValidateSignatureManually(string header, string payload, string signature)
{
    // 1. 重新构建待签名的数据
    var dataToSign = $"{header}.{payload}";
    
    // 2. 使用相同的密钥和HMAC-SHA256算法
    var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);
    using (var hmac = new HMACSHA256(keyBytes))
    {
        var dataBytes = Encoding.UTF8.GetBytes(dataToSign);
        var expectedSignatureBytes = hmac.ComputeHash(dataBytes);
        
        // 3. 转换为Base64URL格式
        var expectedSignature = Base64UrlEncode(expectedSignatureBytes);
        
        // 4. 比较签名
        return expectedSignature == signature;
    }
}
```

## 🔐 签名验证的数学原理

### HMAC-SHA256算法
```
HMAC-SHA256(message, secret_key) = hash_function
```

1. **输入数据**: `Header.Payload` (Base64URL编码的字符串)
2. **密钥**: 服务器的秘密密钥
3. **算法**: HMAC-SHA256
4. **输出**: 256位(32字节)的哈希值

### 验证过程
```csharp
// 原始JWT生成时
var originalData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4i...";
var originalSignature = HMAC_SHA256(originalData, secret_key);

// 验证时重新计算
var receivedData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWRtaW4i...";
var expectedSignature = HMAC_SHA256(receivedData, secret_key);

// 比较签名
if (expectedSignature == receivedSignature) {
    // 验证通过，JWT未被篡改
} else {
    // 验证失败，JWT可能被篡改
}
```

## 🚨 防伪造机制详解

### 为什么签名能防止伪造？

1. **密钥保密性**
   ```csharp
   // 只有服务器知道这个密钥
   private readonly string _jwtKey = "SuperSecureJwtKeyForDemo...";
   ```
   - 攻击者无法获得密钥
   - 无法生成有效签名

2. **哈希函数的单向性**
   ```csharp
   // 从签名无法反推出密钥
   var signature = HMAC_SHA256(data, secret_key);  // 容易计算
   var secret_key = reverse_HMAC(signature, data); // 几乎不可能
   ```

3. **雪崩效应**
   ```csharp
   // 微小的数据变化导致完全不同的签名
   var data1 = "eyJ1c2VyIjoiYWRtaW4ifQ";  // {"user":"admin"}
   var data2 = "eyJ1c2VyIjoiYWRtaW4ifR";  // 只改变最后一个字符
   
   var sig1 = HMAC_SHA256(data1, key);  // 完全不同的签名
   var sig2 = HMAC_SHA256(data2, key);  // 完全不同的签名
   ```

### 常见攻击场景及防护

#### 1. 篡改Payload攻击
```json
// 原始Payload
{"name":"user","role":"user","exp":1640995200}

// 攻击者尝试修改
{"name":"user","role":"admin","exp":1640995200}
```
**防护机制**: 修改后的Payload会产生不同的签名，验证失败

#### 2. 重放攻击
```json
// 攻击者使用过期的有效JWT
{"name":"admin","role":"admin","exp":1640995200}  // 已过期
```
**防护机制**: 过期时间验证
```csharp
var expirationTime = DateTimeOffset.FromUnixTimeSeconds(payload.exp).DateTime;
if (DateTime.UtcNow > expirationTime) {
    return "JWT令牌已过期";
}
```

#### 3. 算法替换攻击
```json
// 攻击者尝试将算法改为"none"
{"alg":"none","typ":"JWT"}
```
**防护机制**: 严格验证算法
```csharp
if (header.alg != "HS256") {
    return $"不支持的签名算法: {header.alg}";
}
```

## 🔧 调试和验证

### 详细的调试日志
```csharp
Console.WriteLine($"[DEBUG] 待签名数据: {dataToSign}");
Console.WriteLine($"[DEBUG] 签名密钥长度: {keyBytes.Length} 字节");
Console.WriteLine($"[DEBUG] 计算出的签名字节: {Convert.ToHexString(expectedSignatureBytes)}");
Console.WriteLine($"[DEBUG] 期望的Base64URL签名: {expectedSignature}");
Console.WriteLine($"[DEBUG] 实际收到的签名: {signature}");
Console.WriteLine($"[DEBUG] 签名比较结果: {(isMatch ? "匹配" : "不匹配")}");
```

### 在Rider中调试
推荐断点位置：
1. `ValidateSignatureManually()` - 签名验证入口
2. `hmac.ComputeHash(dataBytes)` - 签名计算
3. `expectedSignature == signature` - 签名比较
4. `Base64UrlDecode()` - 解码过程

## 📊 性能和安全考虑

### 性能优化
```csharp
// 使用using语句确保HMAC对象正确释放
using (var hmac = new HMACSHA256(keyBytes))
{
    // 签名计算
}
```

### 安全最佳实践
1. **密钥管理**
   - 使用足够长的密钥（至少256位）
   - 定期轮换密钥
   - 安全存储密钥

2. **时间验证**
   - 严格验证过期时间
   - 不允许时钟偏差

3. **算法限制**
   - 只允许安全的算法（HS256）
   - 拒绝"none"算法

## 🎓 学习成果

通过手写JWT签名验证，您现在完全理解了：

1. **JWT的内部结构** - Header、Payload、Signature的具体内容
2. **Base64URL编码** - 为什么使用URL安全的Base64变体
3. **HMAC-SHA256算法** - 如何使用密钥生成和验证签名
4. **防伪造原理** - 为什么签名能防止JWT被篡改
5. **安全威胁** - 常见的JWT攻击方式及防护措施

这种深度理解将帮助您在实际项目中更好地使用和保护JWT认证系统！