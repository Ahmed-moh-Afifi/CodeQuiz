import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
  @Input() token: string = '';

  email: string = '';
  newPassword: string = '';
  confirmPassword: string = '';

  loading: boolean = false;
  error: string = '';
  success: string = '';

  private apiUrl = 'https://129.151.234.105/api/Authentication/ResetPasswordTn';
  // private apiUrl = 'http://localhost:5062/api/Authentication/ResetPasswordTn';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    // If token is passed via input, used directly.
    // Parent component handles extraction.
  }

  onSubmit(event: Event) {
    event.preventDefault();
    this.error = '';
    this.success = '';

    if (!this.email || !this.newPassword || !this.confirmPassword) {
      this.error = 'All fields are required.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    if (!this.token) {
      this.error = 'Invalid token. Please use the link provided in your email.';
      return;
    }

    this.loading = true;

    const payload = {
      email: this.email,
      token: this.token,
      newPassword: this.newPassword,
    };

    this.http.put(this.apiUrl, payload).subscribe({
      next: (response: any) => {
        this.loading = false;
        if (response.success) {
          this.success = response.message || 'Password reset successfully.';
          this.email = '';
          this.newPassword = '';
          this.confirmPassword = '';
        } else {
          console.log(response);
          this.error = response.message || 'Failed to reset password.';
        }
      },
      error: (err) => {
        this.loading = false;
        console.error(err);
        this.error = err.error?.message || 'An error occurred. Please try again.';
      },
    });
  }
}
