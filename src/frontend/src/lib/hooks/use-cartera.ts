'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type {
  CarteraResumenResponse,
  CarteraAntiguedadResponse,
  ClientesListResponse,
  ClienteDetailResponse,
} from '../api/types';

// ============================================================
// CARTERA HOOKS (M3)
// ============================================================

/**
 * Hook to fetch cartera resumen/summary
 */
export function useCarteraResumen() {
  return useQuery({
    queryKey: ['cartera', 'resumen'],
    queryFn: async () => {
      const response = await apiClient.get<CarteraResumenResponse>('/api/cartera/resumen');
      return response.data;
    },
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Hook to fetch cartera antiguedad (aging report)
 */
export function useCarteraAntiguedad() {
  return useQuery({
    queryKey: ['cartera', 'antiguedad'],
    queryFn: async () => {
      const response = await apiClient.get<CarteraAntiguedadResponse>('/api/cartera/antiguedad');
      return response.data;
    },
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Hook to fetch clientes list with pagination and filters
 */
export function useClientes(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  conSaldo?: boolean;
  orderBy?: string;
  orderDir?: 'asc' | 'desc';
}) {
  const queryParams = new URLSearchParams();

  if (params?.page) queryParams.set('page', params.page.toString());
  if (params?.pageSize) queryParams.set('pageSize', params.pageSize.toString());
  if (params?.search) queryParams.set('search', params.search);
  if (params?.conSaldo !== undefined) queryParams.set('conSaldo', params.conSaldo.toString());
  if (params?.orderBy) queryParams.set('orderBy', params.orderBy);
  if (params?.orderDir) queryParams.set('orderDir', params.orderDir);

  const queryString = queryParams.toString();
  const endpoint = `/api/clientes${queryString ? `?${queryString}` : ''}`;

  return useQuery({
    queryKey: ['clientes', params],
    queryFn: async () => {
      const response = await apiClient.get<ClientesListResponse>(endpoint);
      return response.data;
    },
    staleTime: 1 * 60 * 1000, // 1 minute
  });
}

/**
 * Hook to fetch cliente detail by ID
 */
export function useClienteDetalle(id: string | undefined) {
  return useQuery({
    queryKey: ['clientes', id],
    queryFn: async () => {
      const response = await apiClient.get<ClienteDetailResponse>(`/api/clientes/${id}`);
      return response.data;
    },
    enabled: !!id,
    staleTime: 1 * 60 * 1000, // 1 minute
  });
}

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

/**
 * Format money to Mexican Peso (MXN)
 */
export function formatMoney(amount: number): string {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

/**
 * Format percentage
 */
export function formatPercent(value: number): string {
  return `${value.toFixed(1)}%`;
}

/**
 * Get color class for overdue days
 */
export function getOverdueColorClass(days: number): string {
  if (days <= 0) return 'text-green-600';
  if (days <= 30) return 'text-yellow-600';
  if (days <= 60) return 'text-orange-600';
  return 'text-destructive';
}
