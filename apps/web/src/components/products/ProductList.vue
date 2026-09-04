<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import LoadingSpinner from '../common/LoadingSpinner.vue';
import EmptyState from '../common/EmptyState.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import CreateProductModal from './CreateProductModal.vue';
import ProductStockModal from './ProductStockModal.vue';
import { productsApi } from '../../api/productsApi';
import { formatErrorMessage } from '../../api/apiClient';
import type { ProductDto } from '../../types';

const products = ref<ProductDto[]>([]);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);
const searchQuery = ref('');

const isCreateModalOpen = ref(false);
const isStockModalOpen = ref(false);
const selectedProduct = ref<ProductDto | null>(null);

const filteredProducts = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return products.value;
  return products.value.filter(
    (p) => p.code.toLowerCase().includes(q) || p.name.toLowerCase().includes(q)
  );
});

async function loadProducts() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    products.value = await productsApi.getAll();
  } catch (err) {
    errorMessage.value = formatErrorMessage(err, 'Failed to load products.');
  } finally {
    isLoading.value = false;
  }
}

function handleProductCreated(newProduct: ProductDto) {
  products.value.unshift(newProduct);
  isCreateModalOpen.value = false;
}

function openStockModal(product: ProductDto) {
  selectedProduct.value = product;
  isStockModalOpen.value = true;
}

onMounted(() => {
  loadProducts();
});
</script>

<template>
  <div class="product-list-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>Products Catalog</h2>
        <span class="count-badge">{{ filteredProducts.length }} items</span>
      </div>
      <div class="toolbar-right">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter by code or name..."
          class="search-input"
        />
        <button class="btn btn-primary" @click="isCreateModalOpen = true">
          <span>+</span> Add Product
        </button>
      </div>
    </div>

    <ErrorAlert
      v-if="errorMessage"
      :message="errorMessage"
      retry-text="Retry"
      @retry="loadProducts"
      @dismiss="errorMessage = null"
    />

    <LoadingSpinner v-if="isLoading" message="Loading product catalog..." />

    <EmptyState
      v-else-if="products.length === 0"
      title="No Products Found"
      description="Your inventory catalog is currently empty. Get started by registering your first product."
      action-text="Add Product"
      @action="isCreateModalOpen = true"
    />

    <div v-else class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Product Code</th>
            <th>Name / Description</th>
            <th>Created At</th>
            <th style="text-align: right;">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in filteredProducts" :key="product.code">
            <td>
              <span class="code-badge">{{ product.code }}</span>
            </td>
            <td class="product-name">{{ product.name }}</td>
            <td class="product-date">
              {{ new Date(product.createdAt).toLocaleDateString() }}
            </td>
            <td style="text-align: right;">
              <button class="btn btn-outline" @click="openStockModal(product)">
                🔍 View Stock
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="filteredProducts.length === 0 && searchQuery" class="no-results">
        No products match "{{ searchQuery }}"
      </div>
    </div>

    <!-- Modals -->
    <CreateProductModal
      :is-open="isCreateModalOpen"
      @close="isCreateModalOpen = false"
      @created="handleProductCreated"
    />

    <ProductStockModal
      :is-open="isStockModalOpen"
      :product="selectedProduct"
      @close="isStockModalOpen = false"
    />
  </div>
</template>

<style scoped>
.product-list-container {
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

.product-name {
  font-weight: 500;
  color: #1e293b;
}

.product-date {
  color: #64748b;
  font-size: 0.85rem;
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
