<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ProductList from './components/products/ProductList.vue';
import WarehouseList from './components/warehouses/WarehouseList.vue';
import CreateOrderForm from './components/orders/CreateOrderForm.vue';
import LoginModal from './components/auth/LoginModal.vue';
import { useAuth } from './composables/useAuth';

type ActiveTab = 'products' | 'warehouses' | 'orders';
const activeTab = ref<ActiveTab>('products');

const { isAuthenticated, currentUser, isAdmin, showLoginModal, logout, openLoginModal } = useAuth();

onMounted(() => {
  if (!isAuthenticated.value) {
    openLoginModal();
  }
});
</script>

<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="header-container">
        <div class="brand">
          <span class="logo">📦</span>
          <div>
            <h1>Inventory Management Service</h1>
          </div>
        </div>

        <nav class="nav-tabs">
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'products' }"
            @click="activeTab = 'products'"
          >
            📋 Products
          </button>
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'warehouses' }"
            @click="activeTab = 'warehouses'"
          >
            🏢 Warehouses
          </button>
          <button
            class="tab-btn"
            :class="{ active: activeTab === 'orders' }"
            @click="activeTab = 'orders'"
          >
            🔄 Stock Transfers
          </button>
        </nav>

        <div class="header-status">
          <div v-if="isAuthenticated && currentUser" class="user-profile">
            <span class="user-name">👤 {{ currentUser.username }}</span>
            <span :class="['role-badge', isAdmin ? 'role-admin' : 'role-user']">
              {{ currentUser.role }}
            </span>
            <button class="btn-auth-action" @click="openLoginModal" title="Switch Account">
              Switch
            </button>
            <button class="btn-auth-action btn-logout" @click="logout" title="Sign Out">
              Sign Out
            </button>
          </div>
          <div v-else class="auth-actions">
            <button class="btn-sign-in" @click="openLoginModal">
              🔑 Sign In
            </button>
          </div>
        </div>
      </div>
    </header>

    <main class="container">
      <ProductList v-show="activeTab === 'products'" />
      <WarehouseList v-show="activeTab === 'warehouses'" />
      <CreateOrderForm v-show="activeTab === 'orders'" />
    </main>

    <LoginModal
      :is-open="showLoginModal"
      @close="showLoginModal = false"
    />
  </div>
</template>

<style scoped>
.app-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-header {
  background: #ffffff;
  border-bottom: 1px solid #e2e8f0;
  padding: 0.85rem 0;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.header-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.logo {
  font-size: 2rem;
  line-height: 1;
}

.brand h1 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
}

.nav-tabs {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f1f5f9;
  padding: 0.25rem;
  border-radius: 8px;
}

.tab-btn {
  background: transparent;
  border: none;
  padding: 0.45rem 1rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: #64748b;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.tab-btn:hover {
  color: #0f172a;
}

.tab-btn.active {
  background: #ffffff;
  color: #2563eb;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.header-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  padding: 0.35rem 0.65rem;
  border-radius: 8px;
}

.user-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: #1e293b;
}

.role-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.15rem 0.45rem;
  border-radius: 9999px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.role-admin {
  background: #ede9fe;
  color: #6b21a8;
  border: 1px solid #ddd6fe;
}

.role-user {
  background: #e0f2fe;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.btn-auth-action {
  background: transparent;
  border: 1px solid #cbd5e1;
  border-radius: 4px;
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  color: #475569;
  cursor: pointer;
  transition: all 0.15s;
}

.btn-auth-action:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.btn-logout:hover {
  background: #fee2e2;
  border-color: #fca5a5;
  color: #991b1b;
}

.btn-sign-in {
  background: #2563eb;
  color: white;
  border: none;
  font-size: 0.85rem;
  font-weight: 600;
  padding: 0.4rem 0.85rem;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-sign-in:hover {
  background: #1d4ed8;
}
</style>
