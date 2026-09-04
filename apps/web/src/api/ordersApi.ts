import { apiClient } from './apiClient';
import type { OrderDto, CreateOrderRequest } from '../types';

export const ordersApi = {
  async createOrder(request: CreateOrderRequest): Promise<OrderDto> {
    const response = await apiClient.post<OrderDto>('/orders', request);
    return response.data;
  }
};
