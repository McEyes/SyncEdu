<template>
  <div class="education-sync-view">
    <div class="page-header">
      <h2>教育资源同步</h2>
      <el-button type="primary" @click="syncData" :loading="syncing">
        <el-icon><Refresh /></el-icon>同步数据
      </el-button>
    </div>

    <!-- 同步状态 -->
    <el-card class="sync-status">
      <el-descriptions :column="3" border>
        <el-descriptions-item label="数据源">{{ syncStatus.providerName }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="syncStatus.isAvailable ? 'success' : 'info'">
            {{ syncStatus.isAvailable ? '已连接' : '未连接' }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="上次同步">{{ syncStatus.lastSyncAt ? formatDateTime(syncStatus.lastSyncAt) : '从未同步' }}</el-descriptions-item>
        <el-descriptions-item label="教育阶段数">{{ syncStatus.stagesCount }}</el-descriptions-item>
        <el-descriptions-item label="教材数量">{{ syncStatus.textbooksCount }}</el-descriptions-item>
      </el-descriptions>
    </el-card>

    <!-- 学习推荐 -->
    <el-card style="margin-top:16px">
      <template #header><h3>今日学习推荐</h3></template>
      <div v-if="recommendations.encouragementMessage" class="encourage-msg">
        {{ recommendations.encouragementMessage }}
      </div>
      <el-row :gutter="12" style="margin-top:12px">
        <el-col :span="8" v-for="lesson in recommendations.todayLessons" :key="lesson.lessonId">
          <el-card shadow="hover" class="lesson-card">
            <div class="lesson-subject">{{ lesson.subjectName }}</div>
            <div class="lesson-title">{{ lesson.title }}</div>
            <div class="lesson-reason">{{ lesson.reason }}</div>
          </el-card>
        </el-col>
      </el-row>
      <el-empty v-if="recommendations.todayLessons.length === 0" description="暂无推荐，请先设置学习档案" :image-size="80" />
    </el-card>

    <!-- 教材详情浏览 -->
    <el-card style="margin-top:16px">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center">
          <h3>教材内容浏览</h3>
          <el-input-number v-model="textbookId" :min="1" size="small" @change="loadTextbookDetail" />
        </div>
      </template>
      <div v-if="textbookDetail">
        <div class="textbook-info">
          <h4>{{ textbookDetail.title }}</h4>
          <p>{{ textbookDetail.versionName }} | {{ textbookDetail.gradeName }} | {{ textbookDetail.subjectName }}</p>
        </div>
        <el-collapse accordion>
          <el-collapse-item v-for="chapter in textbookDetail.chapters" :key="chapter.id" :title="chapter.title" :name="chapter.id">
            <el-table :data="chapter.lessons" size="small">
              <el-table-column prop="title" label="课时" />
              <el-table-column label="时长" width="100">
                <template #default="{ row }">{{ row.durationMinutes ? row.durationMinutes + '分钟' : '-' }}</template>
              </el-table-column>
            </el-table>
          </el-collapse-item>
        </el-collapse>
      </div>
      <el-empty v-else description="请输入教材ID查看内容" :image-size="80" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { educationSyncApi } from '../../api'
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()

interface SyncStatus {
  providerName: string
  isAvailable: boolean
  lastSyncAt?: string
  stagesCount: number
  textbooksCount: number
}

interface LessonDetail { id: number; title: string; sortOrder: number; durationMinutes?: number }
interface ChapterDetail { id: number; title: string; sortOrder: number; lessons: LessonDetail[] }
interface TextbookDetail {
  id: number; title: string; versionName: string; gradeName: string; subjectName: string
  chapters: ChapterDetail[]
}

const syncing = ref(false)
const syncStatus = ref<SyncStatus>({ providerName: '-', isAvailable: false, stagesCount: 0, textbooksCount: 0 })
const recommendations = ref<{ todayLessons: any[]; encouragementMessage?: string }>({ todayLessons: [] })
const textbookId = ref(1)
const textbookDetail = ref<TextbookDetail | null>(null)

const loadStatus = async () => {
  const res = await educationSyncApi.getStatus()
  syncStatus.value = res.data.data || syncStatus.value
}

const loadRecommendations = async () => {
  if (!childStore.activeChildId) return
  const res = await educationSyncApi.getRecommendations(childStore.activeChildId)
  recommendations.value = res.data.data || { todayLessons: [] }
}

const syncData = async () => {
  syncing.value = true
  try {
    await educationSyncApi.sync()
    ElMessage.success('同步完成')
    await loadStatus()
  } finally {
    syncing.value = false
  }
}

const loadTextbookDetail = async () => {
  const res = await educationSyncApi.getTextbookDetail(textbookId.value)
  textbookDetail.value = res.data.data || null
}

const formatDateTime = (d: string) => new Date(d).toLocaleString()

onMounted(() => {
  loadStatus()
  loadRecommendations()
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.sync-status { margin-bottom: 16px; }
.encourage-msg { font-size: 16px; color: #409eff; padding: 12px; background: #ecf5ff; border-radius: 8px; text-align: center; }
.lesson-card { margin-bottom: 8px; }
.lesson-subject { font-size: 12px; color: #999; }
.lesson-title { font-size: 15px; font-weight: bold; margin: 4px 0; }
.lesson-reason { font-size: 12px; color: #67c23a; }
.textbook-info { margin-bottom: 16px; }
.textbook-info h4 { margin: 0; }
.textbook-info p { color: #999; font-size: 13px; }
</style>
