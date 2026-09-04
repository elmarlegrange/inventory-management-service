export interface WarehouseDto {
  code: string;
  name: string;
  createdAt: string;
}

export interface CreateWarehouseRequest {
  code: string;
  name: string;
}

export interface WarehouseStockItemDto {
  productCode: string;
  productName: string;
  quantity: number;
  updatedAt: string;
}

export interface AddStockItemRequest {
  productCode: string;
  quantity: number;
}
