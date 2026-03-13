import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthApiService } from '../api/services/auth-api.service';
import { AuthStore } from '../api/auth.store';
import type {
  RegisterModel,
  LoginModel,
  LoginResult,
  ResetPasswordModel,
  ForgetPasswordModel,
  ResetPasswordTnModel,
} from '../api/models/auth.models';
import type { UserDTO } from '../api/models/user.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authApi = inject(AuthApiService);
  private readonly authStore = inject(AuthStore);

  get user(): UserDTO | null {
    return this.authStore.user();
  }

  get isAuthenticated(): boolean {
    return this.authStore.isAuthenticated();
  }

  async register(model: RegisterModel): Promise<UserDTO> {
    const response = await firstValueFrom(this.authApi.register(model));
    return response.data!;
  }

  async login(model: LoginModel): Promise<LoginResult> {
    const response = await firstValueFrom(this.authApi.login(model));
    this.authStore.setLoginResult(response.data!);
    return response.data!;
  }

  async resetPassword(model: ResetPasswordModel): Promise<void> {
    await firstValueFrom(this.authApi.resetPassword(model));
  }

  async forgotPassword(model: ForgetPasswordModel): Promise<void> {
    await firstValueFrom(this.authApi.forgetPasswordRequest(model));
  }

  async resetPasswordWithToken(model: ResetPasswordTnModel): Promise<void> {
    await firstValueFrom(this.authApi.resetPasswordWithToken(model));
  }

  logout(): void {
    this.authStore.clear();
  }
}
