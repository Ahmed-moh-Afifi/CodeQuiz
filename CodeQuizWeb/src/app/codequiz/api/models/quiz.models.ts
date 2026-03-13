import { TestCase } from './execution.models';
import { UserDTO } from './user.models';

export interface QuestionConfiguration {
  language: string;
  allowExecution: boolean;
  showOutput: boolean;
  showError: boolean;
  allowIntellisense: boolean;
  allowSignatureHelp: boolean;
}

export interface NewQuestionModel {
  statement: string;
  editorCode: string;
  questionConfiguration?: QuestionConfiguration;
  testCases: TestCase[];
  order: number;
  points: number;
}

export interface NewQuizModel {
  title: string;
  startDate: string;
  endDate: string;
  duration: string;
  examinerId: string;
  globalQuestionConfiguration: QuestionConfiguration;
  allowMultipleAttempts: boolean;
  questions: NewQuestionModel[];
}

export interface QuestionDTO {
  id: number;
  statement: string;
  editorCode: string;
  questionConfiguration: QuestionConfiguration;
  testCases: TestCase[];
  quizId: number;
  order: number;
  points: number;
}

export interface ExaminerQuiz {
  id: number;
  title: string;
  startDate: string;
  endDate: string;
  duration: string;
  code: string;
  examinerId: string;
  globalQuestionConfiguration: QuestionConfiguration;
  allowMultipleAttempts: boolean;
  showAiFeedbackToStudents: boolean;
  questions: QuestionDTO[];
  questionsCount: number;
  attemptsCount: number;
  submittedAttemptsCount: number;
  averageAttemptScore: number;
  totalPoints: number;
}

export interface ExamineeQuiz {
  id: number;
  title: string;
  startDate: string;
  endDate: string;
  duration: string;
  code: string;
  examinerId: string;
  globalQuestionConfiguration: QuestionConfiguration;
  allowMultipleAttempts: boolean;
  showAiFeedbackToStudents: boolean;
  questions: QuestionDTO[];
  examiner: UserDTO;
  questionsCount: number;
  totalPoints: number;
}
