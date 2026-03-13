import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import { RunCodeRequest, CodeRunnerResult, SupportedLanguage } from '../models/execution.models';

@Injectable({ providedIn: 'root' })
export class ExecutionApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Execution`;
  }

  getSupportedLanguages(): Observable<ApiResponse<SupportedLanguage[]>> {
    return this.http.get<ApiResponse<SupportedLanguage[]>>(`${this.url}/languages`);
  }

  runCode(request: RunCodeRequest): Observable<ApiResponse<CodeRunnerResult>> {
    return this.http.post<ApiResponse<CodeRunnerResult>>(`${this.url}/run`, request);
  }
}
