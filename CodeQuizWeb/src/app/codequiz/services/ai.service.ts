import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AiApiService } from '../api/services/ai-api.service';
import type { GenerateTestCasesRequest, GeneratedTestCase } from '../api/models/ai.models';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly aiApi = inject(AiApiService);

  async generateTestCases(request: GenerateTestCasesRequest): Promise<GeneratedTestCase[]> {
    const response = await firstValueFrom(this.aiApi.generateTestCases(request));
    return response.data!;
  }
}
