<template>
  <div class="child-profile">
    <div class="page-header">
      <h2>学习档案设置</h2>
      <el-button @click="$router.back()">返回</el-button>
    </div>

    <el-card>
      <template #header><span style="font-weight:bold">基本信息</span></template>
      <el-form :model="profileForm" label-width="120px">
        <el-form-item label="教育阶段">
          <el-select v-model="profileForm.stageId" placeholder="选择教育阶段" @change="onStageChange">
            <el-option v-for="stage in stages" :key="stage.id" :label="stage.name" :value="stage.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="年级" v-if="grades.length > 0">
          <el-select v-model="profileForm.gradeId" placeholder="选择年级">
            <el-option v-for="grade in grades" :key="grade.id" :label="grade.name" :value="grade.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="学校名称">
          <el-input v-model="profileForm.schoolName" placeholder="输入学校名称" />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSave">保存基本信息</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card style="margin-top: 20px">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center">
          <span style="font-weight:bold">各科目教材版本</span>
          <el-button type="primary" size="small" @click="showAddSubject = true">
            <el-icon><Plus /></el-icon>添加科目
          </el-button>
        </div>
      </template>

      <el-table :data="subjectConfigs" stripe v-if="subjectConfigs.length > 0">
        <el-table-column prop="subjectName" label="科目" width="120" />
        <el-table-column label="教材版本">
          <template #default="{ row }">
            <el-select v-model="row.textbookVersionId" size="small" @change="onConfigVersionChange(row)" style="width: 200px">
              <el-option v-for="v in textbookVersions" :key="v.id" :label="v.name" :value="v.id" />
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }">
            <el-button size="small" type="danger" link @click="deleteConfig(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-else description="尚未配置科目教材版本，点击上方按钮添加" :image-size="60" />
    </el-card>

    <!-- 添加科目对话框 -->
    <el-dialog v-model="showAddSubject" title="添加科目教材配置" width="450px">
      <el-form label-width="90px">
        <el-form-item label="选择科目">
          <el-select v-model="newConfig.subjectId" placeholder="选择科目" style="width: 100%">
            <el-option v-for="s in availableSubjects" :key="s.id" :label="s.name" :value="s.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="教材版本">
          <el-select v-model="newConfig.textbookVersionId" placeholder="选择教材版本" style="width: 100%">
            <el-option v-for="v in textbookVersions" :key="v.id" :label="v.name" :value="v.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddSubject = false">取消</el-button>
        <el-button type="primary" @click="addSubjectConfig">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { childApi, educationApi } from '../../api'

const route = useRoute()
const childId = Number(route.params.id)

const stages = ref<any[]>([])
const grades = ref<any[]>([])
const textbookVersions = ref<any[]>([])
const subjects = ref<any[]>([])
const subjectConfigs = ref<any[]>([])
const showAddSubject = ref(false)

const profileForm = reactive({
  stageId: undefined as number | undefined,
  gradeId: undefined as number | undefined,
  schoolName: ''
})

const newConfig = reactive({
  subjectId: undefined as number | undefined,
  textbookVersionId: undefined as number | undefined
})

// 可选科目 = 全部科目 - 已配置的科目
const availableSubjects = computed(() => {
  const configuredIds = subjectConfigs.value.map((c: any) => c.subjectId)
  return subjects.value.filter(s => !configuredIds.includes(s.id))
})

onMounted(async () => {
  const [stagesRes, versionsRes, subjectsRes, configsRes]: any[] = await Promise.all([
    educationApi.getStages(),
    educationApi.getTextbookVersions(),
    educationApi.getSubjects(),
    educationApi.getChildSubjectConfigs(childId)
  ])
  stages.value = stagesRes.data.data || stagesRes.data || []
  textbookVersions.value = versionsRes.data.data || versionsRes.data || []
  subjects.value = subjectsRes.data.data || subjectsRes.data || []
  subjectConfigs.value = configsRes.data.data || configsRes.data || []
})

async function onStageChange(stageId: number) {
  profileForm.gradeId = undefined
  const res: any = await educationApi.getGrades(stageId)
  grades.value = res.data?.data || res.data || []
}

async function handleSave() {
  try {
    await childApi.updateProfile({
      childId,
      stageId: profileForm.stageId,
      gradeId: profileForm.gradeId,
      schoolName: profileForm.schoolName || undefined
    })
    ElMessage.success('学习档案更新成功')
  } catch {
    // handled
  }
}

async function addSubjectConfig() {
  if (!newConfig.subjectId) return ElMessage.warning('请选择科目')
  if (!newConfig.textbookVersionId) return ElMessage.warning('请选择教材版本')

  await educationApi.setChildSubjectConfig({
    childId,
    subjectId: newConfig.subjectId,
    textbookVersionId: newConfig.textbookVersionId,
    gradeId: profileForm.gradeId || 0
  })
  ElMessage.success('配置成功')
  showAddSubject.value = false
  newConfig.subjectId = undefined
  newConfig.textbookVersionId = undefined
  await refreshConfigs()
}

async function onConfigVersionChange(row: any) {
  await educationApi.setChildSubjectConfig({
    childId,
    subjectId: row.subjectId,
    textbookVersionId: row.textbookVersionId,
    gradeId: profileForm.gradeId || row.gradeId || 0
  })
  ElMessage.success('更新成功')
}

async function deleteConfig(configId: number) {
  await educationApi.deleteChildSubjectConfig(configId)
  ElMessage.success('已删除')
  await refreshConfigs()
}

async function refreshConfigs() {
  const res: any = await educationApi.getChildSubjectConfigs(childId)
  subjectConfigs.value = res.data.data || res.data || []
}
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}
</style>
