<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import ModalDialog from '../common/ModalDialog.vue';
import LoadingSpinner from '../common/LoadingSpinner.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { warehousesApi } from '../../api/warehousesApi';
import { formatErrorMessage } from '../../api/apiClient';
import { useAuth } from '../../composables/useAuth';
import type { WarehouseDto, WarehouseStockItemDto } from '../../types';

const { isAdmin } = useAuth();

const props = defineProps<{
  isOpen: boolean;
  warehouse: WarehouseDto | null;
}>();

defineEmits<{
  (e: 'close'): void;
  (e: 'requestAddStock'): void;
}>();

const stockItems = ref<WarehouseStockItemDto[]>([]);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);

const totalItems = computed(() => {
  return stockItems.value.reduce((sum, item) => sum + item.quantity, 0);
});

async function loadStock() {
  if (!props.warehouse) return;

  isLoading.value = true;
  errorMessage.value = null;
  stockItems.value = [];

  try {
    stockItems.value = await warehousesApi.getStock(props.warehouse.code);
  } catch (err) {
    errorMessage.value = formatErrorMessage(err, 'Failed to retrieve warehouse stock.');
  } finally {
    isLoading.value = false;
  }
}

watch(
  () => props.warehouse,
  (newWarehouse) => {
    if (newWarehouse && props.isOpen) {
      loadStock();
    }
  }
);

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen && props.warehouse) {
      loadStock();
    }
  }
);
</script>

<template>
  <ModalDialog
    :is-open="isOpen"
    :title="warehouse ? `Inventory at ${warehouse.name} (${warehouse.code})` : 'Warehouse Inventory'"
    @close="$emit('close')"
  >
    <LoadingSpinner v-if="isLoading" message="Fetching warehouse stock records..." />

    <ErrorAlert
      v-else-if="errorMessage"
      :message="errorMessage"
      retry-text="Retry"
      @retry="loadStock"
      @dismiss="errorMessage = null"
    />

    <div v-else>
      <div class="summary-header">
        <div>
          <span class="summary-label">Total Inventory Stored:</span>
          <span class="badge badge-stock">{{ totalItems }} units</span>
        </div>
        <button v-if="isAdmin" class="btn btn-primary btn-sm" @click="$emit('requestAddStock')">
          + Add Stock
        </button>
      </div>

      <div v-if="stockItems.length === 0" class="empty-stock">
        <p>No products currently stored in this warehouse facility.</p>
      </div>

      <table v-else class="stock-table">
        <thead>
          <tr>
            <th>Product Code</th>
            <th>Product Name</th>
            <th style="text-align: right;">Quantity</th>
            <th>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in stockItems" :key="item.productCode">
            <td><code>{{ item.productCode }}</code></td>
            <td>{{ item.productName }}</td>
            <td style="text-align: right; font-weight: 600;">{{ item.quantity }}</td>
            <td style="color: #64748b; font-size: 0.8rem;">
              {{ new Date(item.updatedAt).toLocaleString() }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <template #footer>
      <button class="btn btn-secondary" @click="$emit('close')">Close</button>
    </template>
  </ModalDialog>
</template>

<style scoped>
.summary-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #e2e8f0;
}

.summary-label {
  font-weight: 600;
  color: #334155;
  margin-right: 0.5rem;
}

.badge-stock {
  background: #dbeafe;
  color: #1e40af;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-weight: 700;
}

.empty-stock {
  text-align: center;
  padding: 2rem;
  color: #64748b;
  font-size: 0.875rem;
}

.stock-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.stock-table th,
.stock-table td {
  padding: 0.65rem 0.75rem;
  border-bottom: 1px solid #f1f5f9;
  text-align: left;
}

.stock-table th {
  background: #f8fafc;
  font-weight: 600;
  color: #475569;
}

code {
  background: #f1f5f9;
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
  font-family: monospace;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid transparent;
}

.btn-sm {
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
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
</style>
