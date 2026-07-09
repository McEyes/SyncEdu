import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { childApi } from '../api'

export interface ChildInfo {
  id: number
  nickName: string
  avatar?: string
  birthday?: string
  gender: number
  familyId: number
  stageName?: string
  gradeName?: string
  schoolName?: string
  createdAt: string
}

export const useChildStore = defineStore('child', () => {
  const children = ref<ChildInfo[]>([])
  const activeChildId = ref<number>(Number(localStorage.getItem('activeChildId')) || 0)

  const activeChild = computed(() =>
    children.value.find(c => c.id === activeChildId.value) || children.value[0] || null
  )

  async function loadChildren() {
    const res: any = await childApi.list()
    children.value = res.data || []
    // 如果没有活跃小孩或当前活跃小孩不在列表中，自动选第一个
    if (children.value.length > 0) {
      if (!children.value.find(c => c.id === activeChildId.value)) {
        activeChildId.value = children.value[0].id
        localStorage.setItem('activeChildId', String(activeChildId.value))
      }
    }
  }

  function switchChild(childId: number) {
    activeChildId.value = childId
    localStorage.setItem('activeChildId', String(childId))
  }

  function addChild(child: ChildInfo) {
    children.value.push(child)
    if (children.value.length === 1) {
      switchChild(child.id)
    }
  }

  function removeChild(childId: number) {
    children.value = children.value.filter(c => c.id !== childId)
    if (activeChildId.value === childId) {
      activeChildId.value = children.value[0]?.id || 0
      localStorage.setItem('activeChildId', String(activeChildId.value))
    }
  }

  return { children, activeChildId, activeChild, loadChildren, switchChild, addChild, removeChild }
})
