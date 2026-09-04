import { apiClient } from './apiClient';
import type { WarehouseDto, CreateWarehouseRequest, WarehouseStockItemDto, AddStockItemRequest } from '../types';

export const warehousesApi = {
  async getAll(): Promise<WarehouseDto[]> {
    const response = await apiClient.get<WarehouseDto[]>('/warehouses');
    return response.data;
  },

  async getByCode(code: string): Promise<WarehouseDto> {
    const response = await apiClient.get<WarehouseDto>(`/warehouses/${encodeURIComponent(code)}`);
    return response.data;
  },

  async create(request: CreateWarehouseRequest): Promise<WarehouseDto> {
    const response = await apiClient.post<WarehouseDto>('/warehouses', request);
    return response.data;
  },

  async getStock(code: string): Promise<WarehouseStockItemDto[]> {
    const response = await apiClient.get<WarehouseStockItemDto[]>(`/warehouses/${encodeURIComponent(code)}/stock`);
    return response.data;
  },

  async addStock(code: string, request: AddStockItemRequest): Promise<void> {
    await apiClient.post<void>(`/warehouses/${encodeURIComponent(code)}/stock`, request);
  }
};
