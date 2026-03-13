import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CqButton } from '../../../design-system';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CqButton, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  username = '';
  password = '';
  loading = signal(false);
  error = signal('');

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  async onLogin() {
    this.error.set('');

    if (!this.username.trim() || !this.password.trim()) {
      this.error.set('Please enter both username and password.');
      return;
    }

    this.loading.set(true);
    try {
      await this.authService.login({
        username: this.username,
        password: this.password,
      });
      this.router.navigate(['/app']);
    } catch {
      // Error dialog is shown globally by the error interceptor
    } finally {
      this.loading.set(false);
    }
  }

  async onForgotPassword() {
    // TODO: Implement forgot password dialog
  }
}
