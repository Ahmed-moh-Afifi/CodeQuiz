import {
  Injectable,
  ApplicationRef,
  createComponent,
  EnvironmentInjector,
  ComponentRef,
} from '@angular/core';
import { CqToastContainer, ToastConfig } from '../components/toast/cq-toast-container';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastOptions {
  duration?: number; // ms, default 4000
  action?: string; // optional action button text
}

@Injectable({ providedIn: 'root' })
export class CqToastService {
  private containerRef: ComponentRef<CqToastContainer> | null = null;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector,
  ) {}

  success(message: string, options?: ToastOptions) {
    this.show('success', message, options);
  }

  error(message: string, options?: ToastOptions) {
    this.show('error', message, options);
  }

  warning(message: string, options?: ToastOptions) {
    this.show('warning', message, options);
  }

  info(message: string, options?: ToastOptions) {
    this.show('info', message, options);
  }

  private show(type: ToastType, message: string, options?: ToastOptions) {
    const container = this.getOrCreateContainer();
    container.instance.addToast({
      id: Date.now() + Math.random(),
      type,
      message,
      duration: options?.duration ?? 4000,
      action: options?.action,
    });
  }

  private getOrCreateContainer(): ComponentRef<CqToastContainer> {
    if (this.containerRef) return this.containerRef;

    this.containerRef = createComponent(CqToastContainer, {
      environmentInjector: this.injector,
    });

    document.body.appendChild(this.containerRef.location.nativeElement);
    this.appRef.attachView(this.containerRef.hostView);

    return this.containerRef;
  }
}
