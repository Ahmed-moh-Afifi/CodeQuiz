import { Injectable, signal, computed } from '@angular/core';
import { TokenModel, LoginResult } from './models/auth.models';
import { UserDTO } from './models/user.models';

const ACCESS_TOKEN_KEY = 'cq_access_token';
const REFRESH_TOKEN_KEY = 'cq_refresh_token';
const USER_KEY = 'cq_user';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly _user = signal<UserDTO | null>(this.loadUser());
  private readonly _accessToken = signal<string | null>(this.loadToken(ACCESS_TOKEN_KEY));
  private readonly _refreshToken = signal<string | null>(this.loadToken(REFRESH_TOKEN_KEY));

  readonly user = this._user.asReadonly();
  readonly accessToken = this._accessToken.asReadonly();
  readonly refreshToken = this._refreshToken.asReadonly();
  readonly isAuthenticated = computed(() => !!this._accessToken());

  setLoginResult(result: LoginResult): void {
    this.setTokens(result.tokenModel);
    this.setUser(result.user);
  }

  setTokens(tokens: TokenModel): void {
    this._accessToken.set(tokens.accessToken);
    this._refreshToken.set(tokens.refreshToken);
    localStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);
  }

  setUser(user: UserDTO): void {
    this._user.set(user);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  clear(): void {
    this._user.set(null);
    this._accessToken.set(null);
    this._refreshToken.set(null);
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  private loadToken(key: string): string | null {
    return localStorage.getItem(key);
  }

  private loadUser(): UserDTO | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserDTO;
    } catch {
      return null;
    }
  }
}
