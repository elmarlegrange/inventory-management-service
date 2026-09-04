<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import LoadingSpinner from '../common/LoadingSpinner.vue';
import EmptyState from '../common/EmptyState.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import CreateWarehouseModal from './CreateWarehouseModal.vue';
import AddStockModal from './AddStockModal.vue';
import WarehouseStockModal from './WarehouseStockModal.vue';
import { warehousesApi } from '../../api/warehousesApi';
import { formatErrorMessage } from '../../api/apiClient';
import type { WarehouseDto } from '../../types';

const warehouses = ref<WarehouseDto[]>([]);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);
const searchQuery = ref('');

const isCreateModalOpen = ref(false);
const isAddStockModalOpen = ref(false);
const isStockModalOpen = ref(false);
const selectedWarehouse = ref<WarehouseDto | null>(null);

const filteredWarehouses = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return warehouses.value;
  return warehouses.value.filter(
    (w) => w.code.toLowerCase().includes(q) || w.name.toLowerCase().includes(q)
  );
});

async function loadWarehouses() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    warehouses.value = await warehousesApi.getAll();
  } catch (err) {
    errorMessage.value = formatErrorMessage(err, 'Failed to load warehouses.');
  } finally {
    isLoading.value = false;
  }
}

function handleWarehouseCreated(newWarehouse: WarehouseDto) {
  warehouses.value.unshift(newWarehouse);
  isCreateModalOpen.value = false;
}

function openAddStockModal(warehouse: WarehouseDto) {
  selectedWarehouse.value = warehouse;
  isAddStockModalOpen.value = true;
}

function openStockModal(warehouse: WarehouseDto) {
  selectedWarehouse.value = warehouse;
  isStockModalOpen.value = true;
}

function handleStockAdded() {
  isAddStockModalOpen.value = false;
}

onMounted(() => {
  loadWarehouses();
});
</script>

<template>
  <div class="warehouse-list-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>Warehouse Facilities</h2>
        <span class="count-badge">{{ filteredWarehouses.length }} locations</span>
      </div>
      <div class="toolbar-right">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter by code or name..."
          class="search-input"
        />
        <button class="btn btn-primary" @click="isCreateModalOpen = true">
          <span>+</span> Add Warehouse
        </button>
      </div>
    </div>

    <ErrorAlert
      v-if="errorMessage"
      :message="errorMessage"
      retry-text="Retry"
      @retry="loadWarehouses"
      @dismiss="errorMessage = null"
    />

    <LoadingSpinner v-if="isLoading" message="Loading warehouse facilities..." />

    <EmptyState
      v-else-if="warehouses.length === 0"
      title="No Warehouses Registered"
      description="No distribution facilities exist yet. Register your first warehouse location."
      action-text="Add Warehouse"
      @action="isCreateModalOpen = true"
    />

    <div v-else class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Warehouse Code</th>
            <th>Facility Name</th>
            <th>Registered At</th>
            <th style="text-align: right;">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="warehouse in filteredWarehouses" :key="warehouse.code">
            <td>
              <span class="code-badge">{{ warehouse.code }}</span>
            </td>
            <td class="warehouse-name">{{ warehouse.name }}</td>
            <td class="warehouse-date">
              {{ new Date(warehouse.createdAt).toLocaleDateString() }}
            </td>
            <td style="text-align: right;">
              <div class="action-buttons">
                <button class="btn btn-outline" @click="openStockModal(warehouse)">
                  🔍 View Inventory
                </button>
                <button class="btn btn-secondary" @click="openAddStockModal(warehouse)">
                  + Add Stock
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="filteredWarehouses.length === 0 && searchQuery" class="no-results">
        No warehouses match "{{ searchQuery }}"
      </div>
    </div>

    <!-- Modals -->
    <CreateWarehouseModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="handleWarehouseCreated"
    />

    <AddStockModal
      :is-open="isAddStockModalOpen"
      :warehouse="selectedWarehouse"
      @close="isAddStockModalOpen = false"
      @stock-added="handleStockAdded"
    />

    <WarehouseStockModal
      :is-open="isStockModalOpen"
      :warehouse="selectedWarehouse"
      @close="isStockModalOpen = false"
      @request-add-stock="() => { isStockModalOpen = false; isAddStockModalOpen = true; }"
    />
  </div>
</template>

<style scoped>
.warehouse-list-container {
  margin-top: 1.5rem;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.25rem;
  gap: 1rem;
  flex-wrap: wrap;
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.toolbar-left h2 {
  font-size: 1.35rem;
  font-weight: 700;
  color: #0f172a;
}

.count-badge {
  background: #e2e8f0;
  color: #475569;
  padding: 0.2rem 0.55rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.toolbar-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.search-input {
  padding: 0.5rem 0.85rem;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.875rem;
  width: 240px;
  outline: none;
}

.search-input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.1);
}

.table-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.data-table th,
.data-table td {
  padding: 0.85rem 1rem;
  border-bottom: 1px solid #f1f5f9;
  text-align: left;
}

.data-table th {
  background: #f8fafc;
  font-weight: 600;
  color: #475569;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.code-badge {
  background: #f1f5f9;
  color: #1e293b;
  font-family: monospace;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.warehouse-name {
  font-weight: 500;
  color: #1e293b;
}

.warehouse-date {
  color: #64748b;
  font-size: 0.85rem;
}

.action-buttons {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.15s;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
}

.btn-primary:hover {
  background: #1d4ed8;
}

.btn-secondary {
  background: #f1f5f9;
  color: #475569;
}

.btn-secondary:hover {
  background: #e2e8f0;
}

.btn-outline {
  background: transparent;
  border-color: #cbd5e1;
  color: #334155;
}

.btn-outline:hover {
  background: #f8fafc;
  border-color: #94a3b8;
}

.no-results {
  text-align: center;
  padding: 2rem;
  color: #64748b;
  font-size: 0.875rem;
}
</style>
