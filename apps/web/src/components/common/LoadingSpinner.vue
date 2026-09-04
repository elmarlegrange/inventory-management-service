<script setup lang="ts">
defineProps<{
  message?: string;
}>();
</script>

<template>
  <div class="spinner-container">
    <div class="spinner"></div>
    <p v-if="message" class="spinner-text">{{ message }}</p>
  </div>
</template>

<style scoped>
.spinner-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  gap: 0.75rem;
}

.spinner {
  width: 36px;
  height: 36px;
  border: 3px solid #e2e8f0;
  border-top-color: #2563eb;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner-text {
  color: #64748b;
  font-size: 0.875rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
