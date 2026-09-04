export interface OrderDto {
  id: string;
  productCode: string;
  sourceWarehouseCode: string;
  destinationWarehouseCode: string;
  quantity: number;
  createdAt: string;
}

export interface CreateOrderRequest {
  productCode: string;
  sourceWarehouseCode: string;
  destinationWarehouseCode: string;
  quantity: number;
}
