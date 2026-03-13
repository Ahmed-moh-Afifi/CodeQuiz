import { InjectionToken } from '@angular/core';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => 'http://129.151.234.105/api',
});

export const HUB_BASE_URL = new InjectionToken<string>('HUB_BASE_URL', {
  providedIn: 'root',
  factory: () => 'http://129.151.234.105/hubs',
});
