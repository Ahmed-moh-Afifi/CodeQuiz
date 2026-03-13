import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ExecutionApiService } from '../api/services/execution-api.service';
import type {
  RunCodeRequest,
  CodeRunnerResult,
  SupportedLanguage,
} from '../api/models/execution.models';

@Injectable({ providedIn: 'root' })
export class ExecutionService {
  private readonly executionApi = inject(ExecutionApiService);

  async getSupportedLanguages(): Promise<SupportedLanguage[]> {
    const response = await firstValueFrom(this.executionApi.getSupportedLanguages());
    return response.data!;
  }

  async runCode(request: RunCodeRequest): Promise<CodeRunnerResult> {
    const response = await firstValueFrom(this.executionApi.runCode(request));
    return response.data!;
  }
}
