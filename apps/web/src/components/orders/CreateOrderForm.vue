<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';
import LoadingSpinner from '../common/LoadingSpinner.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { productsApi } from '../../api/productsApi';
import { warehousesApi } from '../../api/warehousesApi';
import { ordersApi } from '../../api/ordersApi';
import { extractProblemDetails, formatErrorMessage } from '../../api/apiClient';
import type { ProductDto, WarehouseDto, OrderDto, ProblemDetails } from '../../types';

const products = ref<ProductDto[]>([]);
const warehouses = ref<WarehouseDto[]>([]);
const isLoadingData = ref(true);
const isSubmitting = ref(false);

const form = reactive({
  productCode: '',
  sourceWarehouseCode: '',
  destinationWarehouseCode: '',
  quantity: 1
});

const availableStockAtSource = ref<number | null>(null);
const isLoadingStock = ref(false);

const successOrder = ref<OrderDto | null>(null);
const deficitError = ref<ProblemDetails | null>(null);
const generalError = ref<string | null>(null);

const availableDestinationWarehouses = computed(() => {
  return warehouses.value.filter((w) => w.code !== form.sourceWarehouseCode);
});

async function loadInitialData() {
  isLoadingData.value = true;
  generalError.value = null;

  try {
    const [prods, whs] = await Promise.all([
      productsApi.getAll(),
      warehousesApi.getAll()
    ]);

    products.value = prods;
    warehouses.value = whs;

    if (prods.length > 0) form.productCode = prods[0].code;
    if (whs.length > 0) form.sourceWarehouseCode = whs[0].code;
    if (whs.length > 1) {
      form.destinationWarehouseCode = whs[1].code;
    } else if (whs.length === 1) {
      form.destinationWarehouseCode = '';
    }
  } catch (err) {
    generalError.value = formatErrorMessage(err, 'Failed to load catalog or warehouse data.');
  } finally {
    isLoadingData.value = false;
  }
}

async function updateSourceStock() {
  if (!form.productCode || !form.sourceWarehouseCode) {
    availableStockAtSource.value = null;
    return;
  }

  isLoadingStock.value = true;

  try {
    const locations = await productsApi.getStock(form.productCode);
    const loc = locations.find(
      (l) => l.warehouseCode.toUpperCase() === form.sourceWarehouseCode.toUpperCase()
    );
    availableStockAtSource.value = loc ? loc.quantity : 0;
  } catch {
    availableStockAtSource.value = null;
  } finally {
    isLoadingStock.value = false;
  }
}

watch(
  () => [form.productCode, form.sourceWarehouseCode],
  () => {
    updateSourceStock();
    deficitError.value = null;
    successOrder.value = null;
  }
);

watch(
  () => form.sourceWarehouseCode,
  (newSource) => {
    if (form.destinationWarehouseCode === newSource) {
      const alt = warehouses.value.find((w) => w.code !== newSource);
      form.destinationWarehouseCode = alt ? alt.code : '';
    }
  }
);

async function handleSubmit() {
  deficitError.value = null;
  generalError.value = null;
  successOrder.value = null;

  const qty = Number(form.quantity);

  if (!form.productCode || !form.sourceWarehouseCode || !form.destinationWarehouseCode) {
    generalError.value = 'Please select a product, source warehouse, and destination warehouse.';
    return;
  }

  if (form.sourceWarehouseCode === form.destinationWarehouseCode) {
    generalError.value = 'Source and destination warehouses cannot be identical.';
    return;
  }

  if (isNaN(qty) || qty <= 0 || !Number.isInteger(qty)) {
    generalError.value = 'Quantity must be a positive integer greater than zero.';
    return;
  }

  isSubmitting.value = true;

  try {
    const order = await ordersApi.createOrder({
      productCode: form.productCode,
      sourceWarehouseCode: form.sourceWarehouseCode,
      destinationWarehouseCode: form.destinationWarehouseCode,
      quantity: qty
    });

    successOrder.value = order;
    await updateSourceStock();
  } catch (err: unknown) {
    const problem = extractProblemDetails(err);
    if (problem && problem.missingQuantity !== undefined) {
      deficitError.value = problem;
    } else {
      generalError.value = formatErrorMessage(err, 'Stock transfer failed.');
    }
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(() => {
  loadInitialData().then(() => updateSourceStock());
});
</script>

<template>
  <div class="orders-container">
    <div class="orders-header">
      <h2>Stock Transfer Orders</h2>
      <p class="subtitle">
        Transfer inventory between warehouse facilities.
      </p>
    </div>

    <LoadingSpinner v-if="isLoadingData" message="Loading transfer parameters..." />

    <div v-else-if="warehouses.length < 2" class="warning-card">
      <h3>⚠️ At least two warehouses required</h3>
      <p>
        Stock transfers require at least two warehouse facilities. Please add more warehouses first.
      </p>
    </div>

    <div v-else class="order-wrapper">
      <!-- Transfer Form -->
      <div class="card form-card">
        <h3>Create Transfer Order</h3>

        <!-- Deficit Shortfall Alert (RFC 7807) -->
        <div v-if="deficitError" class="deficit-alert">
          <div class="deficit-icon">⚠️</div>
          <div class="deficit-content">
            <h4>Insufficient Stock (Transfer Rejected)</h4>
            <p>{{ deficitError.detail }}</p>
            <div class="deficit-badges">
              <span class="badge badge-danger">Shortfall: {{ deficitError.missingQuantity }} units</span>
              <span class="badge badge-muted">Available: {{ deficitError.availableQuantity }} units</span>
              <span class="badge badge-muted">Requested: {{ deficitError.requiredQuantity }} units</span>
            </div>
          </div>
        </div>

        <!-- Success Alert -->
        <div v-if="successOrder" class="success-alert">
          <div class="success-icon">✓</div>
          <div>
            <h4>Transfer Executed Successfully!</h4>
            <p>
              Transferred <strong>{{ successOrder.quantity }} units</strong> of
              <code>{{ successOrder.productCode }}</code> from
              <code>{{ successOrder.sourceWarehouseCode }}</code> to
              <code>{{ successOrder.destinationWarehouseCode }}</code>.
            </p>
            <small class="order-id">Order ID: {{ successOrder.id }}</small>
          </div>
        </div>

        <ErrorAlert
          v-if="generalError"
          :message="generalError"
          @dismiss="generalError = null"
        />

        <form @submit.prevent="handleSubmit">
          <div class="form-group">
            <label for="order-product">Product to Move *</label>
            <select
              id="order-product"
              v-model="form.productCode"
              class="form-control"
              :disabled="isSubmitting"
            >
              <option
                v-for="p in products"
                :key="p.code"
                :value="p.code"
              >
                {{ p.code }} — {{ p.name }}
              </option>
            </select>
          </div>

          <div class="form-row">
            <div class="form-group flex-1">
              <label for="order-source">Source Warehouse (Origin) *</label>
              <select
                id="order-source"
                v-model="form.sourceWarehouseCode"
                class="form-control"
                :disabled="isSubmitting"
              >
                <option
                  v-for="w in warehouses"
                  :key="w.code"
                  :value="w.code"
                >
                  {{ w.code }} ({{ w.name }})
                </option>
              </select>

              <div class="stock-indicator">
                <span v-if="isLoadingStock" class="text-muted">Checking stock...</span>
                <span v-else-if="availableStockAtSource !== null">
                  Available on-hand:
                  <strong :class="availableStockAtSource > 0 ? 'text-success' : 'text-danger'">
                    {{ availableStockAtSource }} units
                  </strong>
                </span>
              </div>
            </div>

            <div class="form-group flex-1">
              <label for="order-dest">Destination Warehouse (Target) *</label>
              <select
                id="order-dest"
                v-model="form.destinationWarehouseCode"
                class="form-control"
                :disabled="isSubmitting"
              >
                <option
                  v-for="w in availableDestinationWarehouses"
                  :key="w.code"
                  :value="w.code"
                >
                  {{ w.code }} ({{ w.name }})
                </option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label for="order-quantity">Transfer Quantity (Positive Integer) *</label>
            <input
              id="order-quantity"
              v-model.number="form.quantity"
              type="number"
              min="1"
              step="1"
              class="form-control"
              :disabled="isSubmitting"
              required
            />
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            :disabled="isSubmitting || !availableDestinationWarehouses.length"
          >
            <span v-if="isSubmitting">Executing Transfer...</span>
            <span v-else>Execute Stock Transfer</span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.orders-container {
  margin-top: 1.5rem;
}

.orders-header {
  margin-bottom: 1.5rem;
}

.orders-header h2 {
  font-size: 1.35rem;
  font-weight: 700;
  color: #0f172a;
}

.subtitle {
  color: #64748b;
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.warning-card {
  background: #fffbeb;
  border: 1px solid #fef3c7;
  padding: 2rem;
  border-radius: 8px;
  text-align: center;
  color: #92400e;
}

.order-wrapper {
  max-width: 680px;
  margin: 0 auto;
}

.card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.card h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 1.25rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.flex-1 {
  flex: 1;
}

label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
  margin-bottom: 0.35rem;
}

.form-control {
  width: 100%;
  padding: 0.55rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.9rem;
  color: #0f172a;
  outline: none;
  background: #ffffff;
}

.form-control:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.1);
}

.stock-indicator {
  margin-top: 0.35rem;
  font-size: 0.75rem;
  color: #64748b;
}

.text-success {
  color: #16a34a;
}

.text-danger {
  color: #dc2626;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.65rem 1.25rem;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.15s;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
}

.btn-primary:hover:not(:disabled) {
  background: #1d4ed8;
}

.btn-block {
  width: 100%;
  margin-top: 0.5rem;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.deficit-alert {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
}

.deficit-icon {
  font-size: 1.25rem;
  line-height: 1;
}

.deficit-content h4 {
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.deficit-content p {
  font-size: 0.85rem;
  margin-bottom: 0.5rem;
}

.deficit-badges {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.badge-danger {
  background: #fee2e2;
  color: #b91c1c;
}

.badge-muted {
  background: #f1f5f9;
  color: #475569;
}

.success-alert {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #166534;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 1.25rem;
}

.success-icon {
  font-size: 1.25rem;
  font-weight: bold;
  color: #16a34a;
}

.success-alert h4 {
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.success-alert p {
  font-size: 0.85rem;
  line-height: 1.4;
}

.order-id {
  display: block;
  font-family: monospace;
  color: #64748b;
  margin-top: 0.35rem;
  font-size: 0.75rem;
}

code {
  background: rgba(0, 0, 0, 0.05);
  padding: 0.1rem 0.3rem;
  border-radius: 3px;
  font-family: monospace;
}
</style>
