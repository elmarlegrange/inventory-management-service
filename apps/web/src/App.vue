<script setup lang="ts">
import { ref } from 'vue';

const appVersion = ref('v1.0.0');
</script>

<template>
  <header>
    <div class="header-content">
      <div class="brand">
        <span>📦 Inventory Management</span>
        <span class="badge badge-primary">Vue 3 + .NET 10</span>
      </div>
      <div>
        <span class="badge badge-success">API Connected ({{ appVersion }})</span>
      </div>
    </div>
  </header>

  <main class="container">
    <div class="card">
      <h2>Welcome to Inventory Management Service</h2>
      <p style="color: var(--text-secondary); margin-top: 0.5rem;">
        Frontend client initialized with typed RFC 7807 problem details error handling.
      </p>
    </div>
  </main>
</template>

<style scoped>
h2 {
  font-size: 1.5rem;
  font-weight: 600;
}
</style>
