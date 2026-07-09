<template>
  <div class="notification-panel">
    <el-badge :value="unreadCount" :hidden="unreadCount === 0" class="notification-badge">
      <el-button :icon="Bell" circle @click="showDrawer = true" />
    </el-badge>

    <el-drawer v-model="showDrawer" title="消息通知" :size="360">
      <template #header>
        <div class="drawer-header">
          <span>消息通知</span>
          <div class="header-actions">
            <el-tag v-if="connected" type="success" size="small">在线</el-tag>
            <el-tag v-else type="danger" size="small">离线</el-tag>
            <el-button link type="primary" @click="markAllRead">全部已读</el-button>
            <el-button link type="danger" @click="clearNotifications">清空</el-button>
          </div>
        </div>
      </template>

      <div v-if="notifications.length === 0" class="empty-state">
        <el-empty description="暂无通知" />
      </div>

      <div v-else class="notification-list">
        <div
          v-for="(msg, index) in notifications"
          :key="index"
          class="notification-item"
          :class="'type-' + msg.type"
        >
          <div class="notification-icon">
            <el-icon v-if="msg.type === 'checkin_reminder'" :size="20"><Calendar /></el-icon>
            <el-icon v-else-if="msg.type === 'achievement'" :size="20"><Trophy /></el-icon>
            <el-icon v-else-if="msg.type === 'points'" :size="20"><Coin /></el-icon>
            <el-icon v-else-if="msg.type === 'sync'" :size="20"><Refresh /></el-icon>
            <el-icon v-else :size="20"><Bell /></el-icon>
          </div>
          <div class="notification-content">
            <div class="notification-title">{{ msg.title }}</div>
            <div class="notification-text" v-if="msg.content">{{ msg.content }}</div>
            <div class="notification-time">{{ formatTime(msg.timestamp) }}</div>
          </div>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Bell, Calendar, Trophy, Coin, Refresh } from '@element-plus/icons-vue'
import { useWebSocket } from '../composables/useWebSocket'

const { connected, notifications, unreadCount, markAllRead, clearNotifications } = useWebSocket()

const showDrawer = ref(false)

function formatTime(timestamp: string): string {
  if (!timestamp) return ''
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  return date.toLocaleDateString('zh-CN')
}
</script>

<style scoped>
.notification-badge {
  margin-right: 12px;
}

.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 300px;
}

.notification-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.notification-item {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  background: #f5f7fa;
  transition: background 0.2s;
}

.notification-item:hover {
  background: #ecf5ff;
}

.notification-item.type-achievement {
  background: #fdf6ec;
}

.notification-item.type-points {
  background: #f0f9eb;
}

.notification-icon {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: #409eff22;
  color: #409eff;
}

.notification-item.type-achievement .notification-icon {
  background: #e6a23c22;
  color: #e6a23c;
}

.notification-item.type-points .notification-icon {
  background: #67c23a22;
  color: #67c23a;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-title {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}

.notification-text {
  font-size: 13px;
  color: #606266;
  margin-top: 4px;
}

.notification-time {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}
</style>
