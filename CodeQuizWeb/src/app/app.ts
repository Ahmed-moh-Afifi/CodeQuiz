import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

interface Feature {
  title: string;
  description: string;
  image: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ResetPasswordComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  host: {
    '(mousemove)': 'onMouseMove($event)',
  },
})
export class App implements OnInit {
  onMouseMove(e: MouseEvent) {
    const x = e.clientX;
    const y = e.clientY;
    document.documentElement.style.setProperty('--cursor-x', x + 'px');
    document.documentElement.style.setProperty('--cursor-y', y + 'px');
  }

  title = 'CodeQuiz';
  downloadUrl = signal<string>('https://github.com/Ahmed-moh-Afifi/CodeQuiz/releases/latest');
  latestVersion = signal<string>('');
  resetToken: string | null = null;

  features: Feature[] = [
    {
      title: 'Interactive Dashboard',
      description:
        'Access all your quizzes in one place. View your created quizzes, joined sessions, and track your progress with our intuitive dashboard interface.',
      image: 'assets/screenshots/dashboard.svg',
    },
    {
      title: 'Create Custom Quizzes',
      description:
        'Design challenging coding assessments with multiple question types. Set time limits, points, and passing criteria to test your students effectively.',
      image: 'assets/screenshots/create-quiz.svg',
    },
    {
      title: 'Secure Quiz Joining',
      description:
        'Join exam sessions securely with unique access codes. The verification process ensures that only authorized participants can take the assessment.',
      image: 'assets/screenshots/join-quiz.svg',
    },
    {
      title: 'Detailed Grade Review',
      description:
        'Receive instant feedback on your submissions. Review your code, check test case results, and understand your grade breakdown immediately after completion.',
      image: 'assets/screenshots/grade-review.svg',
    },
  ];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.checkResetToken();

    this.http
      .get<any>('https://api.github.com/repos/Ahmed-moh-Afifi/CodeQuiz/releases/latest')
      .subscribe({
        next: (data) => {
          this.latestVersion.set(data.tag_name);
          const asset = data.assets.find((a: any) => a.browser_download_url.endsWith('.zip'));
          if (asset) {
            this.downloadUrl.set(asset.browser_download_url);
          } else {
            this.downloadUrl.set(data.html_url);
          }
        },
        error: (err) => console.error('Failed to fetch release', err),
      });
  }

  private checkResetToken() {
    const urlParams = new URLSearchParams(window.location.search);
    this.resetToken = urlParams.get('token');
  }
}
