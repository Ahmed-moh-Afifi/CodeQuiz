import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import { NewQuizModel, ExaminerQuiz, ExamineeQuiz } from '../models/quiz.models';
import { ExaminerAttempt } from '../models/attempt.models';

@Injectable({ providedIn: 'root' })
export class QuizzesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Quizzes`;
  }

  createQuiz(model: NewQuizModel): Observable<ApiResponse<ExaminerQuiz>> {
    return this.http.post<ApiResponse<ExaminerQuiz>>(this.url, model);
  }

  updateQuiz(id: number, model: NewQuizModel): Observable<ApiResponse<ExaminerQuiz>> {
    return this.http.put<ApiResponse<ExaminerQuiz>>(`${this.url}/${id}`, model);
  }

  getMyQuizzes(): Observable<ApiResponse<ExaminerQuiz[]>> {
    return this.http.get<ApiResponse<ExaminerQuiz[]>>(`${this.url}/user`);
  }

  getUserQuizzes(userId: string): Observable<ApiResponse<ExaminerQuiz[]>> {
    return this.http.get<ApiResponse<ExaminerQuiz[]>>(
      `${this.url}/user/${encodeURIComponent(userId)}`,
    );
  }

  deleteQuiz(id: number): Observable<ApiResponse<object>> {
    return this.http.delete<ApiResponse<object>>(`${this.url}/${id}`);
  }

  getQuizByCode(code: string): Observable<ApiResponse<ExamineeQuiz>> {
    return this.http.get<ApiResponse<ExamineeQuiz>>(`${this.url}/code/${encodeURIComponent(code)}`);
  }

  getQuizAttempts(quizId: number): Observable<ApiResponse<ExaminerAttempt[]>> {
    return this.http.get<ApiResponse<ExaminerAttempt[]>>(`${this.url}/${quizId}/attempts`);
  }
}
