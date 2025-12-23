import type { ApiResponse, ApiError, ProblemDetails } from './types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

/**
 * API Client for CobranzaCloud
 * Handles all HTTP requests with proper error handling and type safety
 */
class ApiClient {
  private baseUrl: string;
  private getToken: () => string | null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.getToken = () => null; // Will be set by auth provider
  }

  setTokenGetter(getter: () => string | null) {
    this.getToken = getter;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    const token = this.getToken();

    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (token) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new ApiRequestError(
          response.status,
          errorData?.title || errorData?.message || 'Request failed',
          errorData
        );
      }

      const data = await response.json();
      return {
        data,
        status: response.status,
        ok: true,
      };
    } catch (error) {
      if (error instanceof ApiRequestError) {
        throw error;
      }
      throw new ApiRequestError(0, 'Network error', null);
    }
  }

  async get<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'GET' });
  }

  async post<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async put<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async patch<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

/**
 * Custom error class for API requests
 */
export class ApiRequestError extends Error {
  status: number;
  data: ProblemDetails | ApiError | null;

  constructor(status: number, message: string, data: ProblemDetails | ApiError | null) {
    super(message);
    this.name = 'ApiRequestError';
    this.status = status;
    this.data = data;
  }
}

// Singleton instance
export const apiClient = new ApiClient(API_BASE_URL);

// Helper for checking API health
export async function checkApiHealth(): Promise<boolean> {
  try {
    await apiClient.get('/health');
    return true;
  } catch {
    return false;
  }
}
