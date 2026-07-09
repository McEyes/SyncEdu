<template>
  <div class="learning-plan-view">
    <div class="page-header">
      <h2>学习计划</h2>
      <el-button type="primary" @click="openCreateDialog">
        <el-icon><Plus /></el-icon>新建计划
      </el-button>
    </div>

    <!-- 学习计划列表 -->
    <el-row :gutter="16">
      <el-col :span="8" v-for="plan in plans" :key="plan.id">
        <el-card shadow="hover" class="plan-card" @click="selectPlan(plan)">
          <template #header>
            <div class="card-header">
              <span class="plan-title">{{ plan.title }}</span>
              <el-tag size="small">{{ plan.progressPercent }}%</el-tag>
            </div>
          </template>
          <p class="textbook-name">{{ plan.gradeName }} · {{ plan.subjectName }} · {{ plan.semesterName }}</p>
          <p class="textbook-name">{{ plan.textbookTitle }}</p>
          <el-progress :percentage="plan.progressPercent" :stroke-width="8" />
          <div class="plan-meta">
            <span>课时: {{ plan.completedLessons }}/{{ plan.totalLessons }}</span>
            <span v-if="plan.endDate">截止: {{ formatDate(plan.endDate) }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8" v-if="plans.length === 0">
        <el-empty description="暂无学习计划，点击上方按钮创建" />
      </el-col>
    </el-row>

    <!-- 学习进度详情 -->
    <el-dialog v-model="showProgress" :title="selectedPlan?.title + ' - 学习进度'" width="700px">
      <el-table :data="progressList" stripe>
        <el-table-column prop="chapterTitle" label="章节" width="200" />
        <el-table-column prop="lessonTitle" label="课时" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)">{{ statusText(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="学习时长" width="100">
          <template #default="{ row }">
            {{ row.studyMinutes ? row.studyMinutes + '分钟' : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="markProgress(row)">
              {{ row.status === 3 ? '已完成' : '标记完成' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <!-- 创建计划对话框 -->
    <el-dialog v-model="showCreateDialog" title="新建学习计划" width="560px">
      <el-form :model="createForm" label-width="90px">
        <el-form-item label="选择年级" required>
          <el-select v-model="createForm.gradeId" placeholder="请选择年级" @change="onGradeChange" style="width: 100%">
            <el-option v-for="s in stages" :key="s.id" :label="s.name" :value="s.id" disabled />
            <template v-for="s in stages" :key="s.id">
              <el-option v-for="g in s.grades" :key="g.id" :label="s.name + ' - ' + g.name" :value="g.id" />
            </template>
          </el-select>
        </el-form-item>

        <el-form-item label="选择科目" required>
          <el-select v-model="createForm.subjectId" placeholder="请选择科目" style="width: 100%">
            <el-option v-for="sub in subjects" :key="sub.id" :label="sub.name" :value="sub.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="教材版本" required>
          <el-select v-model="createForm.textbookVersionId" placeholder="请选择教材版本" style="width: 100%">
            <el-option v-for="v in textbookVersions" :key="v.id" :label="v.name + (v.publisher ? ' (' + v.publisher + ')' : '')" :value="v.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="学期" required>
          <el-radio-group v-model="createForm.semester">
            <el-radio-button :value="1">上册</el-radio-button>
            <el-radio-button :value="2">下册</el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="学习时长">
          <div class="duration-shortcuts">
            <el-button v-for="d in durationOptions" :key="d.days" :type="createForm.durationDays === d.days ? 'primary' : 'default'" @click="selectDuration(d.days)">
              {{ d.label }}
            </el-button>
          </div>
        </el-form-item>

        <el-form-item label="开始日期">
          <el-date-picker v-model="createForm.startDate" type="date" value-format="YYYY-MM-DD" placeholder="默认今天" style="width: 100%" />
        </el-form-item>

        <el-form-item label="结束日期">
          <el-date-picker v-model="createForm.endDate" type="date" value-format="YYYY-MM-DD" placeholder="由时长自动计算" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreateDialog = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="createPlan">创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { learningApi, educationApi } from '../../api'
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()

interface Plan {
  id: number
  title: string
  textbookTitle: string
  gradeName: string
  subjectName: string
  semesterName: string
  progressPercent: number
  completedLessons: number
  totalLessons: number
  endDate?: string
}

interface Progress {
  id: number
  planId: number
  lessonId: number
  lessonTitle: string
  chapterTitle: string
  status: number
  studyMinutes?: number
}

interface Stage { id: number; name: string; grades: { id: number; name: string }[] }
interface Subject { id: number; name: string }
interface Version { id: number; name: string; publisher?: string }

const durationOptions = [
  { label: '7天', days: 7 },
  { label: '21天', days: 21 },
  { label: '1个月', days: 30 },
  { label: '3个月', days: 90 }
]

const plans = ref<Plan[]>([])
const progressList = ref<Progress[]>([])
const showCreateDialog = ref(false)
const showProgress = ref(false)
const selectedPlan = ref<Plan | null>(null)
const creating = ref(false)

const stages = ref<Stage[]>([])
const subjects = ref<Subject[]>([])
const textbookVersions = ref<Version[]>([])

const createForm = ref({
  gradeId: null as number | null,
  subjectId: null as number | null,
  textbookVersionId: null as number | null,
  semester: 1,
  durationDays: 21 as number | null,
  startDate: '',
  endDate: ''
})

const loadPlans = async () => {
  if (!childStore.activeChildId) return
  const res = await learningApi.getPlans(childStore.activeChildId)
  plans.value = res.data.data || []
}

const loadBaseData = async () => {
  const [stagesRes, subjectsRes, versionsRes] = await Promise.all([
    educationApi.getStages(),
    educationApi.getSubjects(),
    educationApi.getTextbookVersions()
  ])
  stages.value = stagesRes.data.data || []
  subjects.value = subjectsRes.data.data || []
  textbookVersions.value = versionsRes.data.data || []
}

const onGradeChange = () => {
  // 年级变化时不需要特殊处理，只是更新选中值
}

const selectDuration = (days: number) => {
  if (createForm.value.durationDays === days) {
    createForm.value.durationDays = null
    createForm.value.endDate = ''
  } else {
    createForm.value.durationDays = days
    // 自动计算结束日期
    const start = createForm.value.startDate ? new Date(createForm.value.startDate) : new Date()
    const end = new Date(start)
    end.setDate(end.getDate() + days - 1)
    createForm.value.endDate = end.toISOString().slice(0, 10)
  }
}

const openCreateDialog = () => {
  createForm.value = {
    gradeId: null,
    subjectId: null,
    textbookVersionId: null,
    semester: 1,
    durationDays: 21,
    startDate: '',
    endDate: ''
  }
  showCreateDialog.value = true
}

const createPlan = async () => {
  if (!childStore.activeChildId) return
  if (!createForm.value.gradeId) return ElMessage.warning('请选择年级')
  if (!createForm.value.subjectId) return ElMessage.warning('请选择科目')
  if (!createForm.value.textbookVersionId) return ElMessage.warning('请选择教材版本')

  creating.value = true
  try {
    const res = await learningApi.createPlan({
      childId: childStore.activeChildId,
      gradeId: createForm.value.gradeId,
      subjectId: createForm.value.subjectId,
      textbookVersionId: createForm.value.textbookVersionId,
      semester: createForm.value.semester,
      durationDays: createForm.value.durationDays || undefined,
      startDate: createForm.value.startDate || undefined,
      endDate: createForm.value.endDate || undefined
    })
    if (res.data.code === 200) {
      ElMessage.success('计划创建成功')
      showCreateDialog.value = false
      await loadPlans()
    } else {
      ElMessage.error(res.data.message || '创建失败')
    }
  } finally {
    creating.value = false
  }
}

const selectPlan = async (plan: Plan) => {
  selectedPlan.value = plan
  showProgress.value = true
  if (!childStore.activeChildId) return
  const res = await learningApi.getProgress(plan.id, childStore.activeChildId)
  progressList.value = res.data.data || []
}

const markProgress = async (row: Progress) => {
  if (!childStore.activeChildId || !selectedPlan.value) return
  await learningApi.updateProgress({
    planId: selectedPlan.value.id,
    lessonId: row.lessonId,
    childId: childStore.activeChildId,
    status: 3,
    studyMinutes: row.studyMinutes || 30
  })
  ElMessage.success('已标记完成')
  await selectPlan(selectedPlan.value!)
  await loadPlans()
}

const statusText = (status: number) => {
  const map: Record<number, string> = { 1: '未开始', 2: '学习中', 3: '已完成' }
  return map[status] || '未知'
}

const statusType = (status: number) => {
  const map: Record<number, string> = { 1: 'info', 2: 'warning', 3: 'success' }
  return map[status] || 'info'
}

const formatDate = (date: string) => new Date(date).toLocaleDateString()

onMounted(() => {
  loadBaseData()
  loadPlans()
})
</script>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
.plan-card {
  margin-bottom: 16px;
  cursor: pointer;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.plan-title {
  font-weight: bold;
}
.textbook-name {
  color: #666;
  margin: 4px 0;
  font-size: 13px;
}
.plan-meta {
  display: flex;
  justify-content: space-between;
  margin-top: 12px;
  font-size: 13px;
  color: #999;
}
.duration-shortcuts {
  display: flex;
  gap: 8px;
}
</style>
