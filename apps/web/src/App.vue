<script setup lang="ts">
import { ref } from 'vue';
import ProductList from './components/products/ProductList.vue';
import WarehouseList from './components/warehouses/WarehouseList.vue';

type ActiveTab = 'products' | 'warehouses';
const activeTab = ref<ActiveTab>('products');
</script>

<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="header-container">
        <div class="brand">
          <span class="logo">📦</span>
          <div>
            <h1>Inventory Management Service</h1>
            <span class="subtext">Multi-Warehouse Stock & Order Engine</span>
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
        </nav>

        <div class="header-status">
          <span class="badge badge-tech">.NET 10 & PostgreSQL</span>
          <span class="badge badge-connected">● API Online</span>
        </div>
      </div>
    </header>

    <main class="container">
      <ProductList v-show="activeTab === 'products'" />
      <WarehouseList v-show="activeTab === 'warehouses'" />
    </main>
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

.subtext {
  font-size: 0.8rem;
  color: #64748b;
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

.badge-tech {
  background: #f1f5f9;
  color: #334155;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.25rem 0.6rem;
  border-radius: 9999px;
}

.badge-connected {
  background: #dcfce7;
  color: #166534;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.25rem 0.6rem;
  border-radius: 9999px;
}
</style>
