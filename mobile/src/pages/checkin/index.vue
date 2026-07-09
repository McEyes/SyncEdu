<template>
  <view class="checkin-page">
    <!-- 打卡统计卡片 -->
    <view class="stats-card">
      <view class="stats-row">
        <view class="stat-item">
          <text class="stat-num">{{ stats.totalDays }}</text>
          <text class="stat-label">累计天数</text>
        </view>
        <view class="stat-item">
          <text class="stat-num streak">{{ stats.currentStreak }}</text>
          <text class="stat-label">连续打卡</text>
        </view>
        <view class="stat-item">
          <text class="stat-num">{{ stats.longestStreak }}</text>
          <text class="stat-label">最长连续</text>
        </view>
      </view>
      <view class="today-status" :class="{ checked: stats.checkedInToday }">
        {{ stats.checkedInToday ? '今日已打卡 ✓' : '今日未打卡' }}
      </view>
    </view>

    <!-- 打卡表单 -->
    <view class="checkin-form" v-if="!stats.checkedInToday">
      <view class="form-title">立即打卡</view>
      
      <view class="type-selector">
        <view 
          v-for="t in types" :key="t.value"
          class="type-btn" :class="{ active: checkinType === t.value }"
          @tap="checkinType = t.value"
        >
          {{ t.label }}
        </view>
      </view>

      <textarea 
        class="content-input"
        v-model="checkinContent"
        placeholder="记录今天的学习内容..."
        :maxlength="500"
      />

      <!-- 拍照区域 -->
      <view class="camera-section" v-if="checkinType === 2 || checkinType === 3">
        <image v-if="capturedImage" :src="capturedImage" class="captured-img" mode="aspectFill" />
        <view class="camera-btns">
          <button class="btn-capture" @tap="takePhoto" v-if="!capturedImage">
            拍照
          </button>
          <button class="btn-retake" @tap="takePhoto" v-else>
            重拍
          </button>
        </view>
      </view>

      <button class="btn-submit" @tap="submitCheckIn" :disabled="submitting">
        {{ submitting ? '提交中...' : '提交打卡' }}
      </button>
    </view>

    <!-- 打卡日历（近7天） -->
    <view class="calendar-section">
      <view class="section-title">最近打卡</view>
      <view class="calendar-row">
        <view 
          v-for="day in recentCalendar" :key="day.date"
          class="cal-day" :class="{ checked: day.hasCheckIn }"
        >
          <text class="cal-date">{{ day.dayNum }}</text>
          <view class="cal-dot" :class="{ active: day.hasCheckIn }"></view>
        </view>
      </view>
    </view>

    <!-- 打卡记录 -->
    <view class="records-section">
      <view class="section-title">打卡记录</view>
      <view v-for="record in records" :key="record.id" class="record-item">
        <view class="record-header">
          <text class="record-type">{{ record.type === 1 ? '文字' : record.type === 2 ? '拍照' : '视频' }}</text>
          <text class="record-time">{{ formatTime(record.checkInDate) }}</text>
        </view>
        <text class="record-content" v-if="record.content">{{ record.content }}</text>
        <view v-if="record.faceVerified" class="face-verified">人脸已验证</view>
      </view>
      <view v-if="records.length === 0" class="empty-text">暂无打卡记录</view>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { checkInApi } from '../../api'

const activeChildId = ref(0)
const checkinType = ref(1)
const checkinContent = ref('')
const capturedImage = ref('')
const submitting = ref(false)

const types = [
  { value: 1, label: '文字' },
  { value: 2, label: '拍照' },
  { value: 3, label: '视频' }
]

const stats = ref({
  totalDays: 0, currentStreak: 0, longestStreak: 0,
  thisWeekDays: 0, checkedInToday: false, calendar: [] as any[]
})
const records = ref<any[]>([])

const recentCalendar = computed(() => {
  const cal = stats.value.calendar || []
  return cal.slice(-7).map((d: any) => ({
    date: d.date,
    dayNum: new Date(d.date).getDate(),
    hasCheckIn: d.hasCheckIn
  }))
})

const loadChildId = () => {
  const children = uni.getStorageSync('children')
  if (children && children.length > 0) {
    const active = children.find((c: any) => c.isActive) || children[0]
    activeChildId.value = active.id
  }
}

const loadData = async () => {
  if (!activeChildId.value) return
  try {
    const [statsRes, recordsRes] = await Promise.all([
      checkInApi.getStats(activeChildId.value),
      checkInApi.getList(activeChildId.value)
    ])
    stats.value = statsRes.data || stats.value
    records.value = recordsRes.data || []
  } catch (e) {
    console.error(e)
  }
}

const takePhoto = () => {
  uni.chooseImage({
    count: 1,
    sourceType: ['camera'],
    success: (res) => {
      capturedImage.value = res.tempFilePaths[0]
    }
  })
}

const submitCheckIn = async () => {
  if (!activeChildId.value) return
  submitting.value = true
  try {
    await checkInApi.create({
      childId: activeChildId.value,
      type: checkinType.value,
      content: checkinContent.value || undefined
    })
    uni.showToast({ title: '打卡成功！+5积分', icon: 'success' })
    checkinContent.value = ''
    capturedImage.value = ''
    await loadData()
  } finally {
    submitting.value = false
  }
}

const formatTime = (date: string) => {
  const d = new Date(date)
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours()}:${d.getMinutes().toString().padStart(2, '0')}`
}

onMounted(() => {
  loadChildId()
  loadData()
})
</script>

<style scoped>
.checkin-page { padding: 16px; background: #f5f7fa; min-height: 100vh; }
.stats-card { background: linear-gradient(135deg, #409eff, #67c23a); border-radius: 12px; padding: 20px; color: #fff; margin-bottom: 16px; }
.stats-row { display: flex; justify-content: space-around; }
.stat-item { text-align: center; }
.stat-num { font-size: 28px; font-weight: bold; display: block; }
.stat-num.streak { color: #ffe066; }
.stat-label { font-size: 12px; opacity: 0.8; }
.today-status { text-align: center; margin-top: 12px; font-size: 16px; padding: 8px; background: rgba(255,255,255,0.2); border-radius: 8px; }
.today-status.checked { background: rgba(255,255,255,0.3); }
.checkin-form { background: #fff; border-radius: 12px; padding: 16px; margin-bottom: 16px; }
.form-title { font-size: 18px; font-weight: bold; margin-bottom: 12px; }
.type-selector { display: flex; gap: 8px; margin-bottom: 12px; }
.type-btn { padding: 8px 16px; border-radius: 20px; background: #f0f0f0; font-size: 14px; }
.type-btn.active { background: #409eff; color: #fff; }
.content-input { width: 100%; height: 100px; border: 1px solid #eee; border-radius: 8px; padding: 12px; font-size: 14px; box-sizing: border-box; }
.camera-section { margin-top: 12px; text-align: center; }
.captured-img { width: 200px; height: 150px; border-radius: 8px; }
.camera-btns { margin-top: 8px; }
.btn-capture, .btn-retake { background: #409eff; color: #fff; border: none; border-radius: 8px; padding: 8px 24px; font-size: 14px; }
.btn-submit { margin-top: 16px; background: #67c23a; color: #fff; border: none; border-radius: 8px; padding: 12px; font-size: 16px; width: 100%; }
.btn-submit[disabled] { opacity: 0.6; }
.calendar-section, .records-section { background: #fff; border-radius: 12px; padding: 16px; margin-bottom: 16px; }
.section-title { font-size: 16px; font-weight: bold; margin-bottom: 12px; }
.calendar-row { display: flex; justify-content: space-between; }
.cal-day { text-align: center; width: 40px; }
.cal-date { font-size: 14px; display: block; }
.cal-dot { width: 8px; height: 8px; border-radius: 50%; background: #ddd; margin: 4px auto; }
.cal-dot.active { background: #67c23a; }
.record-item { padding: 12px 0; border-bottom: 1px solid #f0f0f0; }
.record-header { display: flex; justify-content: space-between; }
.record-type { font-size: 13px; color: #409eff; background: #ecf5ff; padding: 2px 8px; border-radius: 4px; }
.record-time { font-size: 12px; color: #999; }
.record-content { display: block; margin-top: 8px; font-size: 14px; color: #333; }
.face-verified { margin-top: 4px; font-size: 12px; color: #67c23a; }
.empty-text { text-align: center; color: #999; padding: 20px; }
</style>
