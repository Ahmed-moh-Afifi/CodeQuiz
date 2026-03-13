import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { ApiResponse } from '../models/api-response.model';
import { GenerateTestCasesRequest, GeneratedTestCase } from '../models/ai.models';

@Injectable({ providedIn: 'root' })
export class AiApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get url(): string {
    return `${this.baseUrl}/Ai`;
  }

  generateTestCases(
    request: GenerateTestCasesRequest,
  ): Observable<ApiResponse<GeneratedTestCase[]>> {
    return this.http.post<ApiResponse<GeneratedTestCase[]>>(
      `${this.url}/generate-testcases`,
      request,
    );
  }
}
