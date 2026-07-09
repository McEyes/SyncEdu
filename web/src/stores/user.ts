import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api'

export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const nickName = ref(localStorage.getItem('nickName') || '')
  const phone = ref(localStorage.getItem('phone') || '')
  const userId = ref(Number(localStorage.getItem('userId')) || 0)

  const isLoggedIn = computed(() => !!token.value)

  async function login(phoneNum: string, password: string) {
    const res: any = await authApi.login({ phone: phoneNum, password })
    setLoginData(res.data)
    return res
  }

  async function register(phoneNum: string, password: string, name: string) {
    const res: any = await authApi.register({ phone: phoneNum, password, nickName: name })
    setLoginData(res.data)
    return res
  }

  function setLoginData(data: { token: string; nickName: string; phone: string; userId: number }) {
    token.value = data.token
    nickName.value = data.nickName
    phone.value = data.phone
    userId.value = data.userId
    localStorage.setItem('token', data.token)
    localStorage.setItem('nickName', data.nickName)
    localStorage.setItem('phone', data.phone)
    localStorage.setItem('userId', String(data.userId))
  }

  function logout() {
    token.value = ''
    nickName.value = ''
    phone.value = ''
    userId.value = 0
    localStorage.removeItem('token')
    localStorage.removeItem('nickName')
    localStorage.removeItem('phone')
    localStorage.removeItem('userId')
  }

  return { token, nickName, phone, userId, isLoggedIn, login, register, logout }
})
