import request from './request'

// ==================== 认证 ====================
export const authApi = {
  register: (data: { phone: string; password: string; nickName: string }) =>
    request.post('/auth/register', data),
  login: (data: { phone: string; password: string }) =>
    request.post('/auth/login', data)
}

// ==================== 家庭 ====================
export const familyApi = {
  create: (data: { name: string }) =>
    request.post('/family', data),
  get: () =>
    request.get('/family'),
  join: (inviteCode: string) =>
    request.post('/family/join', inviteCode)
}

// ==================== 小孩 ====================
export const childApi = {
  create: (data: { nickName: string; avatar?: string; birthday?: string; gender: number }) =>
    request.post('/child', data),
  list: () =>
    request.get('/child'),
  update: (data: { id: number; nickName?: string; avatar?: string; birthday?: string; gender?: number }) =>
    request.put('/child', data),
  delete: (id: number) =>
    request.delete(`/child/${id}`),
  updateProfile: (data: { childId: number; stageId?: number; gradeId?: number; schoolName?: string; textbookVersionId?: number }) =>
    request.put('/child/profile', data)
}

// ==================== 教育体系 ====================
export const educationApi = {
  getStages: () =>
    request.get('/education/stages'),
  getGrades: (stageId: number) =>
    request.get(`/education/grades/${stageId}`),
  getTextbookVersions: () =>
    request.get('/education/textbook-versions'),
  getSubjects: () =>
    request.get('/education/subjects'),
  getTextbooks: (gradeId: number, subjectId: number, semester?: number) =>
    request.get('/education/textbooks', { params: { gradeId, subjectId, semester } }),
  getChildSubjectConfigs: (childId: number) =>
    request.get(`/education/child-subject-config/${childId}`),
  setChildSubjectConfig: (data: { childId: number; subjectId: number; textbookVersionId: number; gradeId: number }) =>
    request.post('/education/child-subject-config', data),
  deleteChildSubjectConfig: (configId: number) =>
    request.delete(`/education/child-subject-config/${configId}`)
}

// ==================== 仪表盘 ====================
export const dashboardApi = {
  get: () =>
    request.get('/dashboard')
}

// ==================== 学习计划 ====================
export const learningApi = {
  getPlans: (childId: number) =>
    request.get(`/learning/plans/${childId}`),
  createPlan: (data: { childId: number; gradeId: number; subjectId: number; textbookVersionId: number; semester: number; durationDays?: number; startDate?: string; endDate?: string }) =>
    request.post('/learning/plans', data),
  getProgress: (planId: number, childId: number) =>
    request.get(`/learning/progress/${planId}/${childId}`),
  updateProgress: (data: { planId: number; lessonId: number; childId: number; status: number; studyMinutes?: number }) =>
    request.post('/learning/progress', data),
  startSession: (data: { childId: number; planId?: number; lessonId?: number }) =>
    request.post('/learning/session/start', data),
  endSession: (sessionId: number) =>
    request.post(`/learning/session/${sessionId}/end`)
}

// ==================== 打卡 ====================
export const checkInApi = {
  create: (data: { childId: number; planId?: number; type: number; content?: string; mediaData?: string; faceImageData?: string }) =>
    request.post('/checkin', data),
  getList: (childId: number, startDate?: string, endDate?: string) =>
    request.get(`/checkin/${childId}`, { params: { startDate, endDate } }),
  getStats: (childId: number) =>
    request.get(`/checkin/stats/${childId}`)
}

// ==================== 学习提醒 ====================
export const reminderApi = {
  getList: (childId: number) =>
    request.get(`/reminder/${childId}`),
  create: (data: { childId: number; type: number; title: string; content?: string; reminderTime?: string }) =>
    request.post('/reminder', data),
  toggle: (reminderId: number, isEnabled: boolean) =>
    request.put(`/reminder/${reminderId}/toggle?isEnabled=${isEnabled}`),
  delete: (reminderId: number) =>
    request.delete(`/reminder/${reminderId}`)
}

// ==================== 成就与激励 ====================
export const achievementApi = {
  getAll: () =>
    request.get('/achievement/all'),
  getChildAchievements: (childId: number) =>
    request.get(`/achievement/child/${childId}`),
  getPoints: (childId: number) =>
    request.get(`/achievement/points/${childId}`),
  getEncouragements: (childId: number) =>
    request.get(`/achievement/encouragements/${childId}`),
  addEncouragement: (data: { childId: number; content: string; reason?: string }) =>
    request.post('/achievement/encouragements', data),
  awardPoints: (data: { childId: number; points: number; reason: string }) =>
    request.post('/achievement/points', data)
}

// ==================== 教育资源同步 ====================
export const educationSyncApi = {
  getStatus: () =>
    request.get('/educationSync/status'),
  sync: () =>
    request.post('/educationSync/sync'),
  getTextbookDetail: (textbookId: number) =>
    request.get(`/educationSync/textbook/${textbookId}`),
  getRecommendations: (childId: number) =>
    request.get(`/educationSync/recommendations/${childId}`)
}
