<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import ModalDialog from '../common/ModalDialog.vue';
import LoadingSpinner from '../common/LoadingSpinner.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { productsApi } from '../../api/productsApi';
import { formatErrorMessage } from '../../api/apiClient';
import type { ProductDto, ProductStockLocationDto } from '../../types';

const props = defineProps<{
  isOpen: boolean;
  product: ProductDto | null;
}>();

defineEmits<{
  (e: 'close'): void;
}>();

const stockLocations = ref<ProductStockLocationDto[]>([]);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);

const totalStock = computed(() => {
  return stockLocations.value.reduce((sum, item) => sum + item.quantity, 0);
});

async function loadStock() {
  if (!props.product) return;

  isLoading.value = true;
  errorMessage.value = null;
  stockLocations.value = [];

  try {
    stockLocations.value = await productsApi.getStock(props.product.code);
  } catch (err) {
    errorMessage.value = formatErrorMessage(err, 'Failed to retrieve stock levels.');
  } finally {
    isLoading.value = false;
  }
}

watch(
  () => props.product,
  (newProduct) => {
    if (newProduct && props.isOpen) {
      loadStock();
    }
  }
);

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen && props.product) {
      loadStock();
    }
  }
);
</script>

<template>
  <ModalDialog
    :is-open="isOpen"
    :title="product ? `Stock for ${product.code} - ${product.name}` : 'Stock Levels'"
    @close="$emit('close')"
  >
    <LoadingSpinner v-if="isLoading" message="Fetching inventory distribution..." />

    <ErrorAlert
      v-else-if="errorMessage"
      :message="errorMessage"
      retry-text="Retry"
      @retry="loadStock"
      @dismiss="errorMessage = null"
    />

    <div v-else>
      <div class="summary-header">
        <span class="summary-label">Total On-Hand:</span>
        <span class="badge badge-stock">{{ totalStock }} units</span>
      </div>

      <div v-if="stockLocations.length === 0" class="empty-stock">
        <p>No stock records found for this product in any warehouse.</p>
      </div>

      <table v-else class="stock-table">
        <thead>
          <tr>
            <th>Warehouse Code</th>
            <th>Warehouse Name</th>
            <th style="text-align: right;">Quantity</th>
            <th>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="location in stockLocations" :key="location.warehouseCode">
            <td><code>{{ location.warehouseCode }}</code></td>
            <td>{{ location.warehouseName }}</td>
            <td style="text-align: right; font-weight: 600;">{{ location.quantity }}</td>
            <td style="color: #64748b; font-size: 0.8rem;">
              {{ new Date(location.updatedAt).toLocaleString() }}
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

.btn-secondary {
  background: #f1f5f9;
  color: #475569;
}

.btn-secondary:hover {
  background: #e2e8f0;
}
</style>
