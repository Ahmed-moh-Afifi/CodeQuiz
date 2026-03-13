import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { UsersApiService } from '../api/services/users-api.service';
import type { UserDTO } from '../api/models/user.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly usersApi = inject(UsersApiService);

  async getCurrentUser(): Promise<UserDTO> {
    const response = await firstValueFrom(this.usersApi.getCurrentUser());
    return response.data!;
  }

  async searchUsers(query: string, lastDate?: string, lastId?: string): Promise<UserDTO[]> {
    const response = await firstValueFrom(this.usersApi.searchUsers(query, lastDate, lastId));
    return response.data!;
  }

  async updateUser(userId: string, user: UserDTO): Promise<void> {
    await firstValueFrom(this.usersApi.updateUser(userId, user));
  }

  async isUsernameAvailable(userName: string): Promise<boolean> {
    const response = await firstValueFrom(this.usersApi.isUsernameAvailable(userName));
    return response.data!;
  }
}
