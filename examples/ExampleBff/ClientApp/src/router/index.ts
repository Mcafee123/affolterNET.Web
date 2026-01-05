import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue')
    },
    {
      path: '/public',
      name: 'public',
      component: () => import('@/views/PublicView.vue')
    },
    {
      path: '/protected',
      name: 'protected',
      component: () => import('@/views/ProtectedView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/views/AdminView.vue'),
      meta: { requiresAuth: true, permission: 'admin-resource:view' }
    }
  ]
})

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore()

  // Fetch user if not loaded
  if (!authStore.user) {
    await authStore.fetchUser()
  }

  // Check authentication requirement
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    authStore.login(to.fullPath)
    return
  }

  // Check permission requirement
  if (to.meta.permission) {
    const permissionStr = to.meta.permission as string
    const [resource, action] = permissionStr.split(':')
    if (!authStore.hasPermission(resource, action)) {
      // Redirect to home or show access denied
      next({ name: 'home' })
      return
    }
  }

  next()
})

export default router
