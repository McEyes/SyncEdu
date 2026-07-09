<template>
  <div class="reminder-view">
    <div class="page-header">
      <h2>学习提醒</h2>
      <el-button type="primary" @click="showCreateDialog = true">
        <el-icon><Plus /></el-icon>新建提醒
      </el-button>
    </div>

    <el-table :data="reminders" stripe>
      <el-table-column prop="title" label="提醒标题" />
      <el-table-column label="类型" width="100">
        <template #default="{ row }">
          <el-tag :type="typeTag(row.type)">{{ typeText(row.type) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="reminderTime" label="提醒时间" width="120" />
      <el-table-column prop="content" label="内容" show-overflow-tooltip />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-switch v-model="row.isEnabled" @change="toggleReminder(row)" />
        </template>
      </el-table-column>
      <el-table-column label="操作" width="80">
        <template #default="{ row }">
          <el-button type="danger" link @click="deleteReminder(row.id)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="reminders.length === 0" description="暂无学习提醒" />

    <!-- 创建提醒对话框 -->
    <el-dialog v-model="showCreateDialog" title="新建学习提醒" width="500px">
      <el-form :model="createForm" label-width="80px">
        <el-form-item label="提醒标题">
          <el-input v-model="createForm.title" placeholder="如：该做数学作业啦" />
        </el-form-item>
        <el-form-item label="提醒类型">
          <el-select v-model="createForm.type">
            <el-option :value="1" label="每日学习" />
            <el-option :value="2" label="打卡提醒" />
            <el-option :value="3" label="目标完成" />
          </el-select>
        </el-form-item>
        <el-form-item label="提醒时间">
          <el-time-picker v-model="createForm.time" format="HH:mm" value-format="HH:mm" placeholder="选择时间" />
        </el-form-item>
        <el-form-item label="提醒内容">
          <el-input v-model="createForm.content" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreateDialog = false">取消</el-button>
        <el-button type="primary" @click="createReminder">创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { reminderApi } from '../../api'
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()

interface Reminder {
  id: number
  title: string
  type: number
  reminderTime?: string
  content?: string
  isEnabled: boolean
}

const reminders = ref<Reminder[]>([])
const showCreateDialog = ref(false)
const createForm = ref({ title: '', type: 1, time: '', content: '' })

const loadReminders = async () => {
  if (!childStore.activeChildId) return
  const res = await reminderApi.getList(childStore.activeChildId)
  reminders.value = res.data.data || []
}

const createReminder = async () => {
  if (!childStore.activeChildId) return
  await reminderApi.create({
    childId: childStore.activeChildId,
    type: createForm.value.type,
    title: createForm.value.title,
    content: createForm.value.content || undefined,
    reminderTime: createForm.value.time || undefined
  })
  ElMessage.success('提醒创建成功')
  showCreateDialog.value = false
  createForm.value = { title: '', type: 1, time: '', content: '' }
  await loadReminders()
}

const toggleReminder = async (row: Reminder) => {
  await reminderApi.toggle(row.id, row.isEnabled)
  ElMessage.success(row.isEnabled ? '已开启' : '已关闭')
}

const deleteReminder = async (id: number) => {
  await ElMessageBox.confirm('确定删除该提醒？', '提示', { type: 'warning' })
  await reminderApi.delete(id)
  ElMessage.success('已删除')
  await loadReminders()
}

const typeText = (type: number) => ({ 1: '每日学习', 2: '打卡提醒', 3: '目标完成' } as Record<number, string>)[type] || '其他'
const typeTag = (type: number) => ({ 1: 'primary', 2: 'success', 3: 'warning' } as Record<number, string>)[type] || 'info'

onMounted(loadReminders)
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
</style>
