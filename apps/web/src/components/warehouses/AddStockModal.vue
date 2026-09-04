<script setup lang="ts">
import { ref, reactive, watch, onMounted } from 'vue';
import ModalDialog from '../common/ModalDialog.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { warehousesApi } from '../../api/warehousesApi';
import { productsApi } from '../../api/productsApi';
import { formatErrorMessage } from '../../api/apiClient';
import type { WarehouseDto, ProductDto } from '../../types';

const props = defineProps<{
  isOpen: boolean;
  warehouse: WarehouseDto | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'stockAdded'): void;
}>();

const availableProducts = ref<ProductDto[]>([]);
const isSubmitting = ref(false);
const errorMessage = ref<string | null>(null);

const form = reactive({
  productCode: '',
  quantity: 1
});

async function loadProducts() {
  try {
    availableProducts.value = await productsApi.getAll();
    if (availableProducts.value.length > 0 && !form.productCode) {
      form.productCode = availableProducts.value[0].code;
    }
  } catch {
    // Non-blocking: fallback to text input if products fail to load
  }
}

function resetForm() {
  form.quantity = 1;
  errorMessage.value = null;
  isSubmitting.value = false;
  if (availableProducts.value.length > 0) {
    form.productCode = availableProducts.value[0].code;
  } else {
    form.productCode = '';
  }
}

function handleClose() {
  resetForm();
  emit('close');
}

async function handleSubmit() {
  if (!props.warehouse) return;
  errorMessage.value = null;

  const trimmedProductCode = form.productCode.trim();
  const qty = Number(form.quantity);

  if (!trimmedProductCode) {
    errorMessage.value = 'Product code is required.';
    return;
  }

  if (isNaN(qty) || qty <= 0 || !Number.isInteger(qty)) {
    errorMessage.value = 'Quantity must be a positive integer greater than zero.';
    return;
  }

  isSubmitting.value = true;

  try {
    await warehousesApi.addStock(props.warehouse.code, {
      productCode: trimmedProductCode,
      quantity: qty
    });

    resetForm();
    emit('stockAdded');
  } catch (err: unknown) {
    errorMessage.value = formatErrorMessage(err, 'Failed to add stock.');
  } finally {
    isSubmitting.value = false;
  }
}

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      loadProducts();
    }
  }
);

onMounted(() => {
  if (props.isOpen) {
    loadProducts();
  }
});
</script>

<template>
  <ModalDialog
    :is-open="isOpen"
    :title="warehouse ? `Add Stock to ${warehouse.name} (${warehouse.code})` : 'Add Stock'"
    @close="handleClose"
  >
    <ErrorAlert
      v-if="errorMessage"
      :message="errorMessage"
      @dismiss="errorMessage = null"
    />

    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="stock-product">Select Product *</label>
        <select
          v-if="availableProducts.length > 0"
          id="stock-product"
          v-model="form.productCode"
          class="form-control"
          :disabled="isSubmitting"
        >
          <option
            v-for="p in availableProducts"
            :key="p.code"
            :value="p.code"
          >
            {{ p.code }} — {{ p.name }}
          </option>
        </select>
        <input
          v-else
          id="stock-product"
          v-model="form.productCode"
          type="text"
          placeholder="e.g. PROD-001"
          class="form-control"
          :disabled="isSubmitting"
          required
        />
      </div>

      <div class="form-group">
        <label for="stock-quantity">Quantity to Add (Positive Integer) *</label>
        <input
          id="stock-quantity"
          v-model.number="form.quantity"
          type="number"
          min="1"
          step="1"
          class="form-control"
          :disabled="isSubmitting"
          required
        />
        <small class="help-text">Will increment existing stock or initialize if new</small>
      </div>
    </form>

    <template #footer>
      <button class="btn btn-secondary" :disabled="isSubmitting" @click="handleClose">
        Cancel
      </button>
      <button class="btn btn-primary" :disabled="isSubmitting" @click="handleSubmit">
        <span v-if="isSubmitting">Updating...</span>
        <span v-else>+ Add Stock</span>
      </button>
    </template>
  </ModalDialog>
</template>

<style scoped>
.form-group {
  margin-bottom: 1rem;
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

.help-text {
  display: block;
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 0.25rem;
}

.btn {
  display: inline-flex;
  align-items: center;
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

.btn-secondary:hover:not(:disabled) {
  background: #e2e8f0;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
}

.btn-primary:hover:not(:disabled) {
  background: #1d4ed8;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
