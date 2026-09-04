import axios, { AxiosError } from 'axios';
import type { ProblemDetails } from '../types/problemDetails';

/**
 * Pre-configured Axios client targeting the Inventory API.
 * Uses Vite proxy in development or VITE_API_URL in production.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '',
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json, application/problem+json'
  }
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      if (!error.config?.url?.includes('/auth/login')) {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('auth_user');
        window.dispatchEvent(new CustomEvent('auth:unauthorized'));
      }
    }
    return Promise.reject(error);
  }
);

/**
 * Extracts structured RFC 7807 ProblemDetails from an Axios error, if present.
 */
export function extractProblemDetails(error: unknown): ProblemDetails | null {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ProblemDetails>;
    if (axiosError.response && axiosError.response.data && typeof axiosError.response.data === 'object') {
      const data = axiosError.response.data;
      if (data.title && typeof data.status === 'number') {
        return data;
      }
    }
  }
  return null;
}

/**
 * Formats a friendly display message from an RFC 7807 error or fallback message.
 */
export function formatErrorMessage(error: unknown, fallbackMessage = 'An unexpected error occurred'): string {
  const problem = extractProblemDetails(error);
  if (problem) {
    if (problem.missingQuantity !== undefined && problem.missingQuantity > 0) {
      return `${problem.detail} (Shortfall: ${problem.missingQuantity} units)`;
    }
    if (problem.detail) {
      return problem.detail;
    }
    if (problem.title) {
      return problem.title;
    }
  }
  if (error instanceof Error) {
    return error.message;
  }
  return fallbackMessage;
}
