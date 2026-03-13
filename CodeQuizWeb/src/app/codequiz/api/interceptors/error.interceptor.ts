import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { CqDialogService } from '../../../design-system';
import type { ApiResponse } from '../models/api-response.model';

/**
 * HTTP interceptor that catches error responses and shows an error dialog
 * using the CQ design system's dialog service.
 *
 * Errors are categorized and displayed with appropriate messaging:
 * - 400: Bad Request (validation errors from the backend)
 * - 401: Unauthorized (session expired / not authenticated)
 * - 403: Forbidden (missing permissions)
 * - 404: Not Found
 * - 500: Internal Server Error
 * - 503: Service Unavailable
 * - 0: Network error (no connection)
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const dialog = inject(CqDialogService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Don't show dialog for token refresh failures — the auth interceptor handles those
      if (req.url.includes('/Authentication/Refresh')) {
        return throwError(() => error);
      }

      const { title, message } = extractErrorInfo(error);
      dialog.alert('danger', title, message);

      return throwError(() => error);
    }),
  );
};

function extractErrorInfo(error: HttpErrorResponse): { title: string; message: string } {
  // Try to parse backend ApiResponse message
  const apiMessage = tryGetApiMessage(error);

  if (error.status === 0) {
    return {
      title: 'Connection Error',
      message: 'Unable to reach the server. Please check your internet connection and try again.',
    };
  }

  if (error.status === 401) {
    return {
      title: 'Unauthorized',
      message: apiMessage ?? 'Your session has expired. Please log in again.',
    };
  }

  if (error.status === 403) {
    return {
      title: 'Access Denied',
      message: apiMessage ?? 'You do not have permission to perform this action.',
    };
  }

  if (error.status === 404) {
    return {
      title: 'Not Found',
      message: apiMessage ?? 'The requested resource was not found.',
    };
  }

  if (error.status === 400) {
    return {
      title: 'Request Error',
      message: apiMessage ?? 'The request was invalid. Please check your input and try again.',
    };
  }

  if (error.status === 503) {
    return {
      title: 'Service Unavailable',
      message: apiMessage ?? 'The service is temporarily unavailable. Please try again later.',
    };
  }

  if (error.status >= 500) {
    return {
      title: 'Server Error',
      message: apiMessage ?? 'Something went wrong on the server. Please try again later.',
    };
  }

  return {
    title: 'Error',
    message: apiMessage ?? 'An unexpected error occurred. Please try again.',
  };
}

function tryGetApiMessage(error: HttpErrorResponse): string | null {
  try {
    const body = error.error as ApiResponse<unknown> | null;
    if (body && typeof body === 'object' && 'message' in body && typeof body.message === 'string') {
      return body.message;
    }
  } catch {
    // Response body wasn't parsable JSON
  }
  return null;
}
