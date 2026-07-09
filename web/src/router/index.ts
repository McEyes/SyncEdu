import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '../stores/user'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/login/LoginView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      component: () => import('../layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('../views/dashboard/DashboardView.vue')
        },
        {
          path: 'family',
          name: 'family',
          component: () => import('../views/family/FamilyView.vue')
        },
        {
          path: 'children',
          name: 'children',
          component: () => import('../views/child/ChildListView.vue')
        },
        {
          path: 'children/:id/profile',
          name: 'child-profile',
          component: () => import('../views/child/ChildProfileView.vue')
        },
        {
          path: 'education',
          name: 'education',
          component: () => import('../views/education/EducationView.vue')
        },
        {
          path: 'learning',
          name: 'learning',
          component: () => import('../views/learning/LearningPlanView.vue')
        },
        {
          path: 'checkin',
          name: 'checkin',
          component: () => import('../views/checkin/CheckInView.vue')
        },
        {
          path: 'reminders',
          name: 'reminders',
          component: () => import('../views/reminder/ReminderView.vue')
        },
        {
          path: 'achievements',
          name: 'achievements',
          component: () => import('../views/achievement/AchievementView.vue')
        },
        {
          path: 'education-sync',
          name: 'education-sync',
          component: () => import('../views/education/EducationSyncView.vue')
        }
      ]
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const userStore = useUserStore()
  if (to.meta.requiresAuth !== false && !userStore.isLoggedIn) {
    next({ name: 'login' })
  } else {
    next()
  }
})

export default router
