<template>
  <div class="child-list">
    <div class="page-header">
      <h2>小孩管理</h2>
      <el-button type="primary" @click="showAddDialog = true">添加小孩</el-button>
    </div>

    <el-row :gutter="20">
      <el-col :span="8" v-for="child in childStore.children" :key="child.id">
        <el-card>
          <div class="child-card-header">
            <el-avatar :size="56" :src="child.avatar">
              {{ child.nickName.charAt(0) }}
            </el-avatar>
            <div>
              <h3>{{ child.nickName }}</h3>
              <p v-if="child.stageName">{{ child.stageName }} {{ child.gradeName }}</p>
              <p v-else class="no-profile">未设置学习档案</p>
            </div>
          </div>
          <el-divider />
          <div class="card-actions">
            <el-button size="small" @click="$router.push(`/children/${child.id}/profile`)">
              设置档案
            </el-button>
            <el-button size="small" @click="openEditDialog(child)">编辑</el-button>
            <el-popconfirm title="确定删除吗？" @confirm="handleDelete(child.id)">
              <template #reference>
                <el-button size="small" type="danger">删除</el-button>
              </template>
            </el-popconfirm>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 添加/编辑对话框 -->
    <el-dialog v-model="showAddDialog" :title="editingChild ? '编辑小孩' : '添加小孩'" width="450px">
      <el-form :model="childForm" label-width="80px">
        <el-form-item label="昵称" required>
          <el-input v-model="childForm.nickName" placeholder="小孩昵称" />
        </el-form-item>
        <el-form-item label="性别">
          <el-radio-group v-model="childForm.gender">
            <el-radio :value="1">男</el-radio>
            <el-radio :value="2">女</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="生日">
          <el-date-picker v-model="childForm.birthday" type="date" placeholder="选择生日" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { childApi } from '../../api'
import { useChildStore, type ChildInfo } from '../../stores/child'

const childStore = useChildStore()
const showAddDialog = ref(false)
const editingChild = ref<ChildInfo | null>(null)

const childForm = reactive({
  nickName: '',
  gender: 1,
  birthday: ''
})

onMounted(() => {
  // children already loaded by layout
})

function openEditDialog(child: ChildInfo) {
  editingChild.value = child
  childForm.nickName = child.nickName
  childForm.gender = child.gender
  childForm.birthday = child.birthday || ''
  showAddDialog.value = true
}

async function handleSave() {
  if (!childForm.nickName) {
    ElMessage.warning('请输入昵称')
    return
  }

  try {
    if (editingChild.value) {
      await childApi.update({
        id: editingChild.value.id,
        nickName: childForm.nickName,
        gender: childForm.gender,
        birthday: childForm.birthday || undefined
      })
      ElMessage.success('更新成功')
    } else {
      const res: any = await childApi.create({
        nickName: childForm.nickName,
        gender: childForm.gender,
        birthday: childForm.birthday || undefined
      })
      childStore.addChild(res.data)
      ElMessage.success('添加成功')
    }
    showAddDialog.value = false
    editingChild.value = null
    childForm.nickName = ''
    childForm.gender = 1
    childForm.birthday = ''
    await childStore.loadChildren()
  } catch {
    // handled
  }
}

async function handleDelete(childId: number) {
  try {
    await childApi.delete(childId)
    childStore.removeChild(childId)
    ElMessage.success('删除成功')
  } catch {
    // handled
  }
}
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.child-card-header {
  display: flex;
  align-items: center;
  gap: 16px;
}

.child-card-header h3 {
  margin: 0 0 4px;
}

.no-profile {
  color: #f56c6c;
  font-size: 12px;
}

.card-actions {
  display: flex;
  gap: 8px;
}
</style>
