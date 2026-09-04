/**
 * RFC 7807 Problem Details representation returned by GlobalExceptionHandlerMiddleware.
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;

  // Custom Domain Extensions:
  productCode?: string;
  warehouseCode?: string;
  requiredQuantity?: number;
  availableQuantity?: number;
  missingQuantity?: number;
  entityName?: string;
  entityKey?: string;
  errors?: Record<string, string[]>;
}
