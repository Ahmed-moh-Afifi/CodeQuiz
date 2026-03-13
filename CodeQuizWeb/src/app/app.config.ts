import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { App } from './app';
import { DesignSystemDemoComponent } from './design-system-demo/design-system-demo';
import { TestingPage } from './design-system-demo/testing-page/testing-page';
import { authInterceptor } from './codequiz/api';
import { errorInterceptor } from './codequiz/api/interceptors/error.interceptor';
import { authGuard, guestGuard } from './codequiz/guards/auth.guard';

const routes: Routes = [
  { path: '', component: App },
  { path: 'design-system', component: DesignSystemDemoComponent },
  { path: 'design-system/testing-page', component: TestingPage },

  // Auth pages (guest only)
  {
    path: 'app/login',
    canActivate: [guestGuard],
    loadComponent: () => import('./codequiz/pages/login/login').then((m) => m.Login),
  },
  {
    path: 'app/register',
    canActivate: [guestGuard],
    loadComponent: () => import('./codequiz/pages/register/register').then((m) => m.Register),
  },

  // Main shell (authenticated)
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./codequiz/app/app').then((m) => m.App),
  },

  // Standalone quiz pages (authenticated)
  {
    path: 'app/quiz/create',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./codequiz/pages/create-quiz/create-quiz').then((m) => m.CreateQuiz),
  },
  {
    path: 'app/quiz/take',
    canActivate: [authGuard],
    loadComponent: () => import('./codequiz/pages/take-quiz/take-quiz').then((m) => m.TakeQuiz),
  },
  {
    path: 'app/quiz/view',
    canActivate: [authGuard],
    loadComponent: () => import('./codequiz/pages/view-quiz/view-quiz').then((m) => m.ViewQuiz),
  },
  {
    path: 'app/quiz/grade',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./codequiz/pages/grade-attempt/grade-attempt').then((m) => m.GradeAttempt),
  },
  {
    path: 'app/quiz/review',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./codequiz/pages/review-quiz/review-quiz').then((m) => m.ReviewQuiz),
  },
  {
    path: 'app/profile',
    canActivate: [authGuard],
    loadComponent: () => import('./codequiz/pages/profile/profile').then((m) => m.Profile),
  },
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideRouter(routes),
  ],
};
