export type UserRole = 'Admin' | 'User';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  username: string;
  role: UserRole;
}

export interface CurrentUser {
  id?: string;
  username: string;
  role: UserRole;
}
