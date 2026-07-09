<template>
  <view class="child-page">
    <view class="page-title">学习档案</view>

    <view class="form-section">
      <view class="form-item">
        <text class="label">教育阶段</text>
        <picker :range="stageNames" @change="onStageChange">
          <view class="picker-value">{{ selectedStageName || '请选择' }}</view>
        </picker>
      </view>

      <view class="form-item" v-if="grades.length > 0">
        <text class="label">年级</text>
        <picker :range="gradeNames" @change="onGradeChange">
          <view class="picker-value">{{ selectedGradeName || '请选择' }}</view>
        </picker>
      </view>

      <view class="form-item">
        <text class="label">学校名称</text>
        <input v-model="schoolName" placeholder="输入学校名称" class="input" />
      </view>

      <view class="form-item">
        <text class="label">教材版本</text>
        <picker :range="versionNames" @change="onVersionChange">
          <view class="picker-value">{{ selectedVersionName || '请选择' }}</view>
        </picker>
      </view>
    </view>

    <button class="btn-save" @click="handleSave">保存</button>
  </view>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { onLoad } from '@dcloudio/uni-app'
import { childApi, educationApi } from '../../api'

const childId = ref(0)
const stages = ref<any[]>([])
const grades = ref<any[]>([])
const versions = ref<any[]>([])

const stageIndex = ref(-1)
const gradeIndex = ref(-1)
const versionIndex = ref(-1)
const schoolName = ref('')

const stageNames = computed(() => stages.value.map((s: any) => s.name))
const gradeNames = computed(() => grades.value.map((g: any) => g.name))
const versionNames = computed(() => versions.value.map((v: any) => v.name))
const selectedStageName = computed(() => stageIndex.value >= 0 ? stageNames.value[stageIndex.value] : '')
const selectedGradeName = computed(() => gradeIndex.value >= 0 ? gradeNames.value[gradeIndex.value] : '')
const selectedVersionName = computed(() => versionIndex.value >= 0 ? versionNames.value[versionIndex.value] : '')

onLoad((options) => {
  childId.value = Number(options?.id || 0)
})

onMounted(async () => {
  const [stagesRes, versionsRes] = await Promise.all([
    educationApi.getStages(),
    educationApi.getTextbookVersions()
  ])
  stages.value = stagesRes.data || []
  versions.value = versionsRes.data || []
})

async function onStageChange(e: any) {
  stageIndex.value = Number(e.detail.value)
  const stageId = stages.value[stageIndex.value].id
  const res = await educationApi.getGrades(stageId)
  grades.value = res.data || []
  gradeIndex.value = -1
}

function onGradeChange(e: any) {
  gradeIndex.value = Number(e.detail.value)
}

function onVersionChange(e: any) {
  versionIndex.value = Number(e.detail.value)
}

async function handleSave() {
  try {
    await childApi.updateProfile({
      childId: childId.value,
      stageId: stageIndex.value >= 0 ? stages.value[stageIndex.value].id : undefined,
      gradeId: gradeIndex.value >= 0 ? grades.value[gradeIndex.value].id : undefined,
      schoolName: schoolName.value || undefined,
      textbookVersionId: versionIndex.value >= 0 ? versions.value[versionIndex.value].id : undefined
    })
    uni.showToast({ title: '保存成功' })
    setTimeout(() => uni.navigateBack(), 500)
  } catch (e) {
    // handled
  }
}
</script>

<style scoped>
.child-page {
  min-height: 100vh;
  background: #f5f7fa;
  padding: 20px;
}

.page-title {
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 20px;
}

.form-section {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
}

.form-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 0;
  border-bottom: 1px solid #f0f0f0;
}

.form-item:last-child {
  border-bottom: none;
}

.label {
  font-size: 14px;
  color: #333;
  width: 80px;
}

.picker-value {
  font-size: 14px;
  color: #666;
}

.input {
  flex: 1;
  text-align: right;
  font-size: 14px;
}

.btn-save {
  margin-top: 24px;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 14px;
  font-size: 16px;
}
</style>
