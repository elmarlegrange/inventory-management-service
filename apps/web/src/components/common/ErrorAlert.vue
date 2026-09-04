<script setup lang="ts">
defineProps<{
  title?: string;
  message: string;
  retryText?: string;
}>();

defineEmits<{
  (e: 'retry'): void;
  (e: 'dismiss'): void;
}>();
</script>

<template>
  <div class="alert-banner">
    <div class="alert-icon">⚠️</div>
    <div class="alert-content">
      <h4 v-if="title">{{ title }}</h4>
      <p>{{ message }}</p>
    </div>
    <div class="alert-actions">
      <button v-if="retryText" class="btn-retry" @click="$emit('retry')">
        {{ retryText }}
      </button>
      <button class="btn-dismiss" @click="$emit('dismiss')">✕</button>
    </div>
  </div>
</template>

<style scoped>
.alert-banner {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 6px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
  margin-bottom: 1rem;
}

.alert-icon {
  font-size: 1.25rem;
  line-height: 1;
}

.alert-content {
  flex: 1;
}

h4 {
  font-size: 0.875rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

p {
  font-size: 0.875rem;
  line-height: 1.4;
}

.alert-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-retry {
  background: #ffffff;
  border: 1px solid #fca5a5;
  color: #991b1b;
  padding: 0.25rem 0.6rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
}

.btn-retry:hover {
  background: #fee2e2;
}

.btn-dismiss {
  background: transparent;
  border: none;
  color: #991b1b;
  cursor: pointer;
  font-size: 0.875rem;
  padding: 0.25rem;
}
</style>
