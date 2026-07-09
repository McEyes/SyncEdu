const WS_BASE_URL = 'wss://localhost:8080/api/notification/ws'

interface NotificationMessage {
  type: string
  title: string
  content?: string
  data?: any
  timestamp: string
}

type MessageHandler = (msg: NotificationMessage) => void

let socketTask: UniApp.SocketTask | null = null
let connected = false
let reconnectTimer: ReturnType<typeof setTimeout> | null = null
let heartbeatTimer: ReturnType<typeof setInterval> | null = null
const messageHandlers: MessageHandler[] = []
const notificationList: NotificationMessage[] = []
let unreadCount = 0

function startHeartbeat() {
  stopHeartbeat()
  heartbeatTimer = setInterval(() => {
    if (socketTask && connected) {
      socketTask.send({ data: 'ping' })
    }
  }, 25000)
}

function stopHeartbeat() {
  if (heartbeatTimer) {
    clearInterval(heartbeatTimer)
    heartbeatTimer = null
  }
}

export function connectWebSocket() {
  const token = uni.getStorageSync('token')
  if (!token) return

  if (socketTask && connected) return

  const url = `${WS_BASE_URL}?access_token=${token}`

  socketTask = uni.connectSocket({
    url,
    success: () => {},
    fail: (err) => {
      console.error('WebSocket connect fail:', err)
    }
  })

  socketTask.onOpen(() => {
    connected = true
    startHeartbeat()
    console.log('WebSocket connected')
  })

  socketTask.onMessage((res) => {
    try {
      const msg: NotificationMessage = JSON.parse(res.data as string)
      notificationList.unshift(msg)
      if (notificationList.length > 50) {
        notificationList.splice(50)
      }
      unreadCount++
      // 通知所有监听者
      messageHandlers.forEach(handler => handler(msg))
    } catch (e) {
      console.error('WebSocket message parse error:', e)
    }
  })

  socketTask.onClose(() => {
    connected = false
    stopHeartbeat()
    // 5秒后自动重连
    reconnectTimer = setTimeout(() => {
      connectWebSocket()
    }, 5000)
  })

  socketTask.onError((err) => {
    console.error('WebSocket error:', err)
    connected = false
  })
}

export function disconnectWebSocket() {
  if (reconnectTimer) {
    clearTimeout(reconnectTimer)
    reconnectTimer = null
  }
  stopHeartbeat()
  if (socketTask) {
    socketTask.close({})
    socketTask = null
  }
  connected = false
}

export function onWebSocketMessage(handler: MessageHandler) {
  messageHandlers.push(handler)
  return () => {
    const idx = messageHandlers.indexOf(handler)
    if (idx >= 0) messageHandlers.splice(idx, 1)
  }
}

export function getNotificationList() {
  return notificationList
}

export function getUnreadCount() {
  return unreadCount
}

export function markAllRead() {
  unreadCount = 0
}

export function isWsConnected() {
  return connected
}
