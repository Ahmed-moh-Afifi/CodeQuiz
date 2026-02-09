import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface CodeSnippet {
  label: string;
  html?: string;
  css?: string;
}

@Component({
  selector: 'app-design-system-demo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './design-system-demo.html',
  styleUrl: './design-system-demo.scss',
})
export class DesignSystemDemoComponent {
  // Theme state
  currentTheme = signal<'dark' | 'light'>('dark');

  // Sample data for table demo
  tableData = [
    { id: 1, name: 'Quiz Alpha', status: 'Active', score: 85 },
    { id: 2, name: 'Quiz Beta', status: 'Completed', score: 92 },
    { id: 3, name: 'Quiz Gamma', status: 'Pending', score: 0 },
  ];

  // Dialog visibility states
  showConfirmDialog = false;
  showSuccessDialog = false;
  showWarningDialog = false;
  showInfoDialog = false;
  showFormDialog = false;
  showQuestionDialog = false;

  // Loading states
  isLoading1 = false;
  isLoading2 = false;

  // Active code tooltip
  activeCodeTooltip: string | null = null;

  // Background & Surface color values
  backgroundColor = '';
  surfaceColor = '';

  // Font size values
  fontSizes: {
    name: string;
    variable: string;
    value: number;
    default: number;
    min: number;
    max: number;
  }[] = [
    {
      name: 'Page Title',
      variable: '--cq-font-size-page-title',
      value: 32,
      default: 32,
      min: 20,
      max: 56,
    },
    {
      name: 'Section Header',
      variable: '--cq-font-size-section-header',
      value: 22,
      default: 22,
      min: 14,
      max: 40,
    },
    {
      name: 'Card Title',
      variable: '--cq-font-size-card-title',
      value: 40,
      default: 40,
      min: 24,
      max: 64,
    },
    {
      name: 'Item Title',
      variable: '--cq-font-size-item-title',
      value: 17,
      default: 17,
      min: 12,
      max: 28,
    },
    { name: 'Body', variable: '--cq-font-size-body', value: 15, default: 15, min: 10, max: 24 },
    {
      name: 'Caption',
      variable: '--cq-font-size-caption',
      value: 14,
      default: 14,
      min: 10,
      max: 22,
    },
    { name: 'Code', variable: '--cq-font-size-code', value: 14, default: 14, min: 10, max: 22 },
    { name: 'Small', variable: '--cq-font-size-small', value: 13, default: 13, min: 9, max: 20 },
  ];

  // Color groups for the demo
  colorGroups = [
    {
      name: 'Primary Palette',
      colors: [
        { name: 'Primary', variable: '--cq-primary', value: '' },
        { name: 'Primary Hover', variable: '--cq-primary-hover', value: '' },
        { name: 'Primary Pressed', variable: '--cq-primary-pressed', value: '' },
      ],
    },
    {
      name: 'Status Colors',
      colors: [
        { name: 'Success', variable: '--cq-success', value: '' },
        { name: 'Error', variable: '--cq-error', value: '' },
        { name: 'Warning', variable: '--cq-warning', value: '' },
        { name: 'Info', variable: '--cq-info', value: '' },
      ],
    },
  ];

  // ========== Code snippets for each section ==========
  codeSnippets: Record<string, CodeSnippet[]> = {
    colors: [
      {
        label: 'Override a CSS variable',
        html: `<!-- Override via inline style or :root -->
<div style="--cq-primary: #3b82f6;">
  Content with custom primary
</div>`,
        css: `/* Override globally in your stylesheet */
:root {
  --cq-primary: #3b82f6;
  --cq-primary-hover: #2563eb;
}`,
      },
      {
        label: 'Background & Surface',
        html: `<!-- These control page and card backgrounds -->`,
        css: `:root {
  --cq-background-dark: #0f0f1a;
  --cq-surface-dark: #1a1a2e;
  --cq-elevated-surface: #252545;
}`,
      },
    ],
    typography: [
      {
        label: 'Page Title',
        html: `<h1 class="cq-page-title">Page Title</h1>`,
        css: `.cq-page-title {
  font-size: var(--cq-font-size-page-title); /* 32px */
  font-weight: 700;
  color: var(--cq-text-primary);
}`,
      },
      {
        label: 'Section Header',
        html: `<h2 class="cq-section-header">Section Header</h2>`,
      },
      {
        label: 'Body & Caption Text',
        html: `<p class="cq-body-text">Body text content</p>
<p class="cq-caption-text">Caption text</p>
<p class="cq-code-label">code_label</p>`,
      },
      {
        label: 'Font Size Variables',
        css: `/* All font sizes are customizable via CSS variables */
:root {
  --cq-font-size-page-title: 32px;
  --cq-font-size-section-header: 22px;
  --cq-font-size-card-title: 40px;
  --cq-font-size-item-title: 17px;
  --cq-font-size-body: 15px;
  --cq-font-size-caption: 14px;
  --cq-font-size-code: 14px;
  --cq-font-size-small: 13px;
}`,
      },
    ],
    buttons: [
      {
        label: 'Button Variants',
        html: `<button class="cq-btn-primary">Primary</button>
<button class="cq-btn-secondary">Secondary</button>
<button class="cq-btn-outlined">Outlined</button>
<button class="cq-btn-danger">Danger</button>
<button class="cq-btn-ghost">Ghost</button>
<button class="cq-btn-success">Success</button>`,
      },
      {
        label: 'Loading Button',
        html: `<!-- Loading with text -->
<button class="cq-btn-primary cq-btn-loading">
  <span class="cq-btn-spinner"></span>
  <span>Saving...</span>
</button>

<!-- Spinner replaces text -->
<button class="cq-btn-secondary cq-btn-loading cq-btn-loading-replace">
  <span class="cq-btn-spinner cq-btn-spinner-center"></span>
  <span class="cq-btn-text">Submit</span>
</button>`,
      },
      {
        label: 'Icon Buttons',
        html: `<button class="cq-btn-icon">
  <div class="cq-mask-icon"
    style="--mask-url: url('assets/icons/save.svg');
           background-color: var(--cq-text-secondary);
           width: 18px; height: 18px;">
  </div>
</button>
<button class="cq-btn-icon-primary">...</button>
<button class="cq-btn-icon-outlined">...</button>
<button class="cq-btn-icon-danger">...</button>`,
      },
    ],
    cards: [
      {
        label: 'Card Types',
        html: `<!-- Standard Card -->
<div class="cq-card">
  <h3 class="cq-item-title">Title</h3>
  <p class="cq-body-text">Content</p>
</div>

<!-- Elevated Card (with shadow) -->
<div class="cq-card-elevated">...</div>

<!-- Highlighted Card (primary border) -->
<div class="cq-card-highlighted">...</div>

<!-- Nested Inner Card -->
<div class="cq-card">
  <div class="cq-card-inner">Nested</div>
</div>`,
        css: `.cq-card {
  background-color: var(--cq-surface-dark);
  border: 1px solid var(--cq-border-default);
  border-radius: var(--cq-radius-xl);
  padding: var(--cq-spacing-lg);
}`,
      },
    ],
    inputs: [
      {
        label: 'Text Input',
        html: `<div class="cq-form-group">
  <label class="cq-label">Label</label>
  <div class="cq-input-wrapper">
    <input type="text" class="cq-input"
           placeholder="Enter text..." />
  </div>
</div>`,
      },
      {
        label: 'Select, Textarea, Toggles',
        html: `<!-- Select -->
<div class="cq-select-wrapper">
  <select class="cq-select">
    <option>Option 1</option>
  </select>
</div>

<!-- Textarea -->
<div class="cq-textarea-wrapper">
  <textarea class="cq-textarea" rows="3"></textarea>
</div>

<!-- Switch & Checkbox -->
<input type="checkbox" class="cq-switch" checked />
<input type="checkbox" class="cq-checkbox" checked />`,
      },
    ],
    badges: [
      {
        label: 'Badge Variants',
        html: `<span class="cq-badge-status">Status</span>
<span class="cq-badge-live">Live</span>
<span class="cq-badge-success">Success</span>
<span class="cq-badge-warning">Warning</span>
<span class="cq-badge-error">Error</span>
<span class="cq-badge-info">Info</span>
<button class="cq-badge-action">Action</button>

<!-- Small variant -->
<span class="cq-badge-success cq-badge-sm">v1.0</span>

<!-- Status dots -->
<span class="cq-badge-dot cq-badge-dot-success"></span>`,
      },
    ],
    table: [
      {
        label: 'Table',
        html: `<div class="cq-table-container">
  <table class="cq-table">
    <thead>
      <tr>
        <th>ID</th>
        <th>Name</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="cq-table-cell-muted">1</td>
        <td>Item</td>
      </tr>
    </tbody>
  </table>
</div>`,
      },
    ],
    dialogs: [
      {
        label: 'Alert / Confirm Dialog',
        html: `<!-- Overlay + Dialog -->
<div class="cq-dialog-overlay">
  <div class="cq-dialog cq-dialog-sm">
    <div class="cq-confirm-dialog">
      <div class="cq-confirm-dialog-content">
        <div class="cq-dialog-icon cq-dialog-icon-danger">
          <div class="cq-mask-icon cq-mask-icon-error"
            style="--mask-url: url('assets/icons/question.svg')">
          </div>
        </div>
        <h3 class="cq-alert-dialog-title">Title</h3>
        <p class="cq-alert-dialog-message">Message</p>
        <div class="cq-alert-dialog-actions">
          <button class="cq-btn-secondary">Cancel</button>
          <button class="cq-btn-danger-filled">Delete</button>
        </div>
      </div>
    </div>
  </div>
</div>`,
      },
      {
        label: 'Form Dialog',
        html: `<div class="cq-dialog-overlay">
  <div class="cq-dialog">
    <div class="cq-dialog-header">
      <h3 class="cq-dialog-title">Title</h3>
      <button class="cq-dialog-close">×</button>
    </div>
    <div class="cq-dialog-content">
      <!-- form fields -->
    </div>
    <div class="cq-dialog-footer">
      <button class="cq-btn-secondary">Cancel</button>
      <button class="cq-btn-primary">Submit</button>
    </div>
  </div>
</div>`,
      },
    ],
    messages: [
      {
        label: 'Message Variants',
        html: `<div class="cq-message cq-message-info">
  <div class="cq-message-icon">
    <div class="cq-mask-icon cq-mask-icon-info"
      style="--mask-url: url('assets/icons/lightbulb.svg')">
    </div>
  </div>
  <div class="cq-message-content">
    <h4 class="cq-message-title">Info</h4>
    <p class="cq-message-text">Message text</p>
  </div>
</div>

<!-- Also: cq-message-warning, cq-message-error -->`,
      },
    ],
    layout: [
      {
        label: 'Page Header',
        html: `<div class="cq-page-header">
  <button class="cq-page-header-back">
    <img src="assets/icons/arrow_back_ios_new_rounded.svg" />
  </button>
  <div class="cq-page-header-content">
    <h1 class="cq-page-header-title">Title</h1>
    <p class="cq-page-header-subtitle">Subtitle</p>
  </div>
  <div class="cq-page-header-actions">
    <button class="cq-btn-primary">Action</button>
  </div>
</div>`,
      },
    ],
    utilities: [
      {
        label: 'Progress, Spinners, Avatars',
        html: `<!-- Progress Bar -->
<div class="cq-progress">
  <div class="cq-progress-bar" style="width: 75%"></div>
</div>
<!-- Variants: cq-progress-success, cq-progress-warning, cq-progress-error -->

<!-- Spinner -->
<div class="cq-spinner"></div>
<!-- Sizes: cq-spinner-sm, cq-spinner-lg -->

<!-- Avatar -->
<div class="cq-avatar">JD</div>
<!-- Sizes: cq-avatar-sm, cq-avatar-lg -->`,
      },
      {
        label: 'Mask Icons, Tooltips, Skeleton',
        html: `<!-- Mask Icon (dynamic color) -->
<div class="cq-mask-icon cq-mask-icon-primary"
  style="--mask-url: url('assets/icons/save.svg');
         width: 20px; height: 20px;">
</div>

<!-- Tooltip -->
<button class="cq-tooltip" data-tooltip="Tooltip text">
  Hover me
</button>

<!-- Skeleton Loading -->
<div class="cq-skeleton cq-skeleton-circle"
  style="width: 48px; height: 48px"></div>
<div class="cq-skeleton-text" style="width: 60%"></div>`,
      },
    ],
  };

  constructor() {
    // Check for saved theme preference
    const savedTheme = localStorage.getItem('cq-theme') as 'dark' | 'light';
    if (savedTheme) {
      this.currentTheme.set(savedTheme);
      document.documentElement.setAttribute('data-theme', savedTheme);
    }

    // Initialize color values after a brief delay to ensure styles are applied
    setTimeout(() => this.initializeColors(), 0);
    setTimeout(() => this.initializeBackgroundSurface(), 0);
  }

  initializeColors() {
    const computedStyle = getComputedStyle(document.documentElement);

    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        const value = computedStyle.getPropertyValue(color.variable).trim();
        color.value = this.rgbToHex(value) || value;
      });
    });
  }

  initializeBackgroundSurface() {
    const computedStyle = getComputedStyle(document.documentElement);
    const bg = computedStyle.getPropertyValue('--cq-background-dark').trim();
    const sf = computedStyle.getPropertyValue('--cq-surface-dark').trim();
    this.backgroundColor = this.rgbToHex(bg) || bg;
    this.surfaceColor = this.rgbToHex(sf) || sf;
  }

  updateColor(variable: string, event: Event) {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    document.documentElement.style.setProperty(variable, value);

    // Update local model
    this.colorGroups.forEach((group) => {
      const color = group.colors.find((c) => c.variable === variable);
      if (color) color.value = value;
    });
  }

  updateBackground(event: Event) {
    const input = event.target as HTMLInputElement;
    this.backgroundColor = input.value;
    document.documentElement.style.setProperty('--cq-background-dark', input.value);
  }

  updateSurface(event: Event) {
    const input = event.target as HTMLInputElement;
    this.surfaceColor = input.value;
    document.documentElement.style.setProperty('--cq-surface-dark', input.value);
    document.documentElement.style.setProperty('--cq-card-background', input.value);
  }

  updateFontSize(fontSize: (typeof this.fontSizes)[0], event: Event) {
    const input = event.target as HTMLInputElement;
    fontSize.value = parseInt(input.value);
    document.documentElement.style.setProperty(fontSize.variable, fontSize.value + 'px');
  }

  resetFontSizes() {
    this.fontSizes.forEach((fs) => {
      fs.value = fs.default;
      document.documentElement.style.removeProperty(fs.variable);
    });
  }

  resetBackgroundSurface() {
    document.documentElement.style.removeProperty('--cq-background-dark');
    document.documentElement.style.removeProperty('--cq-surface-dark');
    document.documentElement.style.removeProperty('--cq-card-background');
    setTimeout(() => this.initializeBackgroundSurface(), 50);
  }

  resetAllColors() {
    // Reset palette colors
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        document.documentElement.style.removeProperty(color.variable);
      });
    });
    // Reset background & surface
    document.documentElement.style.removeProperty('--cq-background-dark');
    document.documentElement.style.removeProperty('--cq-surface-dark');
    document.documentElement.style.removeProperty('--cq-card-background');
    // Re-initialize all color values
    setTimeout(() => {
      this.initializeColors();
      this.initializeBackgroundSurface();
    }, 50);
  }

  toggleCodeTooltip(sectionId: string) {
    this.activeCodeTooltip = this.activeCodeTooltip === sectionId ? null : sectionId;
  }

  closeCodeTooltip() {
    this.activeCodeTooltip = null;
  }

  // Helper to convert RGB/RGBA to Hex
  private rgbToHex(rgb: string): string {
    // If it's already hex, return it
    if (rgb.startsWith('#')) return rgb;

    // Parse rgb(r, g, b)
    const result = rgb.match(/\d+/g);
    if (!result || result.length < 3) return '';

    const r = parseInt(result[0]);
    const g = parseInt(result[1]);
    const b = parseInt(result[2]);

    return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
  }

  toggleTheme() {
    const newTheme = this.currentTheme() === 'dark' ? 'light' : 'dark';
    this.currentTheme.set(newTheme);
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('cq-theme', newTheme);

    // Remove inline overrides for the demo constants
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        document.documentElement.style.removeProperty(color.variable);
      });
    });

    // Remove background/surface/font overrides
    document.documentElement.style.removeProperty('--cq-background-dark');
    document.documentElement.style.removeProperty('--cq-surface-dark');
    document.documentElement.style.removeProperty('--cq-card-background');
    this.fontSizes.forEach((fs) => {
      fs.value = fs.default;
      document.documentElement.style.removeProperty(fs.variable);
    });

    // Re-initialize values
    setTimeout(() => {
      this.initializeColors();
      this.initializeBackgroundSurface();
    }, 50);
  }

  // Simulate loading
  simulateLoading1() {
    this.isLoading1 = true;
    setTimeout(() => {
      this.isLoading1 = false;
    }, 2000);
  }

  simulateLoading2() {
    this.isLoading2 = true;
    setTimeout(() => {
      this.isLoading2 = false;
    }, 2000);
  }

  // Dialog methods
  openConfirmDialog() {
    this.showConfirmDialog = true;
  }
  closeConfirmDialog() {
    this.showConfirmDialog = false;
  }

  openQuestionDialog() {
    this.showQuestionDialog = true;
  }

  closeQuestionDialog() {
    this.showQuestionDialog = false;
  }

  openSuccessDialog() {
    this.showSuccessDialog = true;
  }
  closeSuccessDialog() {
    this.showSuccessDialog = false;
  }

  openWarningDialog() {
    this.showWarningDialog = true;
  }
  closeWarningDialog() {
    this.showWarningDialog = false;
  }

  openInfoDialog() {
    this.showInfoDialog = true;
  }
  closeInfoDialog() {
    this.showInfoDialog = false;
  }

  openFormDialog() {
    this.showFormDialog = true;
  }
  closeFormDialog() {
    this.showFormDialog = false;
  }
}
