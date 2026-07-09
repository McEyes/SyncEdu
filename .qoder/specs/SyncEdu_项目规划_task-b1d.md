# SyncEdu (阳光同步学) 项目总体规划

## 项目概览

面向 K-12 到大学生的同步学习平台，支持多孩管理、学习路径跟踪、打卡监督（含摄像头验证）、教育资源同步。

## 技术架构

| 层级 | 技术选型 |
|------|---------|
| Web前端 | Vue3 + Vite + Pinia + Element Plus |
| 移动端 | uni-app (Vue3) |
| 服务端 | C# + Furion + SqlSugar |
| 数据库 | PostgreSQL |
| 缓存 | Redis |
| 文件存储 | MinIO / 阿里云OSS |
| 摄像头打卡 | WebRTC + 前端拍照 + 后端AI人脸校验 |

## 项目目录结构

```
SyncEdu/
├── src/
│   ├── SyncEdu.Api/              # Furion Web API 主项目
│   ├── SyncEdu.Core/             # 核心业务逻辑层
│   ├── SyncEdu.Infrastructure/   # 基础设施（数据库、缓存、OSS）
│   ├── SyncEdu.Shared/           # 共享DTO、枚举、常量
│   └── SyncEdu.Job/              # 定时任务（教育数据同步等）
├── web/                          # Vue3 + Vite Web前端
├── mobile/                       # uni-app 移动端
├── docs/                         # 项目文档
└── SyncEdu.sln                   # 解决方案文件
```

## 数据库核心表设计

```
-- 用户与家庭
family              # 家庭/家长账户
child               # 小孩档案（关联family）
child_profile       # 学习档案（当前阶段、学校等）

-- 教育体系
education_stage     # 教育阶段（幼儿园/小学/初中/高中/大学）
grade               # 年级
subject             # 学科
textbook_version    # 教材版本（人教版/北师大版等）
textbook            # 教材（关联版本+年级+学科）
chapter             # 章节
lesson              # 课时/课文

-- 学习跟踪
learning_plan       # 学习计划（关联child+textbook）
learning_progress   # 学习进度
study_session       # 学习会话记录
check_in            # 打卡记录（含图片/视频路径）
study_reminder      # 学习提醒

-- 激励系统
achievement         # 成就/勋章定义
child_achievement   # 小孩获得的成就
encouragement_log   # 鼓励记录
points_transaction  # 积分流水
```

## 第一阶段：核心框架 + 多孩管理（当前目标）

### Task 1: 后端项目初始化

- 创建 .NET 8 解决方案 `SyncEdu.sln`
- 按分层架构创建项目：Api / Core / Infrastructure / Shared / Job
- 配置 Furion 框架、SqlSugar + PostgreSQL 连接
- 配置 Swagger、全局异常处理、日志（Serilog）
- 实现基础 JWT 认证中间件

### Task 2: 用户与多孩管理模块

- 家长注册/登录 API（手机号+验证码 / 账号密码）
- 家庭(Family)管理：创建家庭、邀请成员
- 小孩(Child)管理：增删改查、头像、昵称、生日
- 学习档案(Profile)：选择教育阶段、学校、年级、教材版本
- 多孩切换：当前活跃小孩的上下文管理

### Task 3: 教育体系数据模型

- 教育阶段/年级/学科/教材版本/教材/章节/课时 完整数据模型
- 教育数据初始化 Seeder（内置基础数据：阶段、常见年级、主流学科）
- 教育数据 CRUD API（管理端）
- 预留教育局 API 对接接口（IEducationDataProvider 抽象）

### Task 4: Web前端项目初始化

- Vue3 + Vite + TypeScript 项目搭建
- Pinia 状态管理 + Vue Router
- Element Plus UI 组件库
- 封装 Axios 请求层（拦截器、Token刷新）
- 布局框架：侧边栏（多孩切换）+ 顶部导航 + 内容区
- 登录/注册页面

### Task 5: 多孩管理前端页面

- 家庭设置页：管理家庭成员、邀请链接
- 小孩档案页：添加/编辑小孩信息
- 学习阶段选择页：阶段 -> 学校 -> 年级 -> 教材版本
- 多孩切换组件：全局顶部/侧边栏的小孩头像切换
- 仪表盘首页：展示各小孩的学习概览

### Task 6: uni-app 移动端初始化

- uni-app (Vue3) 项目创建
- 复用 Web 端 API 接口层
- 登录/注册 + 多孩切换基础页面
- 适配微信小程序 + H5 双端

## 第二阶段（后续）：学习跟踪 + 打卡系统

- 学习计划制定与进度跟踪
- 每日学习任务清单
- 打卡系统（文字/拍照/视频）
- 摄像头调用与人脸识别校验
- 学习提醒推送（微信模板消息/App Push）
- 积分与成就激励系统
- 鼓励策略引擎（根据学习表现生成个性化鼓励语）

## 第三阶段（后续）：教育资源同步

- 对接国家智慧教育平台 API
- 教材内容自动同步
- AI 辅助课程内容生成
- 学习路径智能推荐

## 开发规范

- 后端：RESTful API 风格，统一响应格式 `{ code, message, data }`
- 前端：组合式 API (Composition API)，TypeScript 严格模式
- Git：Conventional Commits 规范
- 分支策略：main / develop / feature-*
