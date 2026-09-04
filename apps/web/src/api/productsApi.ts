import { apiClient } from './apiClient';
import type { ProductDto, CreateProductRequest, ProductStockLocationDto } from '../types';

export const productsApi = {
  async getAll(): Promise<ProductDto[]> {
    const response = await apiClient.get<ProductDto[]>('/products');
    return response.data;
  },

  async getByCode(code: string): Promise<ProductDto> {
    const response = await apiClient.get<ProductDto>(`/products/${encodeURIComponent(code)}`);
    return response.data;
  },

  async create(request: CreateProductRequest): Promise<ProductDto> {
    const response = await apiClient.post<ProductDto>('/products', request);
    return response.data;
  },

  async getStock(code: string): Promise<ProductStockLocationDto[]> {
    const response = await apiClient.get<ProductStockLocationDto[]>(`/products/${encodeURIComponent(code)}/stock`);
    return response.data;
  }
};
