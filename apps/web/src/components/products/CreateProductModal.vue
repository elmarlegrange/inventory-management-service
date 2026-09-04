<script setup lang="ts">
import { ref, reactive } from 'vue';
import ModalDialog from '../common/ModalDialog.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { productsApi } from '../../api/productsApi';
import { extractProblemDetails, formatErrorMessage } from '../../api/apiClient';
import type { ProductDto } from '../../types';

defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'created', product: ProductDto): void;
}>();

const form = reactive({
  code: '',
  name: ''
});

const isSubmitting = ref(false);
const conflictError = ref<string | null>(null);
const generalError = ref<string | null>(null);

function resetForm() {
  form.code = '';
  form.name = '';
  conflictError.value = null;
  generalError.value = null;
  isSubmitting.value = false;
}

function handleClose() {
  resetForm();
  emit('close');
}

async function handleSubmit() {
  conflictError.value = null;
  generalError.value = null;

  const trimmedCode = form.code.trim();
  const trimmedName = form.name.trim();

  if (!trimmedCode || !trimmedName) {
    generalError.value = 'Both product code and name are required.';
    return;
  }

  isSubmitting.value = true;

  try {
    const createdProduct = await productsApi.create({
      code: trimmedCode,
      name: trimmedName
    });

    resetForm();
    emit('created', createdProduct);
  } catch (err: unknown) {
    const problem = extractProblemDetails(err);
    if (problem && problem.status === 409) {
      conflictError.value = `A product with code "${trimmedCode}" already exists. Please choose a unique product code.`;
    } else {
      generalError.value = formatErrorMessage(err, 'Failed to create product. Please try again.');
    }
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<template>
  <ModalDialog :is-open="isOpen" title="Add New Product" @close="handleClose">
    <div v-if="conflictError" class="conflict-alert">
      <span class="icon">⚠️</span>
      <div>
        <strong>Conflict (409)</strong>
        <p>{{ conflictError }}</p>
      </div>
    </div>

    <ErrorAlert
      v-if="generalError"
      :message="generalError"
      @dismiss="generalError = null"
    />

    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="product-code">Product Code *</label>
        <input
          id="product-code"
          v-model="form.code"
          type="text"
          placeholder="e.g. PROD-001"
          class="form-control"
          :disabled="isSubmitting"
          required
        />
        <small class="help-text">Unique alphanumeric identifier</small>
      </div>

      <div class="form-group">
        <label for="product-name">Product Name / Description *</label>
        <input
          id="product-name"
          v-model="form.name"
          type="text"
          placeholder="e.g. Ergonomic Keyboard"
          class="form-control"
          :disabled="isSubmitting"
          required
        />
      </div>
    </form>

    <template #footer>
      <button class="btn btn-secondary" :disabled="isSubmitting" @click="handleClose">
        Cancel
      </button>
      <button class="btn btn-primary" :disabled="isSubmitting" @click="handleSubmit">
        <span v-if="isSubmitting">Saving...</span>
        <span v-else>Save Product</span>
      </button>
    </template>
  </ModalDialog>
</template>

<style scoped>
.conflict-alert {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  background: #fffbeb;
  border: 1px solid #fef3c7;
  color: #92400e;
  padding: 0.875rem;
  border-radius: 6px;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.conflict-alert strong {
  display: block;
  font-weight: 600;
  margin-bottom: 0.15rem;
}

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
  transition: border-color 0.15s;
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
