<template>
  <view class="login-page">
    <view class="login-header">
      <text class="title">阳光同步学</text>
      <text class="subtitle">从幼儿园到大学的同步学习平台</text>
    </view>

    <view class="login-form">
      <view class="form-item">
        <input v-model="phone" type="number" placeholder="手机号" maxlength="11" class="input" />
      </view>
      <view class="form-item">
        <input v-model="password" type="password" placeholder="密码" class="input" />
      </view>

      <button class="btn-primary" @click="handleLogin" :loading="loading">登录</button>

      <view class="form-item" v-if="isRegister">
        <input v-model="nickName" placeholder="昵称" class="input" />
      </view>
      <button v-if="isRegister" class="btn-primary" @click="handleRegister" :loading="loading">注册</button>

      <view class="switch-action" @click="isRegister = !isRegister">
        <text>{{ isRegister ? '已有账号？去登录' : '没有账号？去注册' }}</text>
      </view>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { authApi } from '../../api'

const phone = ref('')
const password = ref('')
const nickName = ref('')
const isRegister = ref(false)
const loading = ref(false)

async function handleLogin() {
  if (!phone.value || !password.value) {
    uni.showToast({ title: '请填写完整', icon: 'none' })
    return
  }
  loading.value = true
  try {
    const res = await authApi.login({ phone: phone.value, password: password.value })
    uni.setStorageSync('token', res.data.token)
    uni.setStorageSync('nickName', res.data.nickName)
    uni.setStorageSync('userId', res.data.userId)
    uni.showToast({ title: '登录成功' })
    setTimeout(() => {
      uni.switchTab({ url: '/pages/dashboard/index' })
    }, 500)
  } catch (e) {
    // handled
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  if (!phone.value || !password.value || !nickName.value) {
    uni.showToast({ title: '请填写完整', icon: 'none' })
    return
  }
  loading.value = true
  try {
    const res = await authApi.register({ phone: phone.value, password: password.value, nickName: nickName.value })
    uni.setStorageSync('token', res.data.token)
    uni.setStorageSync('nickName', res.data.nickName)
    uni.setStorageSync('userId', res.data.userId)
    uni.showToast({ title: '注册成功' })
    setTimeout(() => {
      uni.switchTab({ url: '/pages/dashboard/index' })
    }, 500)
  } catch (e) {
    // handled
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 60px 30px;
}

.login-header {
  text-align: center;
  margin-bottom: 40px;
}

.title {
  display: block;
  font-size: 28px;
  font-weight: bold;
  color: #fff;
}

.subtitle {
  display: block;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.7);
  margin-top: 8px;
}

.login-form {
  background: #fff;
  border-radius: 16px;
  padding: 30px;
}

.form-item {
  margin-bottom: 16px;
}

.input {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px 16px;
  font-size: 16px;
}

.btn-primary {
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 14px;
  font-size: 16px;
  margin-bottom: 16px;
}

.switch-action {
  text-align: center;
  color: #409eff;
  font-size: 14px;
}
</style>
