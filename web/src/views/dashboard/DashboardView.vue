<template>
  <div class="dashboard">
    <h2>学习概览</h2>

    <el-empty v-if="childStore.children.length === 0" description="还没有添加小孩，请先添加">
      <el-button type="primary" @click="$router.push('/children')">添加小孩</el-button>
    </el-empty>

    <el-row :gutter="20" v-else>
      <el-col :span="8" v-for="child in childStore.children" :key="child.id">
        <el-card class="child-card" :class="{ active: child.id === childStore.activeChildId }" @click="childStore.switchChild(child.id)">
          <div class="card-header">
            <el-avatar :size="48" :src="child.avatar">
              {{ child.nickName.charAt(0) }}
            </el-avatar>
            <div class="card-info">
              <h3>{{ child.nickName }}</h3>
              <div class="card-tags">
                <el-tag v-if="child.stageName" size="small" type="primary">{{ child.stageName }}</el-tag>
                <el-tag v-if="child.gradeName" size="small">{{ child.gradeName }}</el-tag>
              </div>
            </div>
          </div>
          <el-divider />
          <div class="card-stats">
            <div class="stat-item">
              <span class="stat-value">0</span>
              <span class="stat-label">今日学习(分钟)</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">0</span>
              <span class="stat-label">连续打卡(天)</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">0%</span>
              <span class="stat-label">学习进度</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { useChildStore } from '../../stores/child'

const childStore = useChildStore()
</script>

<style scoped>
.dashboard h2 {
  margin-bottom: 20px;
}

.child-card {
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 20px;
}

.child-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.child-card.active {
  border-color: #409eff;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 16px;
}

.card-info h3 {
  margin: 0 0 4px;
  font-size: 16px;
}

.card-tags {
  display: flex;
  gap: 6px;
}

.card-stats {
  display: flex;
  justify-content: space-around;
}

.stat-item {
  text-align: center;
}

.stat-value {
  display: block;
  font-size: 24px;
  font-weight: bold;
  color: #409eff;
}

.stat-label {
  font-size: 12px;
  color: #999;
}
</style>
