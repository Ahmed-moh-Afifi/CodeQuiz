import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CqSideNav, CqSideNavItem, CqAvatar } from '../../design-system';
import { Dashboard } from '../pages/dashboard/dashboard';
import { CreatedQuizzes } from '../pages/created-quizzes/created-quizzes';
import { JoinedQuizzes } from '../pages/joined-quizzes/joined-quizzes';
import { AuthService } from '../services/auth.service';
import type { UserDTO } from '../api/models/user.models';

@Component({
  selector: 'app-app',
  imports: [CqSideNav, CqSideNavItem, CqAvatar, Dashboard, CreatedQuizzes, JoinedQuizzes],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  get user(): UserDTO | null {
    return this.authService.user;
  }

  openProfile() {
    this.router.navigate(['/app/profile']);
  }

  async logout() {
    this.authService.logout();
    this.router.navigate(['/app/login']);
  }
}
