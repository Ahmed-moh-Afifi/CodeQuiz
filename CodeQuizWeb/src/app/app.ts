import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ResetPasswordComponent } from './reset-password/reset-password.component';

interface Feature {
  title: string;
  description: string;
  image: string;
}

interface FeatureHighlight {
  icon: string;
  title: string;
  description: string;
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

  tiltX = signal('0deg');
  tiltY = signal('0deg');

  onDownloadCardMove(e: MouseEvent) {
    const card = e.currentTarget as HTMLElement;
    const rect = card.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const centerX = rect.width / 2;
    const centerY = rect.height / 2;

    const rotateX = ((y - centerY) / centerY) * -5; // Max 5 deg rotation
    const rotateY = ((x - centerX) / centerX) * 5;

    this.tiltX.set(`${rotateX}deg`);
    this.tiltY.set(`${rotateY}deg`);
  }

  onDownloadCardLeave() {
    this.tiltX.set('0deg');
    this.tiltY.set('0deg');
  }

  title = 'CodeQuiz';
  downloadUrl = signal<string>('https://github.com/Ahmed-moh-Afifi/CodeQuiz/releases/latest');
  latestVersion = signal<string>('');
  resetToken: string | null = null;

  // Hero section highlights
  heroHighlights: FeatureHighlight[] = [
    {
      icon: 'assets/icons/robot.svg',
      title: 'AI-Powered',
      description: 'Intelligent assessment & grading',
    },
    {
      icon: 'assets/icons/clock.svg',
      title: 'Real-Time',
      description: 'Live updates & notifications',
    },
    {
      icon: 'assets/icons/lock.svg',
      title: 'Secure',
      description: 'Sandboxed code execution',
    },
  ];

  // Main features with screenshots
  features: Feature[] = [
    {
      title: 'Interactive Dashboard',
      description:
        'Access all your quizzes in one place. View your created quizzes, joined sessions, and track your progress with our intuitive dashboard interface.',
      image: 'assets/screenshots/dashboard.png',
    },
    {
      title: 'Comprehensive Quiz Builder',
      description:
        'Design challenging coding assessments with customizable settings. Set time limits, point values, passing criteria, and configure code execution options per question.',
      image: 'assets/screenshots/create-quiz.png',
    },
    {
      title: 'Smart Code Editor',
      description:
        'Write and test code with intelligent features including syntax highlighting, auto-completion, and configurable intellisense. Execute code instantly and see results in real-time.',
      image: 'assets/screenshots/join-quiz.png',
    },
    {
      title: 'AI-Assisted Grading',
      description:
        'Review submissions with AI-powered insights. Get suggested grades, detect suspicious patterns like hardcoded solutions, and provide detailed feedback—all in one unified grading interface.',
      image: 'assets/screenshots/grade-review.png',
    },
    {
      title: 'Detailed Statistics',
      description:
        'Analyze student performance with comprehensive metrics. View total attempts, pass rates, grade distributions, and standard deviation to identify knowledge gaps and improving teaching strategies.',
      image: 'assets/screenshots/statistics.png',
    },
  ];

  // AI capabilities
  aiFeatures = [
    {
      icon: 'assets/icons/flask.svg',
      title: 'Smart Test Generation',
      description:
        'Automatically generate comprehensive test cases including edge cases, boundary values, and anti-hardcoding scenarios.',
    },
    {
      icon: 'assets/icons/target.svg',
      title: 'Intelligent Assessment',
      description:
        'AI analyzes code quality, detects partial implementations, and identifies test-case gaming attempts.',
    },
    {
      icon: 'assets/icons/chart.svg',
      title: 'Suggested Grades',
      description:
        'Get AI-powered grade recommendations with confidence scores and detailed reasoning.',
    },
    {
      icon: 'assets/icons/flag.svg',
      title: 'Cheating Detection',
      description:
        'Automatically flag suspicious solutions including hardcoded answers, empty stubs, and incorrect logic.',
    },
  ];

  // Security features
  securityFeatures = [
    {
      icon: 'assets/icons/docker.svg',
      title: 'Sandboxed Execution',
      description:
        'Code runs in isolated Docker containers with strict memory and CPU limits, ensuring complete isolation from the host system.',
    },
    {
      icon: 'assets/icons/lock.svg',
      title: 'Secure Authentication',
      description:
        'Industry-standard JWT tokens with access and refresh mechanism, password hashing, and rate limiting on sensitive endpoints.',
    },
    {
      icon: 'assets/icons/clock.svg',
      title: 'Timeout Protection',
      description:
        'Configurable execution timeouts prevent infinite loops and resource exhaustion attacks.',
    },
    {
      icon: 'assets/icons/shield.svg',
      title: 'Input Validation',
      description:
        'Server-side validation for all inputs with sanitized error messages to prevent information leakage.',
    },
  ];

  // Real-time features
  realtimeFeatures = [
    {
      icon: 'assets/icons/share.svg',
      title: 'Live Attempt Tracking',
      description: 'See student progress in real-time as they start and submit attempts.',
    },
    {
      icon: 'assets/icons/save.svg',
      title: 'Auto-Save',
      description: 'Never lose work with intelligent auto-save that triggers when you stop typing.',
    },
    {
      icon: 'assets/icons/bell.svg',
      title: 'Instant Notifications',
      description: 'Receive immediate updates when grades are posted or quizzes become available.',
    },
    {
      icon: 'assets/icons/clock.svg',
      title: 'Smart Timer',
      description:
        'Countdown timer with automatic submission when time expires—no submissions lost.',
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

  scrollToSection(sectionId: string) {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }
}
