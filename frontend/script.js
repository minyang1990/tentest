class JWTDemo {
    constructor() {
        this.apiBaseUrl = 'http://localhost:5000/api';
        this.token = localStorage.getItem('jwt_token');
        this.init();
    }

    init() {
        this.bindEvents();
        this.checkAuthStatus();
    }

    bindEvents() {
        // 登录表单提交
        document.getElementById('loginForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.login();
        });

        // 获取用户资料
        document.getElementById('getProfileBtn').addEventListener('click', () => {
            this.getUserProfile();
        });

        // 获取机密数据
        document.getElementById('getDataBtn').addEventListener('click', () => {
            this.getSecureData();
        });

        // 退出登录
        document.getElementById('logoutBtn').addEventListener('click', () => {
            this.logout();
        });

        // 解码令牌
        document.getElementById('decodeTokenBtn').addEventListener('click', () => {
            this.decodeToken();
        });
    }

    async login() {
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        try {
            const response = await fetch(`${this.apiBaseUrl}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();

            if (response.ok) {
                this.token = data.token;
                localStorage.setItem('jwt_token', this.token);
                this.showAuthenticatedUI();
                this.displayToken();
                this.displayResponse('登录成功！', data, 'success');
            } else {
                this.displayResponse('登录失败', data, 'error');
            }
        } catch (error) {
            this.displayResponse('网络错误', { error: error.message }, 'error');
        }
    }

    async getUserProfile() {
        if (!this.token) {
            this.displayResponse('错误', { message: '请先登录' }, 'error');
            return;
        }

        try {
            const response = await fetch(`${this.apiBaseUrl}/user/profile`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json',
                }
            });

            const data = await response.json();

            if (response.ok) {
                this.displayResponse('获取用户资料成功', data, 'success');
                this.displayUserInfo(data);
            } else {
                this.displayResponse('获取用户资料失败', data, 'error');
                if (response.status === 401) {
                    this.logout();
                }
            }
        } catch (error) {
            this.displayResponse('网络错误', { error: error.message }, 'error');
        }
    }

    async getSecureData() {
        if (!this.token) {
            this.displayResponse('错误', { message: '请先登录' }, 'error');
            return;
        }

        try {
            const response = await fetch(`${this.apiBaseUrl}/user/data`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json',
                }
            });

            const data = await response.json();

            if (response.ok) {
                this.displayResponse('获取机密数据成功', data, 'success');
            } else {
                this.displayResponse('获取机密数据失败', data, 'error');
                if (response.status === 401) {
                    this.logout();
                }
            }
        } catch (error) {
            this.displayResponse('网络错误', { error: error.message }, 'error');
        }
    }

    logout() {
        this.token = null;
        localStorage.removeItem('jwt_token');
        this.showLoginUI();
        this.displayResponse('已退出登录', { message: '令牌已清除' }, 'success');
    }

    decodeToken() {
        if (!this.token) {
            this.displayResponse('错误', { message: '没有可解码的令牌' }, 'error');
            return;
        }

        try {
            // JWT由三部分组成，用点号分隔
            const parts = this.token.split('.');
            if (parts.length !== 3) {
                throw new Error('无效的JWT格式');
            }

            // 解码Header
            const header = JSON.parse(atob(parts[0]));
            
            // 解码Payload
            const payload = JSON.parse(atob(parts[1]));

            // 格式化过期时间
            if (payload.exp) {
                payload.expireTime = new Date(payload.exp * 1000).toLocaleString('zh-CN');
            }

            const decodedInfo = {
                header: header,
                payload: payload,
                signature: parts[2]
            };

            document.getElementById('tokenPayload').textContent = JSON.stringify(decodedInfo, null, 2);
            document.getElementById('tokenDecoded').classList.remove('hidden');
            
            this.displayResponse('令牌解码成功', decodedInfo, 'success');
        } catch (error) {
            this.displayResponse('令牌解码失败', { error: error.message }, 'error');
        }
    }

    checkAuthStatus() {
        if (this.token) {
            this.showAuthenticatedUI();
            this.displayToken();
        } else {
            this.showLoginUI();
        }
    }

    showLoginUI() {
        document.getElementById('loginSection').classList.remove('hidden');
        document.getElementById('userSection').classList.add('hidden');
        document.getElementById('tokenSection').classList.add('hidden');
        document.getElementById('userInfo').innerHTML = '';
    }

    showAuthenticatedUI() {
        document.getElementById('loginSection').classList.add('hidden');
        document.getElementById('userSection').classList.remove('hidden');
        document.getElementById('tokenSection').classList.remove('hidden');
    }

    displayToken() {
        document.getElementById('tokenDisplay').value = this.token || '';
    }

    displayUserInfo(userInfo) {
        const userInfoDiv = document.getElementById('userInfo');
        userInfoDiv.innerHTML = `
            <div style="background-color: #e3f2fd; padding: 15px; border-radius: 6px; margin-bottom: 20px;">
                <h3 style="color: #1976d2; margin-bottom: 10px;">当前用户信息</h3>
                <p><strong>用户名:</strong> ${userInfo.username}</p>
                <p><strong>用户ID:</strong> ${userInfo.userId}</p>
                <p><strong>角色:</strong> ${userInfo.role}</p>
            </div>
        `;
    }

    displayResponse(title, data, type = 'success') {
        const responseDiv = document.getElementById('responseDisplay');
        responseDiv.className = type === 'success' ? 'success' : 'error';
        
        const timestamp = new Date().toLocaleString('zh-CN');
        const responseText = `[${timestamp}] ${title}\n\n${JSON.stringify(data, null, 2)}`;
        
        responseDiv.textContent = responseText;
    }
}

// 页面加载完成后初始化应用
document.addEventListener('DOMContentLoaded', () => {
    new JWTDemo();
});