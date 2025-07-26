# JWT 认证演示应用

这是一个简单的前后端分离Web应用，演示JWT（JSON Web Token）的工作原理。

## 项目结构

```
├── backend/                 # .NET Core Web API 后端
│   ├── Controllers/
│   │   ├── AuthController.cs    # 认证控制器
│   │   └── UserController.cs    # 用户控制器
│   ├── Program.cs              # 程序入口
│   └── backend.csproj          # 项目文件
├── frontend/                # JavaScript 前端
│   ├── index.html              # 主页面
│   ├── style.css               # 样式文件
│   └── script.js               # 前端逻辑
└── README.md                   # 说明文档
```

## 功能特性

### 后端功能
- 用户登录认证
- JWT令牌生成和验证
- 受保护的API端点
- CORS跨域支持

### 前端功能
- 用户登录界面
- JWT令牌显示和解码
- 调用受保护的API
- 响应结果展示
- JWT工作原理说明

## 快速开始

### 1. 启动后端服务

```bash
# 进入后端目录
cd backend

# 恢复依赖包
dotnet restore

# 运行后端服务
dotnet run
```

后端服务将在 `http://localhost:5000` 启动

### 2. 启动前端服务

```bash
# 进入前端目录
cd frontend

# 使用Python启动简单HTTP服务器
python -m http.server 8080

# 或者使用Node.js的http-server
# npx http-server -p 8080
```

前端服务将在 `http://localhost:8080` 启动

### 3. 访问应用

在浏览器中打开 `http://localhost:8080`

## 默认登录信息

- **用户名**: admin
- **密码**: password

## JWT 工作流程演示

### 1. 用户登录
- 用户输入用户名和密码
- 后端验证凭据
- 验证成功后生成JWT令牌
- 前端保存令牌到localStorage

### 2. 访问受保护资源
- 前端在请求头中携带JWT令牌
- 后端验证令牌有效性
- 验证通过后返回受保护的数据

### 3. 令牌解码
- 前端可以解码JWT令牌查看内容
- 显示Header、Payload和Signature信息

## API 端点

### 认证相关
- `POST /api/auth/login` - 用户登录

### 用户相关（需要认证）
- `GET /api/user/profile` - 获取用户资料
- `GET /api/user/data` - 获取机密数据

## JWT 结构说明

JWT由三部分组成，用点号(.)分隔：

1. **Header（头部）**
   - 包含令牌类型(typ)和签名算法(alg)
   - Base64编码

2. **Payload（载荷）**
   - 包含声明(claims)，如用户信息、过期时间等
   - Base64编码

3. **Signature（签名）**
   - 用于验证令牌完整性
   - 使用Header中指定的算法和密钥生成

## 安全注意事项

⚠️ **重要提醒**：这是一个演示项目，不适合直接用于生产环境。

生产环境中需要考虑：
- 使用更安全的密钥管理
- 实现令牌刷新机制
- 添加更严格的输入验证
- 使用HTTPS协议
- 实现更完善的错误处理
- 添加日志记录和监控

## 技术栈

- **后端**: .NET 8.0, ASP.NET Core Web API
- **前端**: HTML5, CSS3, JavaScript (ES6+)
- **认证**: JWT (JSON Web Token)

## 学习要点

通过这个演示应用，您可以学习到：

1. JWT的基本结构和工作原理
2. 如何在.NET Core中实现JWT认证
3. 前端如何存储和使用JWT令牌
4. 如何在API请求中携带认证信息
5. JWT令牌的解码和验证过程

## 故障排除

### 常见问题

1. **CORS错误**
   - 确保后端已正确配置CORS策略
   - 检查前端请求的域名和端口

2. **令牌过期**
   - JWT令牌默认1小时过期
   - 过期后需要重新登录

3. **端口冲突**
   - 确保5000端口（后端）和8080端口（前端）未被占用
   - 可以修改为其他可用端口

## 扩展建议

- 添加用户注册功能
- 实现令牌刷新机制
- 添加角色权限控制
- 集成数据库存储用户信息
- 添加密码加密存储
- 实现更复杂的前端路由