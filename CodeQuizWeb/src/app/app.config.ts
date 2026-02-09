import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { App } from './app';
import { DesignSystemDemoComponent } from './design-system-demo/design-system-demo';

const routes: Routes = [
  { path: '', component: App },
  { path: 'design-system', component: DesignSystemDemoComponent },
];

export const appConfig: ApplicationConfig = {
  providers: [provideZonelessChangeDetection(), provideHttpClient(), provideRouter(routes)],
};
