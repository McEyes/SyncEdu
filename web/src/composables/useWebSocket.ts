import { ref, onUnmounted } from 'vue'
import { useUserStore } from '../stores/user'

export interface NotificationMessage {
  type: string
  title: string
  content?: string
  data?: any
  timestamp: string
}

const ws = ref<WebSocket | null>(null)
const connected = ref(false)
const notifications = ref<NotificationMessage[]>([])
const unreadCount = ref(0)
let reconnectTimer: ReturnType<typeof setTimeout> | null = null
let heartbeatTimer: ReturnType<typeof setInterval> | null = null

function getWsUrl(): string {
  const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
  const host = window.location.host
  const userStore = useUserStore()
  return `${protocol}//${host}/api/notification/ws?access_token=${userStore.token}`
}

function startHeartbeat() {
  stopHeartbeat()
  heartbeatTimer = setInterval(() => {
    if (ws.value?.readyState === WebSocket.OPEN) {
      ws.value.send('ping')
    }
  }, 25000)
}

function stopHeartbeat() {
  if (heartbeatTimer) {
    clearInterval(heartbeatTimer)
    heartbeatTimer = null
  }
}

function connect() {
  const userStore = useUserStore()
  if (!userStore.isLoggedIn) return

  if (ws.value && ws.value.readyState === WebSocket.OPEN) return

  try {
    ws.value = new WebSocket(getWsUrl())

    ws.value.onopen = () => {
      connected.value = true
      startHeartbeat()
    }

    ws.value.onmessage = (event) => {
      try {
        const msg: NotificationMessage = JSON.parse(event.data)
        notifications.value.unshift(msg)
        if (notifications.value.length > 50) {
          notifications.value = notifications.value.slice(0, 50)
        }
        unreadCount.value++
      } catch (e) {
        console.error('WebSocket message parse error:', e)
      }
    }

    ws.value.onclose = () => {
      connected.value = false
      stopHeartbeat()
      // 5秒后自动重连
      reconnectTimer = setTimeout(() => {
        connect()
      }, 5000)
    }

    ws.value.onerror = () => {
      connected.value = false
    }
  } catch (e) {
    console.error('WebSocket connect error:', e)
  }
}

function disconnect() {
  if (reconnectTimer) {
    clearTimeout(reconnectTimer)
    reconnectTimer = null
  }
  stopHeartbeat()
  if (ws.value) {
    ws.value.close()
    ws.value = null
  }
  connected.value = false
}

function markAllRead() {
  unreadCount.value = 0
}

function clearNotifications() {
  notifications.value = []
  unreadCount.value = 0
}

export function useWebSocket() {
  onUnmounted(() => {
    // 组件卸载时不断开，保持全局连接
  })

  return {
    connected,
    notifications,
    unreadCount,
    connect,
    disconnect,
    markAllRead,
    clearNotifications
  }
}
