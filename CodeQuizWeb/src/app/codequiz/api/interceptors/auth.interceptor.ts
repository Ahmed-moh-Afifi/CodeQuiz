import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthStore } from '../auth.store';
import { AuthApiService } from '../services/auth-api.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => {
  const authStore = inject(AuthStore);
  const token = authStore.accessToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && authStore.refreshToken()) {
        const authApi = inject(AuthApiService);
        return authApi
          .refresh({
            accessToken: authStore.accessToken()!,
            refreshToken: authStore.refreshToken()!,
          })
          .pipe(
            switchMap((response) => {
              if (response.success && response.data) {
                authStore.setTokens(response.data);
                const retryReq = req.clone({
                  setHeaders: { Authorization: `Bearer ${response.data.accessToken}` },
                });
                return next(retryReq);
              }
              authStore.clear();
              return throwError(() => error);
            }),
            catchError((refreshError) => {
              authStore.clear();
              return throwError(() => refreshError);
            }),
          );
      }
      return throwError(() => error);
    }),
  );
};
