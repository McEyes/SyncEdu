<template>
  <div class="family-view">
    <h2>家庭设置</h2>

    <el-card v-if="family">
      <template #header>
        <div class="card-header">
          <span>{{ family.name }}</span>
          <el-tag type="success">邀请码: {{ family.inviteCode }}</el-tag>
        </div>
      </template>
      <p>家庭成员数: {{ family.memberCount }}</p>
      <p>小孩数: {{ family.childCount }}</p>

      <el-divider />
      <h4>邀请其他成员</h4>
      <p>将邀请码 <strong>{{ family.inviteCode }}</strong> 分享给其他家庭成员</p>
    </el-card>

    <el-card v-else>
      <el-empty description="还没有家庭">
        <el-button type="primary" @click="showCreateDialog = true">创建家庭</el-button>
        <el-button @click="showJoinDialog = true">加入家庭</el-button>
      </el-empty>
    </el-card>

    <!-- 创建家庭对话框 -->
    <el-dialog v-model="showCreateDialog" title="创建家庭" width="400px">
      <el-form @submit.prevent="handleCreate">
        <el-form-item label="家庭名称">
          <el-input v-model="familyName" placeholder="例如：张三家" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreateDialog = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 加入家庭对话框 -->
    <el-dialog v-model="showJoinDialog" title="加入家庭" width="400px">
      <el-form @submit.prevent="handleJoin">
        <el-form-item label="邀请码">
          <el-input v-model="inviteCode" placeholder="请输入8位邀请码" maxlength="8" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showJoinDialog = false">取消</el-button>
        <el-button type="primary" @click="handleJoin">加入</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { familyApi } from '../../api'

const family = ref<any>(null)
const showCreateDialog = ref(false)
const showJoinDialog = ref(false)
const familyName = ref('')
const inviteCode = ref('')

onMounted(async () => {
  await loadFamily()
})

async function loadFamily() {
  try {
    const res: any = await familyApi.get()
    family.value = res.data
  } catch {
    family.value = null
  }
}

async function handleCreate() {
  if (!familyName.value) {
    ElMessage.warning('请输入家庭名称')
    return
  }
  try {
    await familyApi.create({ name: familyName.value })
    ElMessage.success('家庭创建成功')
    showCreateDialog.value = false
    await loadFamily()
  } catch {
    // handled by interceptor
  }
}

async function handleJoin() {
  if (!inviteCode.value) {
    ElMessage.warning('请输入邀请码')
    return
  }
  try {
    await familyApi.join(inviteCode.value)
    ElMessage.success('加入成功')
    showJoinDialog.value = false
    await loadFamily()
  } catch {
    // handled by interceptor
  }
}
</script>

<style scoped>
.family-view h2 {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
</style>
