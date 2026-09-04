<script setup lang="ts">
import { ref } from 'vue';
import ModalDialog from '../common/ModalDialog.vue';
import ErrorAlert from '../common/ErrorAlert.vue';
import { useAuth } from '../../composables/useAuth';
import { formatErrorMessage } from '../../api/apiClient';

defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'login-success'): void;
}>();

const { login, isLoading } = useAuth();

const username = ref('');
const password = ref('');
const errorMessage = ref<string | null>(null);

function fillAdminCredentials() {
  username.value = 'admin';
  password.value = 'Admin123!';
  errorMessage.value = null;
}

function fillUserCredentials() {
  username.value = 'user';
  password.value = 'User123!';
  errorMessage.value = null;
}

async function handleLogin() {
  if (!username.value.trim() || !password.value.trim()) {
    errorMessage.value = 'Please enter both username and password.';
    return;
  }

  errorMessage.value = null;
  try {
    await login({
      username: username.value.trim(),
      password: password.value.trim()
    });
    username.value = '';
    password.value = '';
    emit('login-success');
    emit('close');
  } catch (err) {
    errorMessage.value = formatErrorMessage(err, 'Authentication failed. Please check your credentials.');
  }
}
</script>

<template>
  <ModalDialog :is-open="isOpen" title="Sign In to Inventory Engine" @close="$emit('close')">
    <div class="login-container">
      <div class="quick-credentials">
        <span class="quick-label">Quick Sign-In:</span>
        <div class="quick-buttons">
          <button type="button" class="btn btn-quick btn-quick-admin" @click="fillAdminCredentials">
            👤 Admin Demo (Admin123!)
          </button>
          <button type="button" class="btn btn-quick btn-quick-user" @click="fillUserCredentials">
            👤 User Demo (User123!)
          </button>
        </div>
      </div>

      <ErrorAlert
        v-if="errorMessage"
        :message="errorMessage"
        @dismiss="errorMessage = null"
      />

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="username">Username</label>
          <input
            id="username"
            v-model="username"
            type="text"
            required
            placeholder="e.g. admin or user"
            autocomplete="username"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            placeholder="••••••••"
            autocomplete="current-password"
          />
        </div>

        <div class="form-actions">
          <button type="submit" class="btn btn-primary btn-block" :disabled="isLoading">
            <span v-if="isLoading">Signing in...</span>
            <span v-else>Sign In</span>
          </button>
        </div>
      </form>
    </div>
  </ModalDialog>
</template>

<style scoped>
.login-container {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.quick-credentials {
  background: #f8fafc;
  border: 1px dashed #cbd5e1;
  border-radius: 6px;
  padding: 0.75rem;
}

.quick-label {
  display: block;
  font-size: 0.75rem;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  margin-bottom: 0.5rem;
}

.quick-buttons {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.btn-quick {
  flex: 1;
  font-size: 0.8rem;
  padding: 0.4rem 0.6rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  text-align: center;
  transition: all 0.15s;
}

.btn-quick-admin {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1d4ed8;
}

.btn-quick-admin:hover {
  background: #dbeafe;
}

.btn-quick-user {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #15803d;
}

.btn-quick-user:hover {
  background: #dcfce7;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #334155;
}

input {
  padding: 0.6rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.875rem;
}

input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.2);
}

.btn-block {
  width: 100%;
  padding: 0.65rem 1rem;
  font-weight: 600;
  font-size: 0.9rem;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
  border: 1px solid #2563eb;
  border-radius: 6px;
  cursor: pointer;
}

.btn-primary:hover:not(:disabled) {
  background: #1d4ed8;
}

.btn-primary:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}
</style>
