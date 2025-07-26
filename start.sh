#!/bin/bash

echo "==================================="
echo "JWT 认证演示应用启动脚本"
echo "==================================="

# 检查是否安装了.NET
if ! command -v dotnet &> /dev/null; then
    echo "错误: 未找到 .NET SDK"
    echo "请先安装 .NET 8.0 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi

# 检查是否安装了Python
if ! command -v python3 &> /dev/null && ! command -v python &> /dev/null; then
    echo "错误: 未找到 Python"
    echo "请先安装 Python: https://www.python.org/downloads/"
    exit 1
fi

echo "1. 启动后端服务..."
cd backend

# 恢复依赖包
echo "   恢复 NuGet 包..."
dotnet restore

# 在后台启动后端服务
echo "   启动 .NET Core API 服务..."
dotnet run &
BACKEND_PID=$!

# 等待后端服务启动
echo "   等待后端服务启动..."
sleep 5

cd ..

echo "2. 启动前端服务..."
cd frontend

# 启动前端服务
echo "   启动前端 HTTP 服务器..."
if command -v python3 &> /dev/null; then
    python3 -m http.server 8080 &
elif command -v python &> /dev/null; then
    python -m http.server 8080 &
fi
FRONTEND_PID=$!

cd ..

echo ""
echo "==================================="
echo "服务启动完成！"
echo "==================================="
echo "后端服务: http://localhost:5000"
echo "前端应用: http://localhost:8080"
echo ""
echo "默认登录信息:"
echo "用户名: admin"
echo "密码: password"
echo ""
echo "按 Ctrl+C 停止所有服务"
echo "==================================="

# 等待用户中断
trap "echo '正在停止服务...'; kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; exit" INT

# 保持脚本运行
wait