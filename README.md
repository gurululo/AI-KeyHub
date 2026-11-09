# AI-KeyHub 企业内部统一 AI API 管理平台

AI-KeyHub 是一个专为企业内部设计和使用的轻量级、高可靠 AI API 管理平台。基于 C#、.NET 8 和 ASP.NET Core MVC 传统架构开发，主要用于集中管理不同 AI 服务商的 API 渠道，并向企业内部系统、团队及开发者按需分发独立的内部 API Key，实现统一的 AI 接入路由、鉴权控制、模型隔离与流式代理。

---

## 核心功能特性

- **多服务商渠道接入**：支持接入各类采用 OpenAI 兼容协议的 AI 服务提供商（如 OpenAI、Azure OpenAI、DeepSeek、Moonshot 等），支持自定义 API 地址、上游密钥及模型映射。
- **内部 API Key 授权分发**：管理员可根据业务需求创建多个内部专用 API Key，并绑定至特定的上游 AI 渠道，实现按项目或团队细粒度隔离。
- **模型权限隔离与绑定**：每个内部 API Key 严格限定只能调用其绑定渠道所允许的 AI 模型，杜绝越权使用未授权的高成本模型。
- **统一 OpenAI 兼容 API 接口**：对外提供标准 `/v1/chat/completions` 端点，支持无缝接入现有支持 OpenAI 协议的各类应用系统、LangChain、LlamaIndex 及主流客户端工具。
- **双模响应支持**：原生支持标准 JSON 完整响应与 Server-Sent Events (SSE) 实时流式响应 (`stream: true`)。
- **高安全凭据机制**：
  - 密钥使用加密学安全伪随机数生成器（CSPRNG）生成。
  - 内部完整 API Key 仅在创建成功时展示一次，服务端仅持久化 SHA-256 安全哈希与前后缀脱敏串，防止泄露。
  - 管理后台对所有上游 API Key 进行严格脱敏展示。
  - API 转发与异常捕获过程中自动脱敏拦截，防止上游密钥或敏感信息回传至客户端。
- **管理员安全认证体系**：内置管理员账号安全认证与会话管理，密码采用 PBKDF2-SHA256 加盐单向哈希算法存储，严禁明文持久化；登录页面采用标准密码掩码保护。
- **轻量可靠的本地持久化**：采用本地 JSON 存储引擎，基于并发读写锁与原子文件替换写入机制，杜绝断电损坏风险，无需依赖重量级外部数据库。
- **跨平台支持**：支持在 Windows 和 Linux 环境中编译与部署运行。

---

## 技术架构

- **开发框架**：C# 12 / .NET 8.0 (LTS) / ASP.NET Core MVC
- **前端页面**：Razor Views (.cshtml) + Bootstrap 5 + 现代化专业响应式样式设计
- **身份认证**：Cookie Authentication (`[Authorize]`)
- **密码哈希**：PBKDF2（基于 HMAC-SHA256，100,000 次迭代，独立 128 位加密随机 Salt）
- **数据存储**：本地 JSON 数据仓库（`App_Data/data.json`），支持线程安全并发控制与原子保存
- **上游代理**：`IHttpClientFactory` + 高性能数据流管道转发

---

## 环境要求

- **操作系统**：Windows 10 / 11 / Server 2016+ 或主流 Linux 发行版（Ubuntu 20.04+、CentOS/RHEL 8+、Debian 11+ 等）
- **运行环境**：.NET 8.0 SDK 或 ASP.NET Core 8.0 Runtime

---

## 安装与启动

### 1. 获取源码并进入项目根目录
```bash
git clone <repository_url>
cd AI-KeyHub
```

### 2. 编译项目
```bash
dotnet build
```

### 3. 运行服务
通过开发配置启动（默认监听端口可查看 `Properties/launchSettings.json`）：
```bash
dotnet run --launch-profile http
```

或指定监听端口运行：
```bash
dotnet run --urls "http://0.0.0.0:5000"
```

启动完成后，使用浏览器访问管理后台：`http://localhost:5000`

---

## 管理员初始配置与自举说明

- **初次自举机制**：系统在首次启动时，若检测到尚未初始化管理员账号，会自动在数据仓库中自举建立默认管理员账号 `admin`。
- **密码安全存储**：管理员密码在创建时即通过 PBKDF2 加盐单向哈希处理，数据文件中不包含任何明文密码记录。
- **首次登录凭据**：请使用系统初次自举配置的管理员用户名及预设的初始密码进行登录（相关初始凭据请咨询系统负责人或参考企业内部交付备忘录）。登录页面为密码输入框配置了掩码防护，严禁向外展示敏感凭据。
- **身份校验保护**：所有渠道管理、API Key 管理及系统控制台操作均需通过管理员身份校验方可访问。

---

## 使用指南

### 1. 配置 AI 渠道
1. 登录管理后台后，进入 **「AI 渠道管理」** 页面；
2. 点击 **「添加新渠道」** 按钮，填写渠道名称、上游 API 接口基础地址（如 `https://api.openai.com`）、上游 API Key 以及该渠道支持的模型名称（如 `gpt-4o`）；
3. 渠道支持随时启用、禁用、编辑以及删除操作，密钥列表将自动脱敏显示（如 `sk-****abcd`）。

### 2. 分配内部 API Key
1. 进入 **「内部 API Key」** 管理页面；
2. 点击 **「创建 API Key」**，填写用途名称、备注并选择绑定的 AI 渠道；
3. 创建成功后，系统将弹窗展示完整的内部 API Key（格式如 `sk-hub-...`）；
4. **注意**：出于安全考虑，系统不会明文保存该密钥，**完整密钥仅在此时展示一次**，请立即复制并妥善保存。关闭弹窗后将无法再次查看完整内容。

---

## API 接入规范

系统提供与 OpenAI 规范完全兼容的 Chat Completions 接口：

- **请求地址**：`POST /v1/chat/completions`
- **认证方式**：HTTP Header `Authorization: Bearer <分配的内部 API Key>`
- **请求内容格式**：`application/json`

### 1. 标准调用示例 (cURL)
```bash
curl -X POST "http://localhost:5000/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer sk-hub-xxxxxxxxxxxxxxxx" \
  -d '{
    "model": "gpt-4o",
    "messages": [
      {
        "role": "user",
        "content": "请用一句话介绍企业级 API 网关的核心价值。"
      }
    ],
    "stream": false
  }'
```

### 2. 流式调用示例 (SSE Stream)
```bash
curl -X POST "http://localhost:5000/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer sk-hub-xxxxxxxxxxxxxxxx" \
  -d '{
    "model": "gpt-4o",
    "messages": [
      {
        "role": "user",
        "content": "请写一首关于技术演进的短诗。"
      }
    ],
    "stream": true
  }'
```

### 3. Python OpenAI SDK 接入示例
```python
from openai import OpenAI

client = OpenAI(
    api_key="sk-hub-xxxxxxxxxxxxxxxx",  # 填入平台分配的内部 API Key
    base_url="http://localhost:5000/v1" # 指向 AI-KeyHub 统一接入地址
)

response = client.chat.completions.create(
    model="gpt-4o",                      # 必须与内部 Key 绑定的渠道模型一致
    messages=[
        {"role": "user", "content": "你好，请推荐三个高效协同开发实践。"}
    ],
    stream=False
)

print(response.choices[0].message.content)
```

---

## 异常状态码说明

| HTTP 状态码 | 错误代码 (code) | 说明 |
| :--- | :--- | :--- |
| `401 Unauthorized` | `missing_api_key` / `invalid_api_key` | 请求头缺失认证凭据，或提供的内部 API Key 无效、已禁用或被删除 |
| `400 Bad Request` | `model_mismatch` | 请求的模型与该 Key 所绑定渠道允许的模型不一致 |
| `400 Bad Request` | `missing_model_parameter` / `invalid_json` | 请求体格式错误或缺失必要的 `model` 参数 |
| `503 Service Unavailable` | `channel_disabled` | 内部 API Key 绑定的上游 AI 渠道已被管理员禁用或不存在 |
| `502 Bad Gateway` | `upstream_connect_error` | 连接上游 AI 服务商网络异常或接口超时 |

---

## 目录结构说明

```
AI-KeyHub/
├── Controllers/
│   ├── AccountController.cs       # 管理员认证、登录与退出逻辑
│   ├── ApiController.cs           # OpenAI 兼容 /v1/chat/completions 路由与鉴权代理
│   ├── ApiKeyController.cs        # 内部 API Key 管理后台控制器
│   ├── ChannelController.cs       # AI 渠道管理后台控制器
│   └── HomeController.cs          # 系统控制台概览与状态仪表盘
├── Models/
│   ├── AdminUser.cs               # 管理员用户数据模型
│   ├── AppData.cs                 # 本地数据仓库根实体
│   ├── Channel.cs                 # AI 渠道数据模型
│   ├── InternalApiKey.cs          # 内部 API Key 数据模型
│   ├── Api/                       # OpenAI 协议 API 请求与错误响应模型
│   └── ViewModels/                # Web 后台页面视图模型
├── Services/
│   ├── AuthService.cs             # 管理员自举与凭据校验服务
│   ├── ApiKeyService.cs           # 内部 Key 生成、SHA-256 哈希比对与权限服务
│   ├── ChannelService.cs          # AI 渠道业务逻辑与密钥掩码服务
│   ├── ChatProxyService.cs        # 上游 HTTP 请求管道代理、流式 SSE 与敏感信息脱敏
│   ├── JsonDataStorage.cs         # 本地 JSON 数据持久化与并发锁控制
│   └── PasswordHasher.cs          # PBKDF2 安全加盐单向密码哈希实现
├── Views/
│   ├── Account/                   # 登录页面视图（带掩码与防泄露机制）
│   ├── ApiKey/                    # 内部 Key 管理页面与一次性密钥展示模态框
│   ├── Channel/                   # 渠道管理页面与增删改查交互
│   ├── Home/                      # 控制台概览仪表盘
│   └── Shared/                    # 全局响应式布局与公共脚本
├── wwwroot/                       # 静态资源文件（CSS 样式、JavaScript、第三方库）
├── App_Data/                      # 运行时本地数据目录（Git 忽略 *.json）
├── appsettings.json               # 应用程序配置
├── Program.cs                     # 依赖注入配置与请求管道生命周期
├── .gitignore                     # Git 忽略规则文件
├── AI-KeyHub.csproj               # .NET 8 工程文件
└── README.md                      # 项目说明文档
```
