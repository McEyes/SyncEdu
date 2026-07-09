<template>
  <view class="reminder-page">
    <view class="page-header">
      <text class="page-title">学习提醒</text>
      <button class="btn-add" @tap="showAddForm = !showAddForm">+ 新建</button>
    </view>

    <!-- 新建提醒表单 -->
    <view class="add-form" v-if="showAddForm">
      <input class="form-input" v-model="newReminder.title" placeholder="提醒标题" />
      <view class="type-row">
        <view 
          v-for="t in types" :key="t.value"
          class="type-tag" :class="{ active: newReminder.type === t.value }"
          @tap="newReminder.type = t.value"
        >{{ t.label }}</view>
      </view>
      <input class="form-input" v-model="newReminder.reminderTime" placeholder="提醒时间 HH:mm" />
      <textarea class="form-textarea" v-model="newReminder.content" placeholder="提醒内容（可选）" />
      <button class="btn-save" @tap="createReminder">保存</button>
    </view>

    <!-- 提醒列表 -->
    <view v-for="item in reminders" :key="item.id" class="reminder-item">
      <view class="reminder-header">
        <text class="reminder-title">{{ item.title }}</text>
        <view class="reminder-actions">
          <switch :checked="item.isEnabled" @change="toggleReminder(item, $event)" color="#409eff" />
          <text class="btn-delete" @tap="deleteReminder(item.id)">删除</text>
        </view>
      </view>
      <view class="reminder-meta">
        <text class="reminder-type">{{ typeText(item.type) }}</text>
        <text class="reminder-time" v-if="item.reminderTime">{{ item.reminderTime }}</text>
      </view>
      <text class="reminder-content" v-if="item.content">{{ item.content }}</text>
    </view>

    <view v-if="reminders.length === 0 && !showAddForm" class="empty-text">
      暂无学习提醒，点击上方按钮创建
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { reminderApi } from '../../api'

const activeChildId = ref(0)
const reminders = ref<any[]>([])
const showAddForm = ref(false)

const types = [
  { value: 1, label: '每日学习' },
  { value: 2, label: '打卡提醒' },
  { value: 3, label: '目标完成' }
]

const newReminder = ref({
  title: '',
  type: 1,
  reminderTime: '',
  content: ''
})

const loadChildId = () => {
  const children = uni.getStorageSync('children')
  if (children && children.length > 0) {
    const active = children.find((c: any) => c.isActive) || children[0]
    activeChildId.value = active.id
  }
}

const loadReminders = async () => {
  if (!activeChildId.value) return
  try {
    const res = await reminderApi.getList(activeChildId.value)
    reminders.value = res.data || []
  } catch (e) {
    console.error(e)
  }
}

const createReminder = async () => {
  if (!activeChildId.value || !newReminder.value.title) return
  try {
    await reminderApi.create({
      childId: activeChildId.value,
      type: newReminder.value.type,
      title: newReminder.value.title,
      content: newReminder.value.content || undefined,
      reminderTime: newReminder.value.reminderTime || undefined
    })
    uni.showToast({ title: '创建成功', icon: 'success' })
    showAddForm.value = false
    newReminder.value = { title: '', type: 1, reminderTime: '', content: '' }
    await loadReminders()
  } catch (e) {
    console.error(e)
  }
}

const toggleReminder = async (item: any, e: any) => {
  const isEnabled = e.detail.value
  try {
    await reminderApi.toggle(item.id, isEnabled)
    item.isEnabled = isEnabled
  } catch (e) {
    console.error(e)
  }
}

const deleteReminder = async (id: number) => {
  try {
    await reminderApi.delete(id)
    uni.showToast({ title: '已删除', icon: 'success' })
    await loadReminders()
  } catch (e) {
    console.error(e)
  }
}

const typeText = (type: number) => {
  const map: Record<number, string> = { 1: '每日学习', 2: '打卡提醒', 3: '目标完成' }
  return map[type] || '其他'
}

onMounted(() => {
  loadChildId()
  loadReminders()
})
</script>

<style scoped>
.reminder-page { padding: 16px; background: #f5f7fa; min-height: 100vh; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-title { font-size: 20px; font-weight: bold; }
.btn-add { background: #409eff; color: #fff; border: none; border-radius: 20px; padding: 6px 16px; font-size: 14px; }
.add-form { background: #fff; border-radius: 12px; padding: 16px; margin-bottom: 16px; }
.form-input { width: 100%; padding: 10px; border: 1px solid #eee; border-radius: 8px; font-size: 14px; margin-bottom: 8px; box-sizing: border-box; }
.form-textarea { width: 100%; height: 80px; padding: 10px; border: 1px solid #eee; border-radius: 8px; font-size: 14px; margin-bottom: 8px; box-sizing: border-box; }
.type-row { display: flex; gap: 8px; margin-bottom: 8px; }
.type-tag { padding: 6px 12px; border-radius: 16px; background: #f0f0f0; font-size: 13px; }
.type-tag.active { background: #409eff; color: #fff; }
.btn-save { background: #67c23a; color: #fff; border: none; border-radius: 8px; padding: 10px; font-size: 15px; width: 100%; margin-top: 8px; }
.reminder-item { background: #fff; border-radius: 12px; padding: 16px; margin-bottom: 12px; }
.reminder-header { display: flex; justify-content: space-between; align-items: center; }
.reminder-title { font-size: 16px; font-weight: bold; }
.reminder-actions { display: flex; align-items: center; gap: 8px; }
.btn-delete { color: #f56c6c; font-size: 13px; }
.reminder-meta { display: flex; gap: 12px; margin-top: 8px; }
.reminder-type { font-size: 12px; color: #409eff; background: #ecf5ff; padding: 2px 8px; border-radius: 4px; }
.reminder-time { font-size: 12px; color: #999; }
.reminder-content { display: block; margin-top: 8px; font-size: 14px; color: #666; }
.empty-text { text-align: center; color: #999; padding: 40px 0; }
</style>
