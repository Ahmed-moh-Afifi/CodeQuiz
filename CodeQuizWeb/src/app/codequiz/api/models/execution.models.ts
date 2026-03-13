export interface RunCodeRequest {
  language: string;
  code: string;
  input: string[];
  containOutput: boolean;
  containError: boolean;
}

export interface CodeRunnerResult {
  success: boolean;
  output?: string;
  error?: string;
}

export interface SupportedLanguage {
  name: string;
  extension: string;
}

export interface TestCase {
  testCaseNumber: number;
  input: string[];
  expectedOutput: string;
}

export interface EvaluationResult {
  testCase: TestCase;
  output: string;
  isSuccessful: boolean;
}
