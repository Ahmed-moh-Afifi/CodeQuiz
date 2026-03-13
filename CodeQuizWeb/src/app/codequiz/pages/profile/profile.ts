import { Component, signal, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CqButton, CqSkeleton, CqDialogService, CqToastService } from '../../../design-system';
import { AuthStore } from '../../api/auth.store';
import { AuthService } from '../../services/auth.service';
import { UsersService } from '../../services/users.service';
import type { UserDTO } from '../../api/models/user.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CqButton, CqSkeleton, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile implements OnInit {
  private readonly authStore = inject(AuthStore);
  private readonly authService = inject(AuthService);
  private readonly usersService = inject(UsersService);
  private readonly dialogService = inject(CqDialogService);
  private readonly toastService = inject(CqToastService);
  private readonly router = inject(Router);

  loading = signal(true);
  user = signal<UserDTO | null>(null);

  // Password change form
  currentPassword = signal('');
  newPassword = signal('');
  confirmPassword = signal('');
  changingPassword = signal(false);
  passwordError = signal('');

  async ngOnInit() {
    this.loading.set(true);
    try {
      const user = await this.usersService.getCurrentUser();
      this.user.set(user);
    } finally {
      this.loading.set(false);
    }
  }

  async changePassword() {
    this.passwordError.set('');

    if (!this.currentPassword() || !this.newPassword() || !this.confirmPassword()) {
      this.passwordError.set('All fields are required');
      return;
    }

    if (this.newPassword().length < 6) {
      this.passwordError.set('New password must be at least 6 characters');
      return;
    }

    if (this.newPassword() !== this.confirmPassword()) {
      this.passwordError.set('Passwords do not match');
      return;
    }

    this.changingPassword.set(true);
    try {
      const username = this.user()?.userName ?? this.authStore.user()?.userName ?? '';
      await this.authService.resetPassword({
        username,
        password: this.currentPassword(),
        newPassword: this.newPassword(),
      });

      this.currentPassword.set('');
      this.newPassword.set('');
      this.confirmPassword.set('');
      this.toastService.success('Password changed successfully');
    } catch (err: any) {
      this.passwordError.set(err?.message ?? 'Failed to change password');
    } finally {
      this.changingPassword.set(false);
    }
  }

  get joinDate(): string {
    const d = this.user()?.joinDate;
    if (!d) return '';
    return new Date(d).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  goBack() {
    this.router.navigate(['/app']);
  }
}
