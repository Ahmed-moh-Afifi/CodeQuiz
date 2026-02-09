import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

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

  constructor() {
    // Check for saved theme preference
    const savedTheme = localStorage.getItem('cq-theme') as 'dark' | 'light';
    if (savedTheme) {
      this.currentTheme.set(savedTheme);
      document.documentElement.setAttribute('data-theme', savedTheme);
    }

    // Initialize color values after a brief delay to ensure styles are applied
    setTimeout(() => this.initializeColors(), 0);
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

    // Clear inline styles to revert to new theme defaults
    // Or keep them? User said "see changes live".
    // Usually changing theme should reset to that theme's defaults unless we want to persist custom overrides.
    // Given the prompt "current colors are the default colors", when we switch, we should probably
    // reload the defaults for the NEW theme.

    // Remove inline overrides for the demo constants
    this.colorGroups.forEach((group) => {
      group.colors.forEach((color) => {
        document.documentElement.style.removeProperty(color.variable);
      });
    });

    // Re-initialize values for options
    setTimeout(() => this.initializeColors(), 50);
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
