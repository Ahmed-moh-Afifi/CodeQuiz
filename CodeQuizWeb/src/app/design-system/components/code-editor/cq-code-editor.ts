import {
  Component,
  input,
  output,
  signal,
  effect,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  NgZone,
  inject,
  ChangeDetectorRef,
} from '@angular/core';
import { CqIcon } from '../icon/cq-icon';
import { CqButton } from '../buttons/cq-button/cq-button';
import { FormsModule } from '@angular/forms';

declare const monaco: any;

let monacoLoaded = false;
let monacoLoadPromise: Promise<void> | null = null;

function loadMonaco(): Promise<void> {
  if (monacoLoaded) return Promise.resolve();
  if (monacoLoadPromise) return monacoLoadPromise;

  monacoLoadPromise = new Promise<void>((resolve, reject) => {
    const baseUrl = (document.querySelector('base')?.href ?? '/') + 'assets/monaco';
    const script = document.createElement('script');
    script.src = `${baseUrl}/vs/loader.js`;
    script.onload = () => {
      (window as any).require.config({ paths: { vs: `${baseUrl}/vs` } });
      (window as any).require(['vs/editor/editor.main'], () => {
        monacoLoaded = true;
        resolve();
      });
    };
    script.onerror = () => reject(new Error('Failed to load Monaco Editor'));
    document.head.appendChild(script);
  });

  return monacoLoadPromise;
}

const LANGUAGE_MAP: Record<string, string> = {
  'c#': 'csharp',
  cs: 'csharp',
  csharp: 'csharp',
  'c++': 'cpp',
  cpp: 'cpp',
  python: 'python',
  py: 'python',
  javascript: 'javascript',
  js: 'javascript',
  typescript: 'typescript',
  ts: 'typescript',
  java: 'java',
};

const EXT_MAP: Record<string, string> = {
  csharp: 'cs',
  python: 'py',
  javascript: 'js',
  typescript: 'ts',
  java: 'java',
  cpp: 'cpp',
};

@Component({
  selector: 'cq-code-editor',
  standalone: true,
  imports: [CqIcon, CqButton, FormsModule],
  templateUrl: './cq-code-editor.html',
  styleUrl: './cq-code-editor.scss',
})
export class CqCodeEditor implements AfterViewInit, OnDestroy {
  private readonly zone = inject(NgZone);
  private readonly cdr = inject(ChangeDetectorRef);

  // Inputs
  language = input<string>('python');
  code = input<string>('');
  initialCode = input<string | null>(null);
  readOnly = input<boolean>(false);
  showRunButton = input<boolean>(true);
  showIO = input<boolean>(true);
  showOutput = input<boolean>(true);
  showError = input<boolean>(true);
  fileName = input<string | null>(null);

  // Outputs
  codeChange = output<string>();
  runRequested = output<{ code: string; input: string }>();
  codeChanged = output<string>(); // debounced

  // Internal state
  editorInput = signal('');
  editorOutput = signal('');
  isRunning = signal(false);
  private editorReady = signal(false);

  @ViewChild('editorContainer', { static: false }) editorContainer!: ElementRef<HTMLDivElement>;
  private editor: any = null;
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  get fileLabel(): string {
    if (this.fileName()) return this.fileName()!;
    const lang = this.resolvedLanguage;
    const ext = EXT_MAP[lang] ?? lang;
    return `Solution.${ext}`;
  }

  get resolvedLanguage(): string {
    return LANGUAGE_MAP[this.language().toLowerCase()] ?? this.language().toLowerCase();
  }

  constructor() {
    // React to code input changes
    effect(() => {
      const newCode = this.code();
      if (this.editor && newCode !== this.editor.getValue()) {
        this.editor.setValue(newCode);
      }
    });

    // React to language changes
    effect(() => {
      const lang = this.language();
      if (this.editor && monacoLoaded) {
        const model = this.editor.getModel();
        if (model) {
          monaco.editor.setModelLanguage(model, this.resolvedLanguage);
        }
      }
    });

    // React to readOnly changes
    effect(() => {
      const ro = this.readOnly();
      if (this.editor) {
        this.editor.updateOptions({ readOnly: ro });
      }
    });
  }

  async ngAfterViewInit() {
    await loadMonaco();
    this.zone.runOutsideAngular(() => {
      this.createEditor();
    });
  }

  ngOnDestroy() {
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    if (this.editor) {
      this.editor.dispose();
      this.editor = null;
    }
  }

  private createEditor() {
    if (!this.editorContainer) return;

    this.editor = monaco.editor.create(this.editorContainer.nativeElement, {
      value: this.code(),
      language: this.resolvedLanguage,
      theme: 'vs-dark',
      readOnly: this.readOnly(),
      automaticLayout: true,
      minimap: { enabled: false },
      fontSize: 14,
      fontFamily: "'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace",
      lineNumbers: 'on',
      scrollBeyondLastLine: false,
      renderLineHighlight: 'line',
      padding: { top: 12 },
      scrollbar: {
        verticalScrollbarSize: 8,
        horizontalScrollbarSize: 8,
      },
      overviewRulerBorder: false,
      cursorBlinking: 'smooth',
      tabSize: 4,
    });

    this.editor.onDidChangeModelContent(() => {
      const value = this.editor.getValue();
      this.zone.run(() => {
        this.codeChange.emit(value);
      });

      // Debounced change
      if (this.debounceTimer) clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => {
        this.zone.run(() => {
          this.codeChanged.emit(value);
        });
      }, 2000);
    });

    this.zone.run(() => {
      this.editorReady.set(true);
    });
  }

  resetCode() {
    const initial = this.initialCode() ?? '';
    if (this.editor) {
      this.editor.setValue(initial);
    }
    this.codeChange.emit(initial);
  }

  onRun() {
    if (this.isRunning()) return;
    const code = this.editor ? this.editor.getValue() : this.code();
    this.runRequested.emit({ code, input: this.editorInput() });
  }

  /** Called by parent to set output text */
  setOutput(text: string) {
    this.editorOutput.set(text);
  }

  /** Called by parent to set running state */
  setRunning(running: boolean) {
    this.isRunning.set(running);
  }

  /** Get current code value */
  getValue(): string {
    return this.editor ? this.editor.getValue() : this.code();
  }
}
