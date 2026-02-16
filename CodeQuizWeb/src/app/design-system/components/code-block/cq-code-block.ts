import {
  Component,
  input,
  ElementRef,
  AfterViewInit,
  OnChanges,
  ViewEncapsulation,
  viewChild,
} from '@angular/core';
import hljs from 'highlight.js/lib/core';
import xml from 'highlight.js/lib/languages/xml';
import typescript from 'highlight.js/lib/languages/typescript';
import scss from 'highlight.js/lib/languages/scss';
import css from 'highlight.js/lib/languages/css';

// Register languages
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('html', xml);
hljs.registerLanguage('typescript', typescript);
hljs.registerLanguage('scss', scss);
hljs.registerLanguage('css', css);

@Component({
  selector: 'cq-code-block',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  template: `<pre
    class="cq-code-block-pre"
  ><code #codeEl class="cq-code-block-code" [class]="'language-' + language()">{{ code() }}</code></pre>`,
  styles: [
    `
      .cq-code-block-pre {
        margin: 0;
        padding: 16px;
        border-radius: var(--cq-radius-lg, 12px);
        background-color: var(--cq-code-bg, #1a1a2e);
        overflow-x: auto;
        font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
        font-size: 13px;
        line-height: 1.6;
        border: 1px solid var(--cq-border-default, #2a2a3a);
      }

      .cq-code-block-code {
        font-family: inherit;
        font-size: inherit;
      }

      /* One Dark inspired theme for highlight.js */
      .hljs {
        color: #abb2bf;
        background: transparent;
      }

      .hljs-keyword,
      .hljs-selector-tag,
      .hljs-title,
      .hljs-section,
      .hljs-doctag,
      .hljs-name,
      .hljs-strong {
        color: #c678dd;
        font-weight: normal;
      }

      .hljs-string,
      .hljs-addition {
        color: #98c379;
      }

      .hljs-comment,
      .hljs-quote,
      .hljs-deletion {
        color: #5c6370;
        font-style: italic;
      }

      .hljs-number,
      .hljs-literal {
        color: #d19a66;
      }

      .hljs-attr,
      .hljs-variable,
      .hljs-template-variable,
      .hljs-tag .hljs-attr {
        color: #d19a66;
      }

      .hljs-type,
      .hljs-built_in,
      .hljs-builtin-name,
      .hljs-params {
        color: #e5c07b;
      }

      .hljs-symbol,
      .hljs-bullet,
      .hljs-link,
      .hljs-meta,
      .hljs-selector-id,
      .hljs-title.class_ {
        color: #61afef;
      }

      .hljs-tag {
        color: #e06c75;
      }

      .hljs-tag .hljs-name {
        color: #e06c75;
      }

      .hljs-attribute {
        color: #d19a66;
      }

      .hljs-selector-class {
        color: #e06c75;
      }

      .hljs-selector-pseudo {
        color: #56b6c2;
      }

      .hljs-property {
        color: #56b6c2;
      }

      /* Light theme overrides */
      [data-theme='light'] .cq-code-block-pre {
        background-color: #f6f8fa;
        border-color: var(--cq-border-default, #d0d7de);
      }

      [data-theme='light'] .hljs {
        color: #24292f;
      }

      [data-theme='light'] .hljs-keyword,
      [data-theme='light'] .hljs-name,
      [data-theme='light'] .hljs-selector-tag {
        color: #cf222e;
      }

      [data-theme='light'] .hljs-string,
      [data-theme='light'] .hljs-addition {
        color: #0a3069;
      }

      [data-theme='light'] .hljs-comment,
      [data-theme='light'] .hljs-quote {
        color: #6e7781;
      }

      [data-theme='light'] .hljs-number,
      [data-theme='light'] .hljs-literal {
        color: #0550ae;
      }

      [data-theme='light'] .hljs-attr,
      [data-theme='light'] .hljs-attribute {
        color: #0550ae;
      }

      [data-theme='light'] .hljs-type,
      [data-theme='light'] .hljs-built_in {
        color: #953800;
      }

      [data-theme='light'] .hljs-tag {
        color: #116329;
      }

      [data-theme='light'] .hljs-tag .hljs-name {
        color: #116329;
      }

      [data-theme='light'] .hljs-property {
        color: #0550ae;
      }
    `,
  ],
})
export class CqCodeBlock implements AfterViewInit, OnChanges {
  code = input.required<string>();
  language = input<string>('html');

  private codeEl = viewChild.required<ElementRef>('codeEl');

  ngAfterViewInit() {
    this.highlight();
  }

  ngOnChanges() {
    // Re-highlight when code or language changes
    setTimeout(() => this.highlight());
  }

  private highlight() {
    const el = this.codeEl()?.nativeElement;
    if (el) {
      // Reset hljs state so it re-highlights
      delete (el as any).dataset.highlighted;
      hljs.highlightElement(el);
    }
  }
}
