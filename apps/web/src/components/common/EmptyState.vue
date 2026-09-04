<script setup lang="ts">
defineProps<{
  title: string;
  description?: string;
  actionText?: string;
}>();

defineEmits<{
  (e: 'action'): void;
}>();
</script>

<template>
  <div class="empty-state">
    <div class="icon">📦</div>
    <h3>{{ title }}</h3>
    <p v-if="description">{{ description }}</p>
    <button v-if="actionText" class="btn btn-primary" @click="$emit('action')">
      {{ actionText }}
    </button>
  </div>
</template>

<style scoped>
.empty-state {
  text-align: center;
  padding: 3.5rem 1.5rem;
  background: #ffffff;
  border: 1px dashed #cbd5e1;
  border-radius: 8px;
  margin: 1rem 0;
}

.icon {
  font-size: 2.5rem;
  margin-bottom: 0.75rem;
}

h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

p {
  color: #64748b;
  font-size: 0.875rem;
  margin-bottom: 1.25rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid transparent;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
}

.btn-primary:hover {
  background: #1d4ed8;
}
</style>
