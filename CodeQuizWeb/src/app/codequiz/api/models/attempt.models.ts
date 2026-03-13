import { EvaluationResult } from './execution.models';
import { ExamineeQuiz } from './quiz.models';
import { UserDTO } from './user.models';

export interface AiAssessmentDTO {
  id: number;
  solutionId: number;
  isValid: boolean;
  confidenceScore: number;
  reasoning: string;
  flags: string[];
  assessedAt: string;
  model: string;
  suggestedGrade?: number;
}

export interface SolutionDTO {
  id: number;
  code: string;
  questionId: number;
  attemptId: number;
  evaluatedBy?: string;
  receivedGrade?: number;
  feedback?: string;
  evaluationResults?: EvaluationResult[];
  aiAssessment?: AiAssessmentDTO;
}

export interface BeginAttemptRequest {
  quizCode: string;
}

export interface ExamineeAttempt {
  id: number;
  startTime: string;
  endTime?: string;
  quizId: number;
  examineeId: string;
  grade?: number;
  gradePercentage?: number;
  quiz: ExamineeQuiz;
  solutions: SolutionDTO[];
  maxEndTime: string;
}

export interface ExaminerAttempt {
  id: number;
  startTime: string;
  endTime?: string;
  quizId: number;
  examineeId: string;
  totalPoints: number;
  grade?: number;
  gradePercentage?: number;
  solutions: SolutionDTO[];
  examinee: UserDTO;
}

export interface UpdateSolutionGradeRequest {
  solutionId: number;
  receivedGrade?: number;
  evaluatedBy?: string;
  feedback?: string;
}

export interface SolutionGradeUpdate {
  solutionId: number;
  questionNumber: number;
  totalPoints: number;
  receivedGrade?: number;
  oldGrade?: number;
  feedback?: string;
  oldFeedback?: string;
  evaluatedBy?: string;
}

export interface BatchUpdateSolutionGradesRequest {
  attemptId: number;
  updates: SolutionGradeUpdate[];
}
