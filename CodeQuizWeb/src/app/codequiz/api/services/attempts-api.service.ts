import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import {
  BeginAttemptRequest,
  ExamineeAttempt,
  ExaminerAttempt,
  SolutionDTO,
  UpdateSolutionGradeRequest,
  BatchUpdateSolutionGradesRequest,
} from '../models/attempt.models';

@Injectable({ providedIn: 'root' })
export class AttemptsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Attempts`;
  }

  beginAttempt(request: BeginAttemptRequest): Observable<ApiResponse<ExamineeAttempt>> {
    return this.http.post<ApiResponse<ExamineeAttempt>>(`${this.url}/begin`, request);
  }

  submitAttempt(attemptId: number): Observable<ApiResponse<ExamineeAttempt>> {
    return this.http.post<ApiResponse<ExamineeAttempt>>(`${this.url}/${attemptId}/submit`, {});
  }

  updateSolution(solution: SolutionDTO): Observable<ApiResponse<SolutionDTO>> {
    return this.http.put<ApiResponse<SolutionDTO>>(`${this.url}/solutions`, solution);
  }

  updateSolutionGrade(request: UpdateSolutionGradeRequest): Observable<ApiResponse<SolutionDTO>> {
    return this.http.put<ApiResponse<SolutionDTO>>(`${this.url}/solutions/grade`, request);
  }

  batchUpdateSolutionGrades(
    request: BatchUpdateSolutionGradesRequest,
  ): Observable<ApiResponse<SolutionDTO[]>> {
    return this.http.put<ApiResponse<SolutionDTO[]>>(`${this.url}/solutions/grades/batch`, request);
  }

  aiReassessSolution(solutionId: number): Observable<ApiResponse<SolutionDTO>> {
    return this.http.post<ApiResponse<SolutionDTO>>(
      `${this.url}/solutions/${solutionId}/ai-reassess`,
      {},
    );
  }

  getMyAttempts(): Observable<ApiResponse<ExamineeAttempt[]>> {
    return this.http.get<ApiResponse<ExamineeAttempt[]>>(`${this.url}/user`);
  }

  getAttempt(attemptId: number): Observable<ApiResponse<ExaminerAttempt>> {
    return this.http.get<ApiResponse<ExaminerAttempt>>(`${this.url}/${attemptId}`);
  }
}
