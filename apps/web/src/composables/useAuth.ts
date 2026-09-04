import { ref, computed } from 'vue';
import { authApi } from '../api/authApi';
import type { CurrentUser, LoginRequest } from '../types';

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

function getInitialUser(): CurrentUser | null {
  try {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

const token = ref<string | null>(localStorage.getItem(TOKEN_KEY));
const currentUser = ref<CurrentUser | null>(getInitialUser());
const showLoginModal = ref<boolean>(false);
const isLoading = ref<boolean>(false);
const error = ref<string | null>(null);

if (typeof window !== 'undefined') {
  window.addEventListener('auth:unauthorized', () => {
    token.value = null;
    currentUser.value = null;
    showLoginModal.value = true;
  });
}

export function useAuth() {
  const isAuthenticated = computed(() => !!token.value);
  const isAdmin = computed(() => currentUser.value?.role === 'Admin');
  const isUser = computed(() => currentUser.value?.role === 'User');

  async function login(request: LoginRequest): Promise<void> {
    isLoading.value = true;
    error.value = null;
    try {
      const response = await authApi.login(request);
      token.value = response.token;
      currentUser.value = {
        username: response.username,
        role: response.role
      };
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(USER_KEY, JSON.stringify(currentUser.value));
      showLoginModal.value = false;
    } catch (err) {
      token.value = null;
      currentUser.value = null;
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      throw err;
    } finally {
      isLoading.value = false;
    }
  }

  function logout(): void {
    token.value = null;
    currentUser.value = null;
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  function openLoginModal(): void {
    showLoginModal.value = true;
  }

  function closeLoginModal(): void {
    showLoginModal.value = false;
  }

  return {
    token: computed(() => token.value),
    currentUser: computed(() => currentUser.value),
    isAuthenticated,
    isAdmin,
    isUser,
    isLoading: computed(() => isLoading.value),
    error: computed(() => error.value),
    showLoginModal,
    login,
    logout,
    openLoginModal,
    closeLoginModal
  };
}
