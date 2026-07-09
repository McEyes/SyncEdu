<template>
  <view class="dashboard">
    <view class="header">
      <text class="greeting">你好，{{ nickName }}</text>
      <text class="title">学习概览</text>
    </view>

    <view class="child-list" v-if="children.length > 0">
      <view class="child-card" v-for="child in children" :key="child.id" @click="switchChild(child)">
        <view class="card-top">
          <view class="avatar">{{ child.nickName.charAt(0) }}</view>
          <view class="info">
            <text class="name">{{ child.nickName }}</text>
            <view class="tags">
              <text class="tag" v-if="child.stageName">{{ child.stageName }}</text>
              <text class="tag" v-if="child.gradeName">{{ child.gradeName }}</text>
            </view>
          </view>
        </view>
        <view class="card-stats">
          <view class="stat">
            <text class="stat-val">0</text>
            <text class="stat-label">今日学习</text>
          </view>
          <view class="stat">
            <text class="stat-val">0</text>
            <text class="stat-label">连续打卡</text>
          </view>
          <view class="stat">
            <text class="stat-val">0%</text>
            <text class="stat-label">进度</text>
          </view>
        </view>
      </view>
    </view>

    <view class="empty" v-else>
      <text>还没有添加小孩</text>
      <button class="btn-add" @click="showAddDialog = true">添加小孩</button>
    </view>

    <!-- 添加小孩弹窗 -->
    <view class="dialog-mask" v-if="showAddDialog" @click="showAddDialog = false">
      <view class="dialog" @click.stop>
        <text class="dialog-title">添加小孩</text>
        <input v-model="newChildName" placeholder="昵称" class="dialog-input" />
        <view class="gender-row">
          <text :class="['gender-btn', newChildGender === 1 && 'active']" @click="newChildGender = 1">男</text>
          <text :class="['gender-btn', newChildGender === 2 && 'active']" @click="newChildGender = 2">女</text>
        </view>
        <button class="btn-primary" @click="handleAddChild">保存</button>
      </view>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { childApi } from '../../api'

const nickName = ref(uni.getStorageSync('nickName') || '')
const children = ref<any[]>([])
const showAddDialog = ref(false)
const newChildName = ref('')
const newChildGender = ref(1)

onMounted(() => {
  const token = uni.getStorageSync('token')
  if (!token) {
    uni.redirectTo({ url: '/pages/login/index' })
    return
  }
  loadChildren()
})

async function loadChildren() {
  try {
    const res = await childApi.list()
    children.value = res.data || []
  } catch (e) {
    // handled
  }
}

function switchChild(child: any) {
  uni.setStorageSync('activeChildId', child.id)
  uni.navigateTo({ url: `/pages/child/index?id=${child.id}` })
}

async function handleAddChild() {
  if (!newChildName.value) {
    uni.showToast({ title: '请输入昵称', icon: 'none' })
    return
  }
  try {
    await childApi.create({ nickName: newChildName.value, gender: newChildGender.value })
    uni.showToast({ title: '添加成功' })
    showAddDialog.value = false
    newChildName.value = ''
    await loadChildren()
  } catch (e) {
    // handled
  }
}
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  background: #f5f7fa;
  padding: 20px;
}

.header {
  margin-bottom: 20px;
}

.greeting {
  font-size: 14px;
  color: #999;
}

.title {
  display: block;
  font-size: 22px;
  font-weight: bold;
  margin-top: 4px;
}

.child-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
}

.card-top {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #409eff;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: bold;
}

.name {
  font-size: 16px;
  font-weight: bold;
}

.tags {
  display: flex;
  gap: 6px;
  margin-top: 4px;
}

.tag {
  font-size: 11px;
  background: #ecf5ff;
  color: #409eff;
  padding: 2px 8px;
  border-radius: 4px;
}

.card-stats {
  display: flex;
  justify-content: space-around;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #eee;
}

.stat {
  text-align: center;
}

.stat-val {
  display: block;
  font-size: 20px;
  font-weight: bold;
  color: #409eff;
}

.stat-label {
  font-size: 11px;
  color: #999;
}

.empty {
  text-align: center;
  padding: 60px 0;
  color: #999;
}

.btn-add {
  margin-top: 16px;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px 30px;
  font-size: 14px;
}

.dialog-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
}

.dialog {
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  width: 300px;
}

.dialog-title {
  display: block;
  font-size: 18px;
  font-weight: bold;
  margin-bottom: 16px;
}

.dialog-input {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 10px;
  margin-bottom: 12px;
  font-size: 14px;
}

.gender-row {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
}

.gender-btn {
  flex: 1;
  text-align: center;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 14px;
}

.gender-btn.active {
  border-color: #409eff;
  color: #409eff;
  background: #ecf5ff;
}

.btn-primary {
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px;
  font-size: 14px;
}
</style>
