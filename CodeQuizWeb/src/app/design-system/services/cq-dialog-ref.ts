import { Subject } from 'rxjs';

/**
 * A reference to an open dialog. Use this to close the dialog
 * or subscribe to its result from the calling code.
 */
export class CqDialogRef<R = any> {
  private _afterClosed = new Subject<R | undefined>();
  afterClosed$ = this._afterClosed.asObservable();

  /** @internal called by the service/host when the dialog closes */
  _close(result?: R) {
    this._afterClosed.next(result);
    this._afterClosed.complete();
  }

  /** Programmatically close the dialog with an optional result */
  close(result?: R) {
    this._close(result);
  }
}
