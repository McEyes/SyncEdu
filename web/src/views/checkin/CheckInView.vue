<template>
  <div class="checkin-view">
    <div class="page-header">
      <h2>学习打卡</h2>
    </div>

    <!-- 打卡统计 -->
    <el-row :gutter="16" class="stats-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.totalDays }}</div>
          <div class="stat-label">累计打卡天数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card streak">
          <div class="stat-value">{{ stats.currentStreak }}</div>
          <div class="stat-label">连续打卡天数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.longestStreak }}</div>
          <div class="stat-label">最长连续天数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" :class="{ today: stats.checkedInToday }">
          <div class="stat-value">{{ stats.checkedInToday ? '已打卡' : '未打卡' }}</div>
          <div class="stat-label">今日状态</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 打卡操作区 -->
    <el-card class="checkin-action">
      <h3>立即打卡</h3>
      <el-form :model="checkinForm" label-width="80px">
        <el-form-item label="打卡类型">
          <el-radio-group v-model="checkinForm.type">
            <el-radio :value="1">文字打卡</el-radio>
            <el-radio :value="2">拍照打卡</el-radio>
            <el-radio :value="3">视频打卡</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="打卡内容">
          <el-input v-model="checkinForm.content" type="textarea" :rows="3" placeholder="记录今天的学习内容..." />
        </el-form-item>

        <!-- 摄像头/拍照区域 -->
        <el-form-item v-if="checkinForm.type === 2 || checkinForm.type === 3" label="拍摄验证">
          <div class="camera-area">
            <video v-show="cameraActive" ref="videoRef" width="320" height="240" autoplay></video>
            <canvas v-show="capturedImage" ref="canvasRef" width="320" height="240" style="display:none"></canvas>
            <img v-if="capturedImage && !cameraActive" :src="capturedImage" class="captured-img" />
            <div class="camera-btns">
              <el-button v-if="!cameraActive && !capturedImage" type="primary" @click="startCamera">
                <el-icon><Camera /></el-icon>打开摄像头
              </el-button>
              <el-button v-if="cameraActive" type="success" @click="capturePhoto">拍照</el-button>
              <el-button v-if="cameraActive" @click="stopCamera">关闭</el-button>
              <el-button v-if="capturedImage && !cameraActive" @click="retakePhoto">重拍</el-button>
            </div>
            <el-checkbox v-if="capturedImage" v-model="enableFaceVerify" style="margin-top:8px">
              启用人脸验证
            </el-checkbox>
          </div>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" size="large" @click="submitCheckIn" :disabled="stats.checkedInToday">
            {{ stats.checkedInToday ? '今日已打卡' : '提交打卡' }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 打卡日历 -->
    <el-card class="calendar-card" style="margin-top:16px">
      <template #header><h3>打卡日历（近30天）</h3></template>
      <div class="calendar-grid">
        <div v-for="day in stats.calendar" :key="day.date" class="calendar-day" :class="{ checked: day.hasCheckIn }">
          <div class="day-date">{{ formatDay(day.date) }}</div>
          <div class="day-dot" :class="typeClass(day.type)"></div>
        </div>
      </div>
    </el-card>

    <!-- 打卡记录列表 -->
    <el-card style="margin-top:16px">
      <template #header><h3>打卡记录</h3></template>
      <el-timeline>
        <el-timeline-item v-for="record in records" :key="record.id" :timestamp="formatDateTime(record.checkInDate)" placement="top">
          <el-card>
            <el-tag size="small" :type="record.type === 2 ? 'success' : record.type === 3 ? 'warning' : 'info'">
              {{ record.type === 1 ? '文字' : record.type === 2 ? '拍照' : '视频' }}
            </el-tag>
            <p style="margin-top:8px">{{ record.content }}</p>
            <div v-if="record.faceVerified" style="margin-top:4px">
              <el-tag type="success" size="small">人脸已验证</el-tag>
            </div>
          </el-card>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-if="records.length === 0" description="暂无打卡记录" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Camera } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { checkInApi } from '../../api'
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()

interface CheckInRecord {
  id: number
  type: number
  content?: string
  checkInDate: string
  faceVerified?: boolean
}

interface CalendarDay {
  date: string
  hasCheckIn: boolean
  type: number
}

const stats = ref<{
  totalDays: number
  currentStreak: number
  longestStreak: number
  thisWeekDays: number
  checkedInToday: boolean
  calendar: CalendarDay[]
}>({
  totalDays: 0, currentStreak: 0, longestStreak: 0, thisWeekDays: 0, checkedInToday: false, calendar: []
})

const records = ref<CheckInRecord[]>([])
const checkinForm = ref({ type: 1, content: '' })

const cameraActive = ref(false)
const capturedImage = ref<string | null>(null)
const enableFaceVerify = ref(true)
const videoRef = ref<HTMLVideoElement | null>(null)
const canvasRef = ref<HTMLCanvasElement | null>(null)
let mediaStream: MediaStream | null = null

const loadData = async () => {
  if (!childStore.activeChildId) return
  const [statsRes, recordsRes] = await Promise.all([
    checkInApi.getStats(childStore.activeChildId),
    checkInApi.getList(childStore.activeChildId)
  ])
  stats.value = statsRes.data.data || stats.value
  records.value = recordsRes.data.data || []
}

const startCamera = async () => {
  try {
    mediaStream = await navigator.mediaDevices.getUserMedia({ video: true })
    if (videoRef.value) {
      videoRef.value.srcObject = mediaStream
    }
    cameraActive.value = true
    capturedImage.value = null
  } catch {
    ElMessage.error('无法访问摄像头，请检查权限设置')
  }
}

const capturePhoto = () => {
  if (!videoRef.value || !canvasRef.value) return
  const ctx = canvasRef.value.getContext('2d')
  if (!ctx) return
  ctx.drawImage(videoRef.value, 0, 0, 320, 240)
  capturedImage.value = canvasRef.value.toDataURL('image/jpeg', 0.8)
  stopCamera()
}

const stopCamera = () => {
  if (mediaStream) {
    mediaStream.getTracks().forEach(t => t.stop())
    mediaStream = null
  }
  cameraActive.value = false
}

const retakePhoto = () => {
  capturedImage.value = null
  startCamera()
}

const submitCheckIn = async () => {
  if (!childStore.activeChildId) return

  const data: {
    childId: number
    type: number
    content?: string
    mediaData?: string
    faceImageData?: string
  } = {
    childId: childStore.activeChildId,
    type: checkinForm.value.type,
    content: checkinForm.value.content || undefined
  }

  if ((checkinForm.value.type === 2 || checkinForm.value.type === 3) && capturedImage.value) {
    data.mediaData = capturedImage.value.split(',')[1]
    if (enableFaceVerify.value) {
      data.faceImageData = data.mediaData
    }
  }

  await checkInApi.create(data)
  ElMessage.success('打卡成功！+5积分')
  checkinForm.value = { type: 1, content: '' }
  capturedImage.value = null
  await loadData()
}

const formatDay = (date: string) => new Date(date).getDate().toString()
const formatDateTime = (date: string) => new Date(date).toLocaleString()
const typeClass = (type: number) => type === 1 ? 'text' : type === 2 ? 'photo' : type === 3 ? 'video' : ''

onMounted(loadData)
onUnmounted(stopCamera)
</script>

<style scoped>
.page-header { margin-bottom: 20px; }
.stats-row { margin-bottom: 16px; }
.stat-card { text-align: center; }
.stat-value { font-size: 28px; font-weight: bold; color: #409eff; }
.stat-card.streak .stat-value { color: #e6a23c; }
.stat-card.today.checked .stat-value { color: #67c23a; }
.stat-label { font-size: 13px; color: #999; margin-top: 4px; }
.checkin-action { margin-bottom: 16px; }
.camera-area { width: 100%; }
.camera-btns { margin-top: 8px; }
.captured-img { width: 320px; height: 240px; object-fit: cover; border-radius: 8px; }
.calendar-grid { display: flex; flex-wrap: wrap; gap: 4px; }
.calendar-day { width: 36px; height: 36px; display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 4px; background: #f5f5f5; }
.calendar-day.checked { background: #e8f5e9; }
.day-date { font-size: 12px; }
.day-dot { width: 6px; height: 6px; border-radius: 50%; background: #ccc; }
.day-dot.text { background: #409eff; }
.day-dot.photo { background: #67c23a; }
.day-dot.video { background: #e6a23c; }
</style>
