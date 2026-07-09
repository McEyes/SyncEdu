# 阳光同步学 (SyncEdu)

从幼儿园到大学的多端同步学习平台。

## 功能特性

### Phase 1 - 核心框架
- 用户注册/登录（JWT 认证）
- 家庭管理（创建/加入家庭）
- 多孩管理（添加/编辑/删除小孩）
- 学习档案设置（教育阶段、年级、学校）
- 教育体系数据（阶段、年级、教材版本、学科）

### Phase 2 - 学习跟踪
- 学习计划管理（创建/查看/进度跟踪）
- 打卡系统（文字/拍照/视频打卡，人脸验证）
- 学习提醒（定时提醒推送）
- 成就与激励（积分、成就徽章、鼓励记录）
- 学习会话（开始/结束学习计时）

### Phase 3 - 教育资源同步
- 教育数据同步（对接教育局 API 预留）
- 教材管理（章节、课时管理）
- 学习推荐（智能推荐课程）
- 每科目独立教材版本配置

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端 | C# / .NET 8 / SqlSugar ORM |
| 数据库 | PostgreSQL |
| Web 前端 | Vue 3 / Vite / TypeScript / Element Plus / Pinia |
| 移动端 | uni-app / Vue 3 |
| 认证 | JWT Bearer Token |
| 日志 | Serilog |
| API 文档 | Swagger / OpenAPI |

## 项目结构

```
SyncEdu/
├── src/
│   ├── SyncEdu.Api/          # Web API 项目
│   ├── SyncEdu.Core/         # 核心业务层（实体、服务接口/实现）
│   ├── SyncEdu.Infrastructure/ # 基础设施（SqlSugar、扩展）
│   ├── SyncEdu.Shared/       # 共享模型（DTO、枚举）
│   └── SyncEdu.Job/          # 后台任务（定时同步、提醒推送）
├── web/                      # Vue 3 Web 前端
│   └── src/
│       ├── api/              # API 请求封装
│       ├── stores/           # Pinia 状态管理
│       ├── views/            # 页面组件
│       └── router/           # 路由配置
├── mobile/                   # uni-app 移动端
│   └── src/
│       ├── api/              # API 请求封装
│       └── pages/            # 页面组件
└── SyncEdu.sln              # 解决方案文件
```

## 快速开始

### 环境要求

- .NET 8 SDK
- PostgreSQL 12+
- Node.js 18+
- npm 或 pnpm

### 后端启动

```bash
# 配置数据库连接（可选，默认使用 localhost:5432）
# 修改 src/SyncEdu.Api/appsettings.json 中的 ConnectionStrings

# 启动 API 服务
cd src/SyncEdu.Api
dotnet run

# Swagger UI: http://localhost:8028/swagger
```

首次启动会自动：
- 创建数据库表
- 初始化教育阶段、年级、教材版本、学科等基础数据

### Web 前端启动

```bash
cd web
npm install
npm run dev

# 访问: http://localhost:5173
```

### 移动端启动

```bash
cd mobile
npm install
npm run dev:h5

# H5 预览: http://localhost:5174
```

## API 端口

| 服务 | 地址 |
|------|------|
| HTTP API | http://localhost:8028 |
| HTTPS API | https://localhost:8080 |
| Swagger UI | http://localhost:8028/swagger |
| Web 前端 | http://localhost:5173 |
| 移动端 H5 | http://localhost:5174 |

## 主要 API 接口

| 模块 | 路径 | 说明 |
|------|------|------|
| 认证 | /api/auth | 注册、登录 |
| 家庭 | /api/family | 创建、加入家庭 |
| 小孩 | /api/child | 多孩管理 |
| 教育体系 | /api/education | 阶段、年级、科目、教材 |
| 仪表盘 | /api/dashboard | 学习概览 |
| 学习计划 | /api/learning | 计划、进度、会话 |
| 打卡 | /api/checkin | 打卡记录、统计 |
| 提醒 | /api/reminder | 学习提醒管理 |
| 成就 | /api/achievement | 积分、成就、鼓励 |
| 资源同步 | /api/educationSync | 教育数据同步 |

## 开发计划

- [x] Phase 1: 核心框架 + 多孩管理
- [x] Phase 2: 学习跟踪 + 打卡系统
- [x] Phase 3: 教育资源同步
- [ ] 移动端原生适配
- [ ] 人脸识别集成
- [ ] 教育局 API 对接
- [ ] 消息推送（WebSocket）

## License

MIT
