import {
  ApplicationRef,
  ComponentRef,
  createComponent,
  EnvironmentInjector,
  Injectable,
  Injector,
  NgZone,
  Type,
} from '@angular/core';
import { CqDialogRef } from './cq-dialog-ref';
import { CqDialogHost, AlertDialogConfig, AlertDialogType } from './cq-dialog-host';

export interface DialogOptions {
  /** Data to assign to the component instance */
  data?: Record<string, any>;
  /** Dialog size: sm (360px), md (500px, default), lg (720px) */
  size?: 'sm' | 'md' | 'lg';
  /** Whether clicking the overlay closes the dialog (default: true) */
  closeOnOverlayClick?: boolean;
}

/**
 * Service for opening dialogs programmatically.
 *
 * Usage:
 * ```ts
 * // Alert dialog
 * this.dialog.alert('info', 'Did You Know?', 'Press Ctrl+S to save.');
 *
 * // Confirm dialog (returns true/false)
 * const ref = this.dialog.confirm('danger', 'Delete?', 'This is permanent.', 'Delete', 'Cancel');
 * ref.afterClosed$.subscribe(result => { if (result) { ... } });
 *
 * // Custom component dialog
 * const ref = this.dialog.open(MyFormComponent, { size: 'lg', data: { quizId: 42 } });
 * ref.afterClosed$.subscribe(result => console.log(result));
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class CqDialogService {
  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector,
    private ngZone: NgZone,
  ) {}

  /**
   * Open a custom component inside a dialog.
   * The component can inject `CqDialogRef` to close itself with a result.
   */
  open<T>(component: Type<T>, options: DialogOptions = {}): CqDialogRef {
    const dialogRef = new CqDialogRef();
    const hostRef = this.createHost(dialogRef, options);

    // Create the content component with CqDialogRef available for injection
    const contentInjector = Injector.create({
      providers: [{ provide: CqDialogRef, useValue: dialogRef }],
      parent: this.injector,
    });

    // Wait for the view to initialize, then insert the content component
    setTimeout(() => {
      if (hostRef.instance.contentContainer) {
        const contentRef = hostRef.instance.contentContainer.createComponent(component, {
          injector: contentInjector,
        });
        if (options.data) {
          Object.assign(contentRef.instance as any, options.data);
        }
      }
    });

    // Clean up when closed
    dialogRef.afterClosed$.subscribe(() => this.destroyHost(hostRef));

    return dialogRef;
  }

  /**
   * Show an alert dialog (single "OK" button).
   * Resolves when the user clicks OK.
   */
  alert(
    type: AlertDialogType,
    title: string,
    message: string,
    confirmText = 'OK',
  ): CqDialogRef<boolean> {
    return this.openAlert({
      type,
      title,
      message,
      confirmText,
    });
  }

  /**
   * Show a confirm dialog (confirm + cancel buttons).
   * Result is `true` if confirmed, `false` if cancelled.
   */
  confirm(
    type: AlertDialogType,
    title: string,
    message: string,
    confirmText = 'Confirm',
    cancelText = 'Cancel',
  ): CqDialogRef<boolean> {
    return this.openAlert({
      type,
      title,
      message,
      confirmText,
      cancelText,
    });
  }

  private openAlert(config: AlertDialogConfig): CqDialogRef<boolean> {
    const dialogRef = new CqDialogRef<boolean>();
    const hostRef = this.createHost(dialogRef, { size: 'sm' });
    hostRef.instance.alertConfig = config;
    dialogRef.afterClosed$.subscribe(() => this.destroyHost(hostRef));
    return dialogRef;
  }

  private createHost(
    dialogRef: CqDialogRef,
    options: DialogOptions = {},
  ): ComponentRef<CqDialogHost> {
    const hostRef = createComponent(CqDialogHost, {
      environmentInjector: this.injector,
    });

    hostRef.instance.dialogRef = dialogRef;
    hostRef.instance.ngZone = this.ngZone;
    if (options.closeOnOverlayClick !== undefined) {
      hostRef.instance.closeOnOverlayClick = options.closeOnOverlayClick;
    }
    if (options.size) {
      hostRef.instance.dialogContainerClass =
        options.size === 'md' ? 'cq-dialog' : `cq-dialog cq-dialog-${options.size}`;
    }

    document.body.appendChild(hostRef.location.nativeElement);
    this.appRef.attachView(hostRef.hostView);

    return hostRef;
  }

  private destroyHost(hostRef: ComponentRef<CqDialogHost>) {
    this.appRef.detachView(hostRef.hostView);
    hostRef.destroy();
    if (document.body.contains(hostRef.location.nativeElement)) {
      document.body.removeChild(hostRef.location.nativeElement);
    }
  }
}
