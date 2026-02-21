import {
  Injectable,
  ApplicationRef,
  createComponent,
  EnvironmentInjector,
  ComponentRef,
} from '@angular/core';
import { CqLoadingOverlay } from '../components/loading-overlay/cq-loading-overlay';

let nextId = 0;

export interface LoadingRef {
  id: number;
}

@Injectable({ providedIn: 'root' })
export class CqLoadingService {
  private overlayRef: ComponentRef<CqLoadingOverlay> | null = null;
  private stack: { id: number; message: string }[] = [];

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector,
  ) {}

  /**
   * Show the loading overlay.
   * Multiple calls stack — the overlay stays visible until all refs are hidden.
   * @param message Optional loading message displayed below the spinner
   * @returns A `LoadingRef` token to pass to `hide()` later
   */
  show(message = ''): LoadingRef {
    const ref: LoadingRef = { id: ++nextId };
    this.stack.push({ id: ref.id, message });

    const overlay = this.getOrCreateOverlay();
    // Show with the most recent message
    overlay.instance.show(this.stack[this.stack.length - 1].message);

    return ref;
  }

  /**
   * Hide a specific loading ref. Overlay disappears when the stack is empty.
   */
  hide(ref: LoadingRef) {
    this.stack = this.stack.filter((s) => s.id !== ref.id);

    if (this.stack.length === 0) {
      this.overlayRef?.instance.hide();
    } else {
      // Update message to the top of the stack
      this.overlayRef?.instance.show(this.stack[this.stack.length - 1].message);
    }
  }

  /**
   * Force-clear all loading refs and hide the overlay.
   */
  hideAll() {
    this.stack = [];
    this.overlayRef?.instance.hide();
  }

  private getOrCreateOverlay(): ComponentRef<CqLoadingOverlay> {
    if (this.overlayRef) return this.overlayRef;

    this.overlayRef = createComponent(CqLoadingOverlay, {
      environmentInjector: this.injector,
    });

    document.body.appendChild(this.overlayRef.location.nativeElement);
    this.appRef.attachView(this.overlayRef.hostView);

    return this.overlayRef;
  }
}
