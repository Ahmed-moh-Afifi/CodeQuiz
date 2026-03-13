import { Component, signal, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import {
  CqButton,
  CqInput,
  CqTextarea,
  CqSelect,
  CqSkeleton,
  CqBadge,
  CqDialogService,
  CqDialogRef,
  CqIcon,
} from '../../../design-system';
import type { CqSelectOption } from '../../../design-system/components/inputs/cq-select/cq-select';
import { QuizzesService } from '../../services/quizzes.service';
import { ExecutionService } from '../../services/execution.service';
import { AuthStore } from '../../api/auth.store';
import type {
  NewQuizModel,
  NewQuestionModel,
  QuestionConfiguration,
  ExaminerQuiz,
} from '../../api/models/quiz.models';
import type { TestCase, SupportedLanguage } from '../../api/models/execution.models';

@Component({
  selector: 'app-create-quiz',
  standalone: true,
  imports: [
    CqButton,
    CqInput,
    CqTextarea,
    CqSelect,
    CqSkeleton,
    CqBadge,
    CqIcon,
    FormsModule,
    DatePipe,
  ],
  templateUrl: './create-quiz.html',
  styleUrl: './create-quiz.scss',
})
export class CreateQuiz implements OnInit {
  private readonly quizzesService = inject(QuizzesService);
  private readonly executionService = inject(ExecutionService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialogService = inject(CqDialogService);

  loading = signal(true);
  saving = signal(false);

  // Edit mode
  editQuizId: number | null = null;
  get isEditing(): boolean {
    return this.editQuizId !== null;
  }
  get pageTitle(): string {
    return this.isEditing ? 'Edit Quiz' : 'New Quiz';
  }

  // Quiz fields
  quizTitle = '';
  durationMinutes = '';
  startDate = '';
  startTime = '';
  endDate = '';
  endTime = '';

  // Language
  languages: SupportedLanguage[] = [];
  languageOptions: CqSelectOption[] = [];
  selectedLanguage = '';

  // Configuration
  allowExecution = false;
  showOutput = false;
  showErrors = false;
  allowIntellisense = false;
  allowSignatureHelp = false;
  allowMultipleAttempts = false;

  // Questions
  questions = signal<NewQuestionModel[]>([]);

  // Add question dialog state
  showQuestionDialog = signal(false);
  editingQuestionIndex: number | null = null;
  qStatement = '';
  qEditorCode = '';
  qPoints = '';
  qTestCases: TestCase[] = [];

  async ngOnInit() {
    this.loading.set(true);
    try {
      this.languages = await this.executionService.getSupportedLanguages();
      this.languageOptions = this.languages.map((l) => ({ value: l.name, label: l.name }));

      // Set defaults
      const now = new Date();
      const tomorrow = new Date(now);
      tomorrow.setDate(tomorrow.getDate() + 1);
      const fiveDays = new Date(now);
      fiveDays.setDate(fiveDays.getDate() + 5);

      this.startDate = this.formatDateForInput(tomorrow);
      this.startTime = '09:00';
      this.endDate = this.formatDateForInput(fiveDays);
      this.endTime = '23:59';

      // Check if editing
      const quizId = this.route.snapshot.queryParams['quizId'];
      if (quizId) {
        await this.loadQuizForEdit(parseInt(quizId, 10));
      }
    } finally {
      this.loading.set(false);
    }
  }

  private async loadQuizForEdit(quizId: number) {
    const quizzes = await this.quizzesService.getMyQuizzes();
    const quiz = quizzes.find((q) => q.id === quizId);
    if (!quiz) return;

    this.editQuizId = quizId;
    this.quizTitle = quiz.title;
    this.allowMultipleAttempts = quiz.allowMultipleAttempts;

    // Parse duration (HH:mm:ss)
    const dParts = quiz.duration.split(':');
    const totalMins = parseInt(dParts[0], 10) * 60 + parseInt(dParts[1], 10);
    this.durationMinutes = totalMins.toString();

    // Parse dates
    const start = new Date(quiz.startDate);
    const end = new Date(quiz.endDate);
    this.startDate = this.formatDateForInput(start);
    this.startTime = this.formatTimeForInput(start);
    this.endDate = this.formatDateForInput(end);
    this.endTime = this.formatTimeForInput(end);

    // Configuration
    const config = quiz.globalQuestionConfiguration;
    this.selectedLanguage = config.language;
    this.allowExecution = config.allowExecution;
    this.showOutput = config.showOutput;
    this.showErrors = config.showError;
    this.allowIntellisense = config.allowIntellisense;
    this.allowSignatureHelp = config.allowSignatureHelp;

    // Questions
    this.questions.set(
      quiz.questions.map((q) => ({
        statement: q.statement,
        editorCode: q.editorCode,
        questionConfiguration: q.questionConfiguration,
        testCases: q.testCases,
        order: q.order,
        points: q.points,
      })),
    );
  }

  // --- Question dialog ---
  openAddQuestion() {
    this.editingQuestionIndex = null;
    this.qStatement = '';
    this.qEditorCode = '';
    this.qPoints = '';
    this.qTestCases = [];
    this.showQuestionDialog.set(true);
  }

  openEditQuestion(index: number) {
    const q = this.questions()[index];
    this.editingQuestionIndex = index;
    this.qStatement = q.statement;
    this.qEditorCode = q.editorCode;
    this.qPoints = q.points.toString();
    this.qTestCases = [...q.testCases.map((tc) => ({ ...tc }))];
    this.showQuestionDialog.set(true);
  }

  closeQuestionDialog() {
    this.showQuestionDialog.set(false);
  }

  saveQuestion() {
    const points = parseFloat(this.qPoints) || 0;
    const q: NewQuestionModel = {
      statement: this.qStatement,
      editorCode: this.qEditorCode,
      testCases: this.qTestCases,
      order: 0,
      points,
    };

    const current = [...this.questions()];
    if (this.editingQuestionIndex !== null) {
      current[this.editingQuestionIndex] = q;
    } else {
      current.push(q);
    }
    // Reorder
    current.forEach((item, i) => (item.order = i + 1));
    this.questions.set(current);
    this.showQuestionDialog.set(false);
  }

  deleteQuestion(index: number) {
    const ref = this.dialogService.confirm(
      'danger',
      'Delete Question',
      `Are you sure you want to delete Question ${index + 1}?`,
      'Delete',
      'Cancel',
    );
    ref.afterClosed$.subscribe((confirmed) => {
      if (confirmed) {
        const current = [...this.questions()];
        current.splice(index, 1);
        current.forEach((item, i) => (item.order = i + 1));
        this.questions.set(current);
      }
    });
  }

  addTestCase() {
    this.qTestCases.push({
      testCaseNumber: this.qTestCases.length + 1,
      input: [],
      expectedOutput: '',
    });
  }

  removeTestCase(index: number) {
    this.qTestCases.splice(index, 1);
    this.qTestCases.forEach((tc, i) => (tc.testCaseNumber = i + 1));
  }

  updateTestCaseInput(tc: TestCase, value: string) {
    tc.input = value.split('\n');
  }

  getTestCaseInputText(tc: TestCase): string {
    return tc.input.join('\n');
  }

  // --- Validation ---
  validate(): string[] {
    const errors: string[] = [];
    if (!this.quizTitle.trim()) errors.push('Quiz title is required');
    if (!this.durationMinutes || isNaN(parseInt(this.durationMinutes, 10)))
      errors.push('Invalid duration');
    if (!this.selectedLanguage) errors.push('Programming language is required');
    if (this.questions().length === 0) errors.push('At least one question is required');

    const startDt = new Date(`${this.startDate}T${this.startTime}`);
    const endDt = new Date(`${this.endDate}T${this.endTime}`);
    if (endDt <= startDt) errors.push('End date must be after start date');

    const mins = parseInt(this.durationMinutes, 10);
    if (!isNaN(mins) && (endDt.getTime() - startDt.getTime()) / 60000 < mins) {
      errors.push('Availability period is shorter than quiz duration');
    }

    return errors;
  }

  // --- Publish ---
  async publishQuiz() {
    const errors = this.validate();
    if (errors.length > 0) {
      this.dialogService.alert('danger', 'Invalid Input', errors.join('\n'));
      return;
    }

    const action = this.isEditing ? 'update' : 'publish';
    const ref = this.dialogService.confirm(
      'info',
      `Confirm ${this.isEditing ? 'Update' : 'Publish'}`,
      `Are you sure you want to ${action} this quiz?`,
      this.isEditing ? 'Update' : 'Publish',
      'Cancel',
    );

    ref.afterClosed$.subscribe(async (confirmed) => {
      if (!confirmed) return;

      this.saving.set(true);
      try {
        const duration = parseInt(this.durationMinutes, 10);
        const hours = Math.floor(duration / 60);
        const mins = duration % 60;

        const model: NewQuizModel = {
          title: this.quizTitle.trim(),
          startDate: new Date(`${this.startDate}T${this.startTime}`).toISOString(),
          endDate: new Date(`${this.endDate}T${this.endTime}`).toISOString(),
          duration: `${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}:00`,
          examinerId: this.authStore.user()!.id,
          allowMultipleAttempts: this.allowMultipleAttempts,
          globalQuestionConfiguration: {
            language: this.selectedLanguage,
            allowExecution: this.allowExecution,
            showOutput: this.showOutput,
            showError: this.showErrors,
            allowIntellisense: this.allowIntellisense,
            allowSignatureHelp: this.allowSignatureHelp,
          },
          questions: this.questions(),
        };

        if (this.isEditing) {
          await this.quizzesService.updateQuiz(this.editQuizId!, model);
        } else {
          await this.quizzesService.createQuiz(model);
        }

        this.router.navigate(['/app']);
      } finally {
        this.saving.set(false);
      }
    });
  }

  goBack() {
    if (this.questions().length > 0 || this.quizTitle.trim()) {
      const ref = this.dialogService.confirm(
        'warning',
        'Discard Changes?',
        'You have unsaved changes. Are you sure you want to go back?',
        'Discard',
        'Stay',
      );
      ref.afterClosed$.subscribe((confirmed) => {
        if (confirmed) this.router.navigate(['/app']);
      });
    } else {
      this.router.navigate(['/app']);
    }
  }

  // --- Helpers ---
  private formatDateForInput(d: Date): string {
    return d.toISOString().split('T')[0];
  }

  private formatTimeForInput(d: Date): string {
    return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
  }
}
