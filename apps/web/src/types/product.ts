export interface ProductDto {
  code: string;
  name: string;
  createdAt: string;
}

export interface CreateProductRequest {
  code: string;
  name: string;
}

export interface ProductStockLocationDto {
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
  updatedAt: string;
}
