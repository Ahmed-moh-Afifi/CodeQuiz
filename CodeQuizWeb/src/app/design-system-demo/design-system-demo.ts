import { Component, signal, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CqButton,
  CqCard,
  CqInput,
  CqTextarea,
  CqSelect,
  CqSwitch,
  CqCheckbox,
  CqBadge,
  CqMessage,
  CqTabView,
  CqTab,
  CqPage,
  CqCodeBlock,
  CqDialogService,
  CqToastService,
  CqTooltipDirective,
  CqAvatar,
  CqProgressBar,
  CqDropdown,
  CqDropdownItem,
  CqDropdownDivider,
} from '../design-system';
import { CreateQuizDialog } from './dialogs/create-quiz-dialog';

interface CodeSnippet {
  label: string;
  html?: string;
  css?: string;
  language?: string; // defaults to 'html' if not specified
}

interface ColorConfig {
  name: string;
  variable: string;
  value: string;
  description: string;
}

interface ColorGroup {
  name: string;
  colors: ColorConfig[];
}

@Component({
  selector: 'app-design-system-demo',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CqButton,
    CqCard,
    CqInput,
    CqTextarea,
    CqSelect,
    CqSwitch,
    CqCheckbox,
    CqBadge,
    CqMessage,
    CqTabView,
    CqTab,
    CqPage,
    CqCodeBlock,
    CqTooltipDirective,
    CqAvatar,
    CqProgressBar,
    CqDropdown,
    CqDropdownItem,
    CqDropdownDivider,
  ],
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

  // Color config dialog (kept inline since it modifies the page)
  showColorConfigDialog = false;

  // Last dialog result for demo
  lastDialogResult = signal<string>('');

  // Loading states
  isLoading1 = false;
  isLoading2 = false;

  // Active code tooltip
  activeCodeTooltip: string | null = null;

  // Component demo form values
  demoInputValue = '';
  demoTextareaValue = '';
  demoSelectValue = '1';
  demoSwitchValue = true;
  demoCheckboxValue = false;

  // Progress bar demo
  demoProgress = signal(65);

  demoSelectOptions = [
    { value: '1', label: 'Option 1 — Dark Background' },
    { value: '2', label: 'Option 2 — Visible Text' },
    { value: '3', label: 'Option 3 — Proper Contrast' },
  ];

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

  // All color configuration with descriptions
  colorGroups: ColorGroup[] = [
    {
      name: 'Background & Surface',
      colors: [
        {
          name: 'Background',
          variable: '--cq-background-dark',
          value: '',
          description: 'Main page background color. Affects the overall tone of the app.',
        },
        {
          name: 'Surface / Cards',
          variable: '--cq-surface-dark',
          value: '',
          description:
            'Card backgrounds and elevated surfaces. Should be slightly lighter than background.',
        },
        {
          name: 'Elevated Surface',
          variable: '--cq-elevated-surface',
          value: '',
          description: 'Popovers, dropdowns, and elevated UI. Even lighter than surface.',
        },
      ],
    },
    {
      name: 'Primary Palette',
      colors: [
        {
          name: 'Primary',
          variable: '--cq-primary',
          value: '',
          description:
            'Main brand color. Used for primary buttons, active states, links, and accents.',
        },
        {
          name: 'Primary Hover',
          variable: '--cq-primary-hover',
          value: '',
          description: 'Hover state for primary-colored elements. Slightly lighter than primary.',
        },
        {
          name: 'Primary Pressed',
          variable: '--cq-primary-pressed',
          value: '',
          description: 'Active/pressed state for primary elements. Slightly darker than primary.',
        },
      ],
    },
    {
      name: 'Status Colors',
      colors: [
        {
          name: 'Success',
          variable: '--cq-success',
          value: '',
          description:
            'Success badges, success messages, completion indicators, and positive score cells.',
        },
        {
          name: 'Error',
          variable: '--cq-error',
          value: '',
          description:
            'Error messages, danger buttons, destructive dialog icons, and error badges.',
        },
        {
          name: 'Warning',
          variable: '--cq-warning',
          value: '',
          description:
            'Warning messages, warning badges, caution dialog icons, and moderate-score cells.',
        },
        {
          name: 'Info',
          variable: '--cq-info',
          value: '',
          description:
            'Info messages, info badges, live-status indicators, and informational dialogs.',
        },
      ],
    },
    {
      name: 'Text Colors',
      colors: [
        {
          name: 'Text Primary',
          variable: '--cq-text-primary',
          value: '',
          description: 'Main text color for headings, body text, and table cells.',
        },
        {
          name: 'Text Secondary',
          variable: '--cq-text-secondary',
          value: '',
          description: 'Secondary text — labels, subtitles, and less prominent content.',
        },
        {
          name: 'Text Muted',
          variable: '--cq-text-muted',
          value: '',
          description: 'Muted text for captions, placeholders, and helper text.',
        },
      ],
    },
    {
      name: 'Border Colors',
      colors: [
        {
          name: 'Border Default',
          variable: '--cq-border-default',
          value: '',
          description: 'Standard border color for cards, inputs, table rows, and dividers.',
        },
        {
          name: 'Border Subtle',
          variable: '--cq-border-subtle',
          value: '',
          description: 'Subtle border for checkboxes, outlined buttons, and secondary borders.',
        },
      ],
    },
    {
      name: 'Secondary Palette',
      colors: [
        {
          name: 'Secondary',
          variable: '--cq-secondary',
          value: '',
          description:
            'Secondary button background, switch track, badge-status background, and tab headers.',
        },
        {
          name: 'Secondary Hover',
          variable: '--cq-secondary-hover',
          value: '',
          description: 'Hover state for secondary-colored elements.',
        },
      ],
    },
    {
      name: 'Typography Colors',
      colors: [
        {
          name: 'Text on Primary',
          variable: '--cq-text-on-primary',
          value: '',
          description: 'Text color on primary-colored backgrounds (buttons, badges, etc.).',
        },
        {
          name: 'Code Text',
          variable: '--cq-code-color',
          value: '',
          description: 'Color for inline code and code blocks.',
        },
        {
          name: 'Link',
          variable: '--cq-link-color',
          value: '',
          description: 'Hyperlink text color. Usually matches or derives from primary.',
        },
        {
          name: 'Heading',
          variable: '--cq-heading-color',
          value: '',
          description: 'Color for page titles and section headers. Usually same as text-primary.',
        },
      ],
    },
  ];

  // ========== Code snippets for each section ==========
  codeSnippets: Record<string, { css: CodeSnippet[]; component: CodeSnippet[] }> = {
    buttons: {
      css: [
        {
          label: 'All Button Variants',
          html: `<button class="cq-btn-primary">Primary</button>
<button class="cq-btn-secondary">Secondary</button>
<button class="cq-btn-outlined">Outlined</button>
<button class="cq-btn-danger">Danger</button>
<button class="cq-btn-ghost">Ghost</button>
<button class="cq-btn-success">Success</button>
<button class="cq-btn-danger-filled">Danger Filled</button>
<button class="cq-btn-warning">Warning</button>
<button class="cq-add-item-button">+ Add Item</button>`,
        },
        {
          label: 'Loading & Disabled',
          html: `<button class="cq-btn-primary" disabled>Disabled</button>
<button class="cq-btn-primary cq-btn-loading">
  <span class="cq-btn-spinner"></span>
  <span>Saving...</span>
</button>`,
        },
        {
          label: 'Icon Buttons',
          html: `<button class="cq-btn-icon">...</button>
<button class="cq-btn-icon-primary">...</button>
<button class="cq-btn-icon-outlined">...</button>
<button class="cq-btn-icon-danger">...</button>`,
        },
      ],
      component: [
        {
          label: 'All Button Variants',
          html: `<cq-button type="primary">Primary</cq-button>
<cq-button type="secondary">Secondary</cq-button>
<cq-button type="outlined">Outlined</cq-button>
<cq-button type="danger">Danger</cq-button>
<cq-button type="ghost">Ghost</cq-button>
<cq-button type="success">Success</cq-button>
<cq-button type="danger-filled">Danger Filled</cq-button>
<cq-button type="warning">Warning</cq-button>
<cq-button type="add-item">+ Add Item</cq-button>`,
        },
        {
          label: 'Loading & Disabled',
          html: `<cq-button type="primary" [disabled]="true">
  Disabled
</cq-button>
<cq-button type="primary"
  [loading]="isLoading"
  loadingText="Saving...">
  Save Changes
</cq-button>`,
        },
      ],
    },
    cards: {
      css: [
        {
          label: 'Card Types',
          html: `<div class="cq-card">
  <h3 class="cq-item-title">Title</h3>
  <p class="cq-body-text">Content</p>
</div>
<div class="cq-card-elevated">...</div>
<div class="cq-card-highlighted">...</div>`,
        },
      ],
      component: [
        {
          label: 'Card Types',
          html: `<cq-card type="standard">
  <span slot="card-title">Title</span>
  Content
</cq-card>
<cq-card type="elevated">...</cq-card>
<cq-card type="highlighted">...</cq-card>`,
        },
      ],
    },
    inputs: {
      css: [
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

<!-- Switch & Checkbox -->
<input type="checkbox" class="cq-switch" checked />
<input type="checkbox" class="cq-checkbox" checked />`,
        },
      ],
      component: [
        {
          label: 'Text Input',
          html: `<cq-input
  label="Text Input"
  placeholder="Enter text..."
  [(ngModel)]="value">
</cq-input>`,
        },
        {
          label: 'Select, Textarea, Toggles',
          html: `<cq-select
  label="Select"
  [options]="options"
  [(ngModel)]="selectValue">
</cq-select>

<cq-textarea
  label="Textarea"
  placeholder="Enter..."
  [(ngModel)]="textValue">
</cq-textarea>

<cq-switch label="Toggle" [(ngModel)]="on" />
<cq-checkbox label="Check" [(ngModel)]="ok" />`,
        },
      ],
    },
    badges: {
      css: [
        {
          label: 'Badge Variants',
          html: `<span class="cq-badge-status">Status</span>
<span class="cq-badge-live">Live</span>
<span class="cq-badge-success">Success</span>
<span class="cq-badge-warning">Warning</span>
<span class="cq-badge-error">Error</span>
<span class="cq-badge-info">Info</span>
<button class="cq-badge-action">Action</button>
<span class="cq-badge-success cq-badge-sm">v1.0</span>`,
        },
      ],
      component: [
        {
          label: 'Badge Variants',
          html: `<cq-badge type="status">Status</cq-badge>
<cq-badge type="live">Live</cq-badge>
<cq-badge type="success">Success</cq-badge>
<cq-badge type="warning">Warning</cq-badge>
<cq-badge type="error">Error</cq-badge>
<cq-badge type="info">Info</cq-badge>
<cq-badge type="action">Action</cq-badge>
<cq-badge type="success" size="sm">v1.0</cq-badge>`,
        },
        {
          label: 'Status Dots',
          html: `<cq-badge type="success" [dot]="true"></cq-badge>
<cq-badge type="error" [dot]="true"></cq-badge>
<cq-badge type="live" [dot]="true"></cq-badge>
<cq-badge type="warning" [dot]="true"></cq-badge>`,
        },
      ],
    },
    table: {
      css: [
        {
          label: 'Table',
          html: `<div class="cq-table-container">
  <table class="cq-table">
    <thead>
      <tr><th>ID</th><th>Name</th></tr>
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
      component: [
        {
          label: 'Table (CSS only, no table component)',
          html: `<!-- Tables use CSS classes directly.
   A table component is not needed since
   HTML tables vary too much in structure. -->
<div class="cq-table-container">
  <table class="cq-table">...</table>
</div>`,
        },
      ],
    },
    dialogs: {
      css: [
        {
          label: 'Alert Dialog (CSS)',
          html: `<div class="cq-dialog-overlay">
  <div class="cq-dialog cq-dialog-sm">
    <div class="cq-alert-dialog">
      <div class="cq-dialog-icon cq-dialog-icon-danger">
        <div class="cq-mask-icon cq-mask-icon-error"
          style="--mask-url: url('...')"></div>
      </div>
      <h3 class="cq-alert-dialog-title">Title</h3>
      <p class="cq-alert-dialog-message">Msg</p>
      <div class="cq-alert-dialog-actions">
        <button class="cq-btn-secondary">Cancel</button>
        <button class="cq-btn-danger-filled">Delete</button>
      </div>
    </div>
  </div>
</div>`,
        },
      ],
      component: [
        {
          label: 'Alert (Service)',
          language: 'typescript',
          html: `// Inject the service
constructor(private dialog: CqDialogService) {}

// Show an alert
this.dialog.alert(
  'success', 'Saved!', 'Your changes were saved.', 'OK'
);`,
        },
        {
          label: 'Confirm (Service)',
          language: 'typescript',
          html: `const ref = this.dialog.confirm(
  'danger',
  'Delete Item?',
  'This cannot be undone.',
  'Delete', 'Cancel'
);

ref.afterClosed$.subscribe(result => {
  if (result) { /* user confirmed */ }
});`,
        },
        {
          label: 'Custom Dialog (Service)',
          language: 'typescript',
          html: `// Open a custom component dialog
const ref = this.dialog.open(
  CreateQuizDialog,
  { size: 'md', data: { quizId: 42 } }
);

ref.afterClosed$.subscribe(result => {
  console.log('Dialog result:', result);
});

// Inside the dialog component:
@Component({ ... })
export class CreateQuizDialog {
  constructor(public dialogRef: CqDialogRef) {}

  onSubmit() {
    this.dialogRef.close({ title: 'My Quiz' });
  }
}`,
        },
      ],
    },
    messages: {
      css: [
        {
          label: 'Message Variants',
          html: `<div class="cq-message cq-message-info">
  <div class="cq-message-icon">
    <div class="cq-mask-icon cq-mask-icon-info"
      style="--mask-url: url('...')"></div>
  </div>
  <div class="cq-message-content">
    <h4 class="cq-message-title">Info</h4>
    <p class="cq-message-text">Message</p>
  </div>
</div>`,
        },
      ],
      component: [
        {
          label: 'Message Variants',
          html: `<cq-message type="info" title="Info"
  icon="assets/icons/lightbulb.svg">
  Informational message text.
</cq-message>

<cq-message type="warning" title="Warning"
  icon="assets/icons/warning.svg">
  Warning message text.
</cq-message>

<cq-message type="error" title="Error"
  icon="assets/icons/error.svg">
  Error message text.
</cq-message>`,
        },
      ],
    },
    layout: {
      css: [
        {
          label: 'Page Header',
          html: `<div class="cq-page-header">
  <button class="cq-page-header-back">
    <img src="..." />
  </button>
  <div class="cq-page-header-content">
    <h1 class="cq-page-header-title">Title</h1>
    <p class="cq-page-header-subtitle">Sub</p>
  </div>
  <div class="cq-page-header-actions">
    <button class="cq-btn-primary">Action</button>
  </div>
</div>`,
        },
      ],
      component: [
        {
          label: 'Page Component',
          html: `<cq-page title="My Page"
  subtitle="Page description"
  [hasBackButton]="true"
  (back)="goBack()">
  <cq-button slot="actions">Action</cq-button>
  <!-- Page content here -->
</cq-page>`,
        },
      ],
    },
    utilities: {
      css: [
        {
          label: 'Progress, Spinners, Avatars',
          html: `<!-- Progress Bar -->
<div class="cq-progress">
  <div class="cq-progress-bar" style="width: 75%"></div>
</div>

<!-- Spinner -->
<div class="cq-spinner"></div>

<!-- Avatar -->
<div class="cq-avatar">JD</div>`,
        },
      ],
      component: [
        {
          label: 'Utilities (CSS Only)',
          html: `<!-- Progress bars, spinners, avatars, skeletons,
   and tooltips use CSS classes directly.
   They are simple enough that wrapping in
   components adds little value. -->
<div class="cq-progress">
  <div class="cq-progress-bar" style="width: 75%"></div>
</div>
<div class="cq-spinner"></div>
<div class="cq-avatar">JD</div>`,
        },
      ],
    },
    toasts: {
      css: [],
      component: [
        {
          label: 'Toast Service',
          language: 'typescript',
          html: `// Inject the service
constructor(private toast: CqToastService) {}

// Show different toast types
this.toast.success('Changes saved successfully!');
this.toast.error('Failed to upload file.');
this.toast.warning('Session expires in 5 min.');
this.toast.info('New version available.');`,
        },
        {
          label: 'With Options',
          language: 'typescript',
          html: `this.toast.success('Item deleted', {
  duration: 6000,  // 6 seconds
  action: 'Undo'
});`,
        },
      ],
    },
    tooltips: {
      css: [],
      component: [
        {
          label: 'Tooltip Directive',
          html: `<button cqTooltip="Save changes"
  cqTooltipPosition="top">
  Hover me
</button>

<button cqTooltip="More info"
  cqTooltipPosition="bottom">
  Bottom tooltip
</button>`,
        },
      ],
    },
    avatars: {
      css: [
        {
          label: 'CSS Avatars',
          html: `<div class="cq-avatar">JD</div>
<div class="cq-avatar cq-avatar-lg">AB</div>
<div class="cq-avatar cq-avatar-sm">XY</div>`,
        },
      ],
      component: [
        {
          label: 'Avatar Component',
          html: `<cq-avatar initials="JD" size="lg"></cq-avatar>
<cq-avatar initials="AB" size="md"></cq-avatar>
<cq-avatar initials="XY" size="sm"></cq-avatar>
<cq-avatar src="/photo.jpg" alt="User"
  initials="FB"></cq-avatar>`,
        },
      ],
    },
    progressBars: {
      css: [
        {
          label: 'CSS Progress Bars',
          html: `<div class="cq-progress">
  <div class="cq-progress-bar" style="width: 75%"></div>
</div>
<div class="cq-progress cq-progress-success">
  <div class="cq-progress-bar" style="width: 100%"></div>
</div>`,
        },
      ],
      component: [
        {
          label: 'Progress Bar Component',
          html: `<cq-progress-bar [value]="65"></cq-progress-bar>
<cq-progress-bar [value]="100" type="success"
  [showLabel]="true"></cq-progress-bar>
<cq-progress-bar [value]="45" type="warning"
  [animated]="true" [showLabel]="true">
</cq-progress-bar>
<cq-progress-bar [value]="20" type="error">
</cq-progress-bar>`,
        },
      ],
    },
    dropdowns: {
      css: [],
      component: [
        {
          label: 'Dropdown Menu',
          html: `<cq-dropdown>
  <button slot="trigger" class="cq-btn-outlined">
    Actions ▾
  </button>
  <cq-dropdown-item (selected)="onEdit()">
    Edit
  </cq-dropdown-item>
  <cq-dropdown-item (selected)="onDuplicate()">
    Duplicate
  </cq-dropdown-item>
  <cq-dropdown-divider></cq-dropdown-divider>
  <cq-dropdown-item (selected)="onDelete()">
    Delete
  </cq-dropdown-item>
</cq-dropdown>`,
        },
      ],
    },
  };

  constructor(
    private dialogService: CqDialogService,
    private cdr: ChangeDetectorRef,
    private toastService: CqToastService,
  ) {
    const savedTheme = localStorage.getItem('cq-theme') as 'dark' | 'light';
    if (savedTheme) {
      this.currentTheme.set(savedTheme);
      document.documentElement.setAttribute('data-theme', savedTheme);
    }
    setTimeout(() => this.initializeAllColors(), 0);
  }

  initializeAllColors() {
    const computedStyle = getComputedStyle(document.documentElement);
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        const value = computedStyle.getPropertyValue(color.variable).trim();
        color.value = this.rgbToHex(value) || value;
      });
    });
  }

  updateColor(variable: string, event: Event) {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    document.documentElement.style.setProperty(variable, value);
    this.colorGroups.forEach((group) => {
      const color = group.colors.find((c) => c.variable === variable);
      if (color) color.value = value;
    });
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

  resetAllColors() {
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        document.documentElement.style.removeProperty(color.variable);
      });
    });
    this.fontSizes.forEach((fs) => {
      fs.value = fs.default;
      document.documentElement.style.removeProperty(fs.variable);
    });
    setTimeout(() => this.initializeAllColors(), 50);
  }

  toggleCodeTooltip(sectionId: string) {
    this.activeCodeTooltip = this.activeCodeTooltip === sectionId ? null : sectionId;
  }

  closeCodeTooltip() {
    this.activeCodeTooltip = null;
  }

  private rgbToHex(rgb: string): string {
    if (rgb.startsWith('#')) return rgb;
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

    // Remove all inline overrides
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        document.documentElement.style.removeProperty(color.variable);
      });
    });
    this.fontSizes.forEach((fs) => {
      fs.value = fs.default;
      document.documentElement.style.removeProperty(fs.variable);
    });
    setTimeout(() => this.initializeAllColors(), 50);
  }

  simulateLoading1() {
    this.isLoading1 = true;
    setTimeout(() => (this.isLoading1 = false), 2000);
  }

  simulateLoading2() {
    this.isLoading2 = true;
    setTimeout(() => (this.isLoading2 = false), 2000);
  }

  // Dialog service methods
  openQuestionDialog() {
    const ref = this.dialogService.confirm(
      'question',
      'Confirm Action',
      'Do you want to proceed with this operation?',
      'Yes, Proceed',
      'No',
    );
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(`Question dialog: ${result}`);
      // this.cdr.detectChanges();
    });
  }
  openConfirmDialog() {
    const ref = this.dialogService.confirm(
      'danger',
      'Delete Item?',
      'Are you sure you want to delete this item? This action cannot be undone.',
      'Delete',
      'Cancel',
    );
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(`Danger confirm: ${result}`);
      // this.cdr.detectChanges();
    });
  }
  openSuccessDialog() {
    const ref = this.dialogService.alert(
      'success',
      'Success!',
      'Your changes have been saved successfully.',
      'Continue',
    );
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(`Success alert: ${result}`);
      // this.cdr.detectChanges();
    });
  }
  openWarningDialog() {
    const ref = this.dialogService.confirm(
      'warning',
      'Warning',
      'This action may have unintended consequences. Please review before proceeding.',
      'Proceed Anyway',
      'Cancel',
    );
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(`Warning: ${result}`);
      // this.cdr.detectChanges();
    });
  }
  openInfoDialog() {
    const ref = this.dialogService.alert(
      'info',
      'Did You Know?',
      'You can use keyboard shortcuts to navigate quickly. Press Ctrl+S to save anytime.',
      'Got it!',
    );
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(`Info alert: ${result}`);
      // this.cdr.detectChanges();
    });
  }
  openFormDialog() {
    const ref = this.dialogService.open(CreateQuizDialog, { size: 'md' });
    ref.afterClosed$.subscribe((result) => {
      this.lastDialogResult.set(
        result ? `Created quiz: ${JSON.stringify(result)}` : 'Form cancelled',
      );
      // this.cdr.detectChanges();
    });
  }
  // Toast demos
  showSuccessToast() {
    this.toastService.success('Changes saved successfully!');
  }
  showErrorToast() {
    this.toastService.error('Failed to process. Please try again.');
  }
  showWarningToast() {
    this.toastService.warning('Your session will expire in 5 minutes.');
  }
  showInfoToast() {
    this.toastService.info('A new version is available.');
  }

  openColorConfig() {
    this.showColorConfigDialog = true;
  }
  closeColorConfig() {
    this.showColorConfigDialog = false;
  }

  // Save/Export configuration
  saveConfiguration() {
    const lines: string[] = [
      '/* CodeQuiz Design System — Custom Configuration */',
      `/* Generated: ${new Date().toISOString()} */`,
      `/* Theme: ${this.currentTheme()} */`,
      '',
      ':root {',
    ];

    // Collect color overrides
    this.colorGroups.forEach((group) => {
      lines.push(`  /* ${group.name} */`);
      group.colors.forEach((color) => {
        if (color.value) {
          lines.push(`  ${color.variable}: ${color.value};`);
        }
      });
      lines.push('');
    });

    // Collect font size overrides
    const changedFonts = this.fontSizes.filter((fs) => fs.value !== fs.default);
    if (changedFonts.length > 0) {
      lines.push('  /* Font Sizes */');
      changedFonts.forEach((fs) => {
        lines.push(`  ${fs.variable}: ${fs.value}px;`);
      });
      lines.push('');
    }

    lines.push('}');

    // Download as CSS file
    const blob = new Blob([lines.join('\n')], { type: 'text/css' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `cq-design-system-config-${this.currentTheme()}.css`;
    link.click();
    URL.revokeObjectURL(url);
  }
}
