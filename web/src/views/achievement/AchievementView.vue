<template>
  <div class="achievement-view">
    <div class="page-header">
      <h2>成就与激励</h2>
      <el-button type="primary" @click="showAwardDialog = true">
        <el-icon><Plus /></el-icon>奖励积分
      </el-button>
    </div>

    <!-- 积分概览 -->
    <el-row :gutter="16" class="points-overview">
      <el-col :span="8">
        <el-card shadow="hover" class="points-card total">
          <div class="points-value">{{ points.totalPoints }}</div>
          <div class="points-label">总积分</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="points-card">
          <div class="points_value">{{ points.thisWeekPoints }}</div>
          <div class="points-label">本周积分</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="points-card">
          <div class="points_value">{{ points.todayPoints }}</div>
          <div class="points-label">今日积分</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 已获成就 -->
    <el-card style="margin-top:16px">
      <template #header><h3>已获成就</h3></template>
      <el-row :gutter="12">
        <el-col :span="4" v-for="ach in childAchievements" :key="ach.id">
          <div class="achievement-item">
            <div class="achievement-icon">{{ ach.icon || '🏆' }}</div>
            <div class="achievement-name">{{ ach.achievementName }}</div>
            <div class="achievement-date">{{ formatDate(ach.achievedAt) }}</div>
          </div>
        </el-col>
      </el-row>
      <el-empty v-if="childAchievements.length === 0" description="还没有获得成就，继续加油！" :image-size="80" />
    </el-card>

    <!-- 全部成就定义 -->
    <el-card style="margin-top:16px">
      <template #header><h3>全部成就</h3></template>
      <el-row :gutter="12">
        <el-col :span="4" v-for="ach in allAchievements" :key="ach.id">
          <div class="achievement-item" :class="{ locked: !isUnlocked(ach.id) }">
            <div class="achievement-icon">{{ ach.icon || '🏆' }}</div>
            <div class="achievement-name">{{ ach.name }}</div>
            <div class="achievement-desc">{{ ach.description || '未解锁' }}</div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 积分流水 -->
    <el-card style="margin-top:16px">
      <template #header><h3>积分记录</h3></template>
      <el-table :data="points.recentTransactions" stripe>
        <el-table-column prop="reason" label="原因" />
        <el-table-column label="积分" width="100">
          <template #default="{ row }">
            <span :style="{ color: row.points > 0 ? '#67c23a' : '#f56c6c' }">
              {{ row.points > 0 ? '+' : '' }}{{ row.points }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="时间" width="160">
          <template #default="{ row }">{{ formatDateTime(row.transactionDate) }}</template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 鼓励记录 -->
    <el-card style="margin-top:16px">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center">
          <h3>鼓励墙</h3>
          <el-button size="small" type="primary" @click="showEncourageDialog = true">发送鼓励</el-button>
        </div>
      </template>
      <div v-for="enc in encouragements" :key="enc.id" class="encouragement-item">
        <div class="enc-content">{{ enc.content }}</div>
        <div class="enc-meta">
          <span v-if="enc.triggerReason">{{ enc.triggerReason }}</span>
          <span>{{ formatDateTime(enc.createdAt) }}</span>
        </div>
      </div>
      <el-empty v-if="encouragements.length === 0" description="还没有鼓励记录" :image-size="80" />
    </el-card>

    <!-- 奖励积分对话框 -->
    <el-dialog v-model="showAwardDialog" title="奖励积分" width="400px">
      <el-form :model="awardForm" label-width="80px">
        <el-form-item label="积分数量">
          <el-input-number v-model="awardForm.points" :min="1" :max="100" />
        </el-form-item>
        <el-form-item label="奖励原因">
          <el-input v-model="awardForm.reason" placeholder="如：完成作业" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAwardDialog = false">取消</el-button>
        <el-button type="primary" @click="awardPoints">确认</el-button>
      </template>
    </el-dialog>

    <!-- 发送鼓励对话框 -->
    <el-dialog v-model="showEncourageDialog" title="发送鼓励" width="400px">
      <el-form :model="encourageForm" label-width="80px">
        <el-form-item label="鼓励内容">
          <el-input v-model="encourageForm.content" type="textarea" :rows="3" placeholder="写一句鼓励的话..." />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showEncourageDialog = false">取消</el-button>
        <el-button type="primary" @click="sendEncouragement">发送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { achievementApi } from '../../api'
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()

interface Achievement { id: number; name: string; description?: string; icon?: string; type: number; threshold?: number }
interface ChildAch { id: number; achievementId: number; achievementName: string; icon?: string; achievedAt: string }
interface Encouragement { id: number; content: string; triggerReason?: string; createdAt: string }

const allAchievements = ref<Achievement[]>([])
const childAchievements = ref<ChildAch[]>([])
const points = ref({ totalPoints: 0, thisWeekPoints: 0, todayPoints: 0, recentTransactions: [] as any[] })
const encouragements = ref<Encouragement[]>([])
const showAwardDialog = ref(false)
const showEncourageDialog = ref(false)
const awardForm = ref({ points: 10, reason: '' })
const encourageForm = ref({ content: '' })

const loadData = async () => {
  if (!childStore.activeChildId) return
  const cid = childStore.activeChildId
  const [allAchRes, childAchRes, pointsRes, encRes] = await Promise.all([
    achievementApi.getAll(),
    achievementApi.getChildAchievements(cid),
    achievementApi.getPoints(cid),
    achievementApi.getEncouragements(cid)
  ])
  allAchievements.value = allAchRes.data.data || []
  childAchievements.value = childAchRes.data.data || []
  points.value = pointsRes.data.data || { totalPoints: 0, thisWeekPoints: 0, todayPoints: 0, recentTransactions: [] }
  encouragements.value = encRes.data.data || []
}

const isUnlocked = (achId: number) => childAchievements.value.some(a => a.achievementId === achId)

const awardPoints = async () => {
  if (!childStore.activeChildId) return
  await achievementApi.awardPoints({ childId: childStore.activeChildId, points: awardForm.value.points, reason: awardForm.value.reason })
  ElMessage.success(`已奖励 ${awardForm.value.points} 积分`)
  showAwardDialog.value = false
  awardForm.value = { points: 10, reason: '' }
  await loadData()
}

const sendEncouragement = async () => {
  if (!childStore.activeChildId) return
  await achievementApi.addEncouragement({ childId: childStore.activeChildId, content: encourageForm.value.content })
  ElMessage.success('鼓励已发送')
  showEncourageDialog.value = false
  encourageForm.value = { content: '' }
  await loadData()
}

const formatDate = (d: string) => new Date(d).toLocaleDateString()
const formatDateTime = (d: string) => new Date(d).toLocaleString()

onMounted(loadData)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.points-card { text-align: center; }
.points_value, .points-value { font-size: 32px; font-weight: bold; color: #e6a23c; }
.points-card.total .points-value { color: #409eff; }
.points-label { font-size: 13px; color: #999; margin-top: 4px; }
.achievement-item { text-align: center; padding: 12px; }
.achievement-icon { font-size: 36px; }
.achievement-name { font-size: 13px; font-weight: bold; margin-top: 4px; }
.achievement-date, .achievement-desc { font-size: 12px; color: #999; margin-top: 2px; }
.achievement-item.locked { opacity: 0.4; }
.encouragement-item { padding: 12px; border-bottom: 1px solid #f0f0f0; }
.enc-content { font-size: 15px; }
.enc-meta { font-size: 12px; color: #999; margin-top: 4px; display: flex; justify-content: space-between; }
</style>
