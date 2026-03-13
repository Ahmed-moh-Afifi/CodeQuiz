import { TestCase } from './execution.models';

export interface GenerateTestCasesRequest {
  problemStatement: string;
  language: string;
  existingTestCases?: TestCase[];
  sampleSolution?: string;
  count?: number;
}

export interface GeneratedTestCase {
  testCase: TestCase;
  description: string;
  category: string;
  confidence: number;
}
