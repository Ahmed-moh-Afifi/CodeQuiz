import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import { UserDTO } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Users`;
  }

  getCurrentUser(): Observable<ApiResponse<UserDTO>> {
    return this.http.get<ApiResponse<UserDTO>>(this.url);
  }

  searchUsers(
    query: string,
    lastDate?: string,
    lastId?: string,
  ): Observable<ApiResponse<UserDTO[]>> {
    let params = new HttpParams().set('query', query);
    if (lastDate) params = params.set('lastDate', lastDate);
    if (lastId) params = params.set('lastId', lastId);
    return this.http.get<ApiResponse<UserDTO[]>>(`${this.url}/Search`, { params });
  }

  updateUser(userId: string, user: UserDTO): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.url}/${userId}`, user);
  }

  isUsernameAvailable(userName: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(
      `${this.url}/Username/${encodeURIComponent(userName)}/Available`,
    );
  }
}
