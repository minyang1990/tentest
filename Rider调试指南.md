# JetBrains Rider 调试指南

## 🚀 在Rider中打开项目

### 1. 打开解决方案文件
- 启动JetBrains Rider
- 选择 "Open" 或 "Open Solution"
- 导航到项目根目录
- 选择 `JwtDemo.sln` 文件
- 点击 "Open"

### 2. 项目结构
```
JwtDemo/
├── JwtDemo.sln                 # 解决方案文件
├── backend/                    # .NET Core 后端项目
│   ├── Controllers/
│   │   ├── AuthController.cs   # 认证控制器
│   │   └── UserController.cs   # 用户控制器
│   ├── Program.cs              # 程序入口
│   └── backend.csproj          # 项目文件
└── frontend/                   # 前端文件
    ├── index.html
    ├── style.css
    └── script.js
```

## 🔍 设置断点调试

### 1. 设置断点
- 在代码编辑器中，点击行号左侧的空白区域
- 红色圆点表示断点已设置
- 推荐设置断点的位置：

#### AuthController.cs 关键断点位置：
```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    Console.WriteLine($"[DEBUG] 收到登录请求 - 用户名: {request.Username}"); // 断点1
    
    if (request.Username == "admin" && request.Password == "password")
    {
        Console.WriteLine("[DEBUG] 用户验证成功，开始生成JWT令牌"); // 断点2
        var token = GenerateJwtToken(request.Username); // 断点3
        
        return Ok(new { 
            token = token,
            message = "登录成功",
            username = request.Username
        }); // 断点4
    }
    
    return Unauthorized(new { message = "用户名或密码错误" }); // 断点5
}
```

#### JWT生成方法断点：
```csharp
private string GenerateJwtToken(string username)
{
    Console.WriteLine($"[DEBUG] 开始为用户 '{username}' 生成JWT令牌"); // 断点6
    
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtKey); // 断点7
    
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("userId", "1"),
            new Claim("role", "admin")
        }), // 断点8
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor); // 断点9
    return tokenHandler.WriteToken(token); // 断点10
}
```

### 2. 启动调试
- 在Rider顶部工具栏中，确保选择了 "Backend API" 配置
- 点击绿色的调试按钮（虫子图标）或按 `Shift+F9`
- 或者使用菜单：Run → Debug 'Backend API'

### 3. 调试控制
- **继续执行**: `F9` 或点击播放按钮
- **单步执行**: `F8` 
- **步入方法**: `F7`
- **步出方法**: `Shift+F8`
- **停止调试**: `Ctrl+F2`

## 🧪 测试调试流程

### 1. 启动调试会话
1. 在 `AuthController.Login` 方法的第一行设置断点
2. 启动调试（Shift+F9）
3. 等待应用启动（通常在 http://localhost:5000）

### 2. 触发断点
1. 打开浏览器访问 http://localhost:8080
2. 输入用户名：`admin`，密码：`password`
3. 点击登录按钮
4. Rider会在断点处暂停执行

### 3. 检查变量
- 在调试时，将鼠标悬停在变量上查看值
- 使用 "Variables" 窗口查看所有局部变量
- 使用 "Watches" 窗口添加监视表达式

### 4. 调试JWT生成过程
1. 在 `GenerateJwtToken` 方法中设置断点
2. 单步执行，观察：
   - `username` 参数值
   - `_jwtKey` 密钥内容
   - `tokenDescriptor` 对象属性
   - 最终生成的 `token` 字符串

## 🔧 高级调试技巧

### 1. 条件断点
- 右键点击断点
- 选择 "Edit Breakpoint"
- 添加条件，如：`request.Username == "admin"`

### 2. 日志断点
- 设置断点时选择 "Log message to console"
- 可以输出变量值而不暂停执行

### 3. 异常断点
- Run → View Breakpoints
- 添加异常断点来捕获特定异常

### 4. 调用堆栈
- 在调试时查看 "Call Stack" 窗口
- 了解方法调用链

## 🎯 调试场景示例

### 场景1：调试登录验证
```csharp
// 在这里设置断点，检查请求参数
if (request.Username == "admin" && request.Password == "password")
{
    // 检查用户名和密码是否正确传递
}
```

### 场景2：调试JWT令牌生成
```csharp
// 检查Claims是否正确添加
Subject = new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Name, username), // 断点：检查username值
    new Claim("userId", "1"),
    new Claim("role", "admin")
}),
```

### 场景3：调试API认证
```csharp
// 在UserController中设置断点
[HttpGet("profile")]
public IActionResult GetProfile()
{
    var username = User.FindFirst(ClaimTypes.Name)?.Value; // 断点：检查JWT解析结果
    // 检查User.Claims集合
}
```

## 📊 调试信息查看

### 1. Variables 窗口
- 查看当前作用域内的所有变量
- 展开对象查看属性

### 2. Immediate 窗口
- 在调试时执行C#表达式
- 例如：`request.Username.Length`

### 3. Console 输出
- 查看 `Console.WriteLine` 的调试日志
- 结合断点使用更有效

## 🚨 常见问题解决

### 1. 断点不生效
- 确保项目是Debug模式编译
- 检查断点是否在可执行代码行上
- 重新构建项目

### 2. 无法启动调试
- 检查端口5000是否被占用
- 确保.NET 8.0 SDK已安装
- 检查项目依赖是否正确恢复

### 3. 变量显示 "无法计算表达式"
- 可能是编译器优化导致
- 在Debug配置下重新编译

通过Rider的强大调试功能，您可以深入理解JWT认证的每个步骤，包括令牌生成、验证和用户信息提取的完整过程！