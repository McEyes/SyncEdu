<template>
  <el-container class="main-layout">
    <!-- 侧边栏 -->
    <el-aside width="220px" class="sidebar">
      <div class="logo">
        <h2>阳光同步学</h2>
      </div>

      <!-- 多孩切换区域 -->
      <div class="child-switcher" v-if="childStore.children.length > 0">
        <div class="child-switcher-title">切换小孩</div>
        <div
          v-for="child in childStore.children"
          :key="child.id"
          class="child-item"
          :class="{ active: child.id === childStore.activeChildId }"
          @click="childStore.switchChild(child.id)"
        >
          <el-avatar :size="32" :src="child.avatar">
            {{ child.nickName.charAt(0) }}
          </el-avatar>
          <span class="child-name">{{ child.nickName }}</span>
        </div>
      </div>

      <!-- 导航菜单 -->
      <el-menu
        :default-active="route.path"
        router
        class="sidebar-menu"
      >
        <el-menu-item index="/">
          <el-icon><HomeFilled /></el-icon>
          <span>学习概览</span>
        </el-menu-item>
        <el-menu-item index="/children">
          <el-icon><User /></el-icon>
          <span>小孩管理</span>
        </el-menu-item>
        <el-menu-item index="/learning">
          <el-icon><Notebook /></el-icon>
          <span>学习计划</span>
        </el-menu-item>
        <el-menu-item index="/checkin">
          <el-icon><Calendar /></el-icon>
          <span>学习打卡</span>
        </el-menu-item>
        <el-menu-item index="/reminders">
          <el-icon><Bell /></el-icon>
          <span>学习提醒</span>
        </el-menu-item>
        <el-menu-item index="/achievements">
          <el-icon><Trophy /></el-icon>
          <span>成就激励</span>
        </el-menu-item>
        <el-menu-item index="/education">
          <el-icon><Reading /></el-icon>
          <span>教育体系</span>
        </el-menu-item>
        <el-menu-item index="/education-sync">
          <el-icon><Refresh /></el-icon>
          <span>资源同步</span>
        </el-menu-item>
        <el-menu-item index="/family">
          <el-icon><House /></el-icon>
          <span>家庭设置</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <!-- 主内容区 -->
    <el-container>
      <!-- 顶部导航 -->
      <el-header class="header">
        <div class="header-left">
          <span v-if="childStore.activeChild" class="current-child">
            当前：{{ childStore.activeChild.nickName }}
            <el-tag v-if="childStore.activeChild.stageName" size="small" type="primary">
              {{ childStore.activeChild.stageName }}
            </el-tag>
            <el-tag v-if="childStore.activeChild.gradeName" size="small">
              {{ childStore.activeChild.gradeName }}
            </el-tag>
          </span>
        </div>
        <div class="header-right">
          <NotificationPanel />
          <span class="user-name">{{ userStore.nickName }}</span>
          <el-dropdown @command="handleCommand">
            <el-avatar :size="32">
              {{ userStore.nickName.charAt(0) }}
            </el-avatar>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 内容 -->
      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { HomeFilled, User, House, Reading, Notebook, Calendar, Bell, Trophy, Refresh } from '@element-plus/icons-vue'
import { useUserStore } from '../stores/user'
import { useChildStore } from '../stores/child'
import { useWebSocket } from '../composables/useWebSocket'
import NotificationPanel from '../components/NotificationPanel.vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const childStore = useChildStore()
const { connect, disconnect } = useWebSocket()

onMounted(async () => {
  await childStore.loadChildren()
  // 登录后自动连接 WebSocket
  connect()
})

onUnmounted(() => {
  disconnect()
})

function handleCommand(command: string) {
  if (command === 'logout') {
    userStore.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.main-layout {
  height: 100vh;
}

.sidebar {
  background: #1d1e1f;
  color: #fff;
  overflow-y: auto;
}

.logo {
  padding: 20px;
  text-align: center;
  border-bottom: 1px solid #333;
}

.logo h2 {
  color: #409eff;
  font-size: 18px;
  margin: 0;
}

.child-switcher {
  padding: 16px;
  border-bottom: 1px solid #333;
}

.child-switcher-title {
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}

.child-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.child-item:hover {
  background: #333;
}

.child-item.active {
  background: #409eff22;
  border: 1px solid #409eff;
}

.child-name {
  font-size: 14px;
}

.sidebar-menu {
  border: none;
  background: transparent;
}

.sidebar-menu .el-menu-item {
  color: #ccc;
}

.sidebar-menu .el-menu-item:hover,
.sidebar-menu .el-menu-item.is-active {
  color: #409eff;
  background: #409eff11;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #eee;
  background: #fff;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.current-child {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-name {
  font-size: 14px;
  color: #666;
}

.main-content {
  background: #f5f7fa;
}
</style>
