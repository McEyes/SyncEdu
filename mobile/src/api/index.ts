const BASE_URL = 'https://localhost:8080/api'

interface RequestOptions {
  url: string
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  data?: any
  header?: Record<string, string>
}

export function request(options: RequestOptions): Promise<any> {
  return new Promise((resolve, reject) => {
    const token = uni.getStorageSync('token') || ''
    const header: Record<string, string> = {
      'Content-Type': 'application/json',
      ...options.header
    }
    if (token) {
      header['Authorization'] = `Bearer ${token}`
    }

    uni.request({
      url: BASE_URL + options.url,
      method: options.method || 'GET',
      data: options.data,
      header,
      success: (res) => {
        const data = res.data
        // HTTP 状态码可能是 200 但业务层返回 401
        if (res.statusCode === 401) {
          uni.removeStorageSync('token')
          uni.redirectTo({ url: '/pages/login/index' })
          reject(new Error('未登录'))
          return
        }
        if (data.code !== 0) {
          uni.showToast({ title: data.message || '请求失败', icon: 'none' })
          reject(new Error(data.message))
          return
        }
        resolve(data)
      },
      fail: (err) => {
        console.error('Request fail:', err)
        uni.showToast({ title: '网络错误', icon: 'none' })
        reject(err)
      }
    })
  })
}

// API 方法
export const authApi = {
  register: (data: { phone: string; password: string; nickName: string }) =>
    request({ url: '/auth/register', method: 'POST', data }),
  login: (data: { phone: string; password: string }) =>
    request({ url: '/auth/login', method: 'POST', data })
}

export const childApi = {
  create: (data: { nickName: string; gender: number; birthday?: string }) =>
    request({ url: '/child', method: 'POST', data }),
  list: () =>
    request({ url: '/child' }),
  update: (data: any) =>
    request({ url: '/child', method: 'PUT', data }),
  delete: (id: number) =>
    request({ url: `/child/${id}`, method: 'DELETE' }),
  updateProfile: (data: any) =>
    request({ url: '/child/profile', method: 'PUT', data })
}

export const educationApi = {
  getStages: () =>
    request({ url: '/education/stages' }),
  getGrades: (stageId: number) =>
    request({ url: `/education/grades/${stageId}` }),
  getTextbookVersions: () =>
    request({ url: '/education/textbook-versions' })
}

export const dashboardApi = {
  get: () =>
    request({ url: '/dashboard' })
}

export const familyApi = {
  create: (data: { name: string }) =>
    request({ url: '/family', method: 'POST', data }),
  get: () =>
    request({ url: '/family' }),
  join: (inviteCode: string) =>
    request({ url: '/family/join', method: 'POST', data: inviteCode })
}

// Phase 2 API
export const checkInApi = {
  create: (data: { childId: number; type: number; content?: string; mediaData?: string; faceImageData?: string }) =>
    request({ url: '/checkin', method: 'POST', data }),
  getList: (childId: number) =>
    request({ url: `/checkin/${childId}` }),
  getStats: (childId: number) =>
    request({ url: `/checkin/stats/${childId}` })
}

export const reminderApi = {
  getList: (childId: number) =>
    request({ url: `/reminder/${childId}` }),
  create: (data: { childId: number; type: number; title: string; content?: string; reminderTime?: string }) =>
    request({ url: '/reminder', method: 'POST', data }),
  toggle: (id: number, isEnabled: boolean) =>
    request({ url: `/reminder/${id}/toggle?isEnabled=${isEnabled}`, method: 'PUT' }),
  delete: (id: number) =>
    request({ url: `/reminder/${id}`, method: 'DELETE' })
}

export const learningApi = {
  getPlans: (childId: number) =>
    request({ url: `/learning/plans/${childId}` }),
  createPlan: (data: { childId: number; textbookId: number; title: string }) =>
    request({ url: '/learning/plans', method: 'POST', data })
}

export const achievementApi = {
  getPoints: (childId: number) =>
    request({ url: `/achievement/points/${childId}` }),
  getChildAchievements: (childId: number) =>
    request({ url: `/achievement/child/${childId}` })
}

// 通知（WebSocket）
export const notificationApi = {
  getStatus: () =>
    request({ url: '/notification/status' }),
  sendTest: (data: { type?: string; title?: string; content?: string }) =>
    request({ url: '/notification/test', method: 'POST', data }),
  broadcast: (data: { type?: string; title?: string; content?: string }) =>
    request({ url: '/notification/broadcast', method: 'POST', data })
}
