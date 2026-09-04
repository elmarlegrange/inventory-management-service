import { apiClient } from './apiClient';
import type { AuthResponse, CurrentUser, LoginRequest } from '../types';

export const authApi = {
  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/login', request);
    return response.data;
  },

  async getCurrentUser(): Promise<CurrentUser> {
    const response = await apiClient.get<CurrentUser>('/auth/me');
    return response.data;
  }
};
