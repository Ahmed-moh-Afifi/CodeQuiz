import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import {
  RegisterModel,
  LoginModel,
  LoginResult,
  TokenModel,
  ResetPasswordModel,
  ForgetPasswordModel,
  ResetPasswordTnModel,
} from '../models/auth.models';
import { UserDTO } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Authentication`;
  }

  register(model: RegisterModel): Observable<ApiResponse<UserDTO>> {
    return this.http.post<ApiResponse<UserDTO>>(`${this.url}/Register`, model);
  }

  login(model: LoginModel): Observable<ApiResponse<LoginResult>> {
    return this.http.post<ApiResponse<LoginResult>>(`${this.url}/Login`, model);
  }

  refresh(model: TokenModel): Observable<ApiResponse<TokenModel>> {
    return this.http.post<ApiResponse<TokenModel>>(`${this.url}/Refresh`, model);
  }

  resetPassword(model: ResetPasswordModel): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.url}/ResetPassword`, model);
  }

  forgetPasswordRequest(model: ForgetPasswordModel): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.url}/ForgetPasswordRequest`, model);
  }

  resetPasswordWithToken(model: ResetPasswordTnModel): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.url}/ResetPasswordTn`, model);
  }
}
