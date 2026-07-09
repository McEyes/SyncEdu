<template>
  <div class="education-view">
    <h2>教育体系</h2>

    <el-card>
      <template #header>
        <span>教育阶段与年级</span>
      </template>

      <el-collapse v-model="expandedStages">
        <el-collapse-item v-for="stage in stages" :key="stage.id" :title="stage.name" :name="stage.id">
          <el-table :data="stage.grades" stripe>
            <el-table-column prop="name" label="年级" />
            <el-table-column prop="sortOrder" label="排序" width="80" />
          </el-table>
        </el-collapse-item>
      </el-collapse>
    </el-card>

    <el-card style="margin-top: 20px">
      <template #header>
        <span>教材版本</span>
      </template>

      <el-table :data="textbookVersions" stripe>
        <el-table-column prop="name" label="版本名称" />
        <el-table-column prop="publisher" label="出版社" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { educationApi } from '../../api'

const stages = ref<any[]>([])
const textbookVersions = ref<any[]>([])
const expandedStages = ref<number[]>([])

onMounted(async () => {
  const [stagesRes, versionsRes]: any[] = await Promise.all([
    educationApi.getStages(),
    educationApi.getTextbookVersions()
  ])
  stages.value = stagesRes.data || []
  textbookVersions.value = versionsRes.data || []
  // 默认展开所有阶段
  expandedStages.value = stages.value.map((s: any) => s.id)
})
</script>

<style scoped>
.education-view h2 {
  margin-bottom: 20px;
}
</style>
