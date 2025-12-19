# CodeQuiz - UML Class Diagrams

## 1. Domain Model Class Diagram (Overview)

```plantuml
@startuml Domain_Model_Overview
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Authentication" {
    class User {
        +Id: string
        +FirstName: string
        +LastName: string
        +UserName: string
        +Email: string
        +JoinDate: DateTime
        +ProfilePicture: string?
        +ToDTO(): UserDTO
    }

    class RefreshToken {
        +Id: int
        +Token: string
        +UserId: string
        +ExpiryDate: DateTime
        +IsRevoked: bool
    }
}

package "Quiz Management" {
    class Quiz {
        +Id: int
        +Title: string
        +StartDate: DateTime
        +EndDate: DateTime
        +Duration: TimeSpan
        +Code: string
        +ExaminerId: string
        +AllowMultipleAttempts: bool
        +TotalPoints: float
        +GlobalQuestionConfiguration: QuestionConfiguration
        +ToExaminerQuiz(): ExaminerQuiz
        +ToExamineeQuiz(): ExamineeQuiz
    }

    class Question {
        +Id: int
        +Statement: string
        +EditorCode: string
        +QuizId: int
        +Order: int
        +Points: float
        +QuestionConfiguration: QuestionConfiguration?
        +TestCases: List<TestCase>
        +ToDTO(): QuestionDTO
    }

    class QuestionConfiguration {
        +Language: string
        +AllowExecution: bool
        +ShowOutput: bool
        +ShowError: bool
    }

    class Attempt {
        +Id: int
        +StartTime: DateTime
        +EndTime: DateTime?
        +QuizId: int
        +ExamineeId: string
        +ToExamineeAttempt(): ExamineeAttempt
        +ToExaminerAttempt(): ExaminerAttempt
    }

    class Solution {
        +Id: int
        +Code: string
        +QuestionId: int
        +AttemptId: int
        +EvaluatedBy: string?
        +ReceivedGrade: float?
        +EvaluationResults: List<EvaluationResult>?
        +ToDTO(): SolutionDTO
    }
}

package "Execution" {
    class TestCase {
        +TestCaseNumber: int
        +Input: List<string>
        +ExpectedOutput: string
    }

    class EvaluationResult {
        +TestCase: TestCase
        +Output: string
        +IsSuccessful: bool
    }

    class CodeRunnerResult {
        +Success: bool
        +Output: string?
        +Error: string?
    }
}

' Relationships
User "1" -- "*" Quiz : creates >
User "1" -- "*" Attempt : takes >
User "1" -- "*" RefreshToken : has >

Quiz "1" *-- "*" Question : contains >
Quiz "1" *-- "1" QuestionConfiguration : has >
Quiz "1" -- "*" Attempt : has >

Question "*" -- "1" QuestionConfiguration : may have >
Question "1" *-- "*" TestCase : contains >

Attempt "1" *-- "*" Solution : contains >
Attempt "*" -- "1" Quiz : for >

Solution "*" -- "1" Question : answers >
Solution "1" *-- "*" EvaluationResult : has >

EvaluationResult "*" -- "1" TestCase : for >

@enduml
```

---

## 2. Authentication Module Class Diagram

```plantuml
@startuml Authentication_Module
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Models" {
    class User {
        +Id: string
        +FirstName: string
        +LastName: string
        +UserName: string
        +Email: string
        +JoinDate: DateTime
        +ProfilePicture: string?
        +FromRegisterModel(model): User
        +ToDTO(): UserDTO
        +UpdateFromDTO(dto): void
    }

    class RefreshToken {
        +Id: int
        +Token: string
        +UserId: string
        +ExpiryDate: DateTime
        +IsRevoked: bool
    }

    class RegisterModel {
        +FirstName: string
        +LastName: string
        +Username: string
        +Email: string
        +Password: string
    }

    class LoginModel {
        +Username: string
        +Password: string
    }

    class LoginResult {
        +User: UserDTO
        +TokenModel: TokenModel
    }

    class TokenModel {
        +AccessToken: string
        +RefreshToken: string
    }

    class ResetPasswordModel {
        +Username: string
        +Password: string
        +NewPassword: string
    }

    class ForgetPasswordModel {
        +Email: string
    }

    class UserDTO {
        +Id: string
        +FirstName: string
        +LastName: string
        +UserName: string
        +Email: string
        +JoinDate: DateTime
        +ProfilePicture: string?
    }
}

package "Services" {
    interface IAuthenticationService {
        +Register(model): Task<UserDTO>
        +Login(model): Task<LoginResult>
        +RefreshToken(model): Task<TokenModel>
        +ResetPassword(model): Task
        +ForgetPassword(model): Task
        +ResetPasswordWithToken(model): Task
    }

    class JWTAuthenticationService {
        -userManager: UserManager<User>
        -tokenService: TokenService
        -dbContext: ApplicationDbContext
        +Register(model): Task<UserDTO>
        +Login(model): Task<LoginResult>
        +RefreshToken(model): Task<TokenModel>
        +ResetPassword(model): Task
        +ForgetPassword(model): Task
        +ResetPasswordWithToken(model): Task
    }

    class TokenService {
        -configuration: IConfiguration
        +GenerateAccessToken(claims): string
        +GenerateRefreshToken(): string
        +GetPrincipalFromExpiredToken(token): List<Claim>
    }
}

package "Controllers" {
    class AuthenticationController {
        -authenticationService: IAuthenticationService
        +Register(model): ActionResult<ApiResponse<UserDTO>>
        +Login(model): ActionResult<ApiResponse<LoginResult>>
        +Refresh(model): ActionResult<ApiResponse<TokenModel>>
        +ResetPassword(model): ActionResult<ApiResponse>
        +ForgetPasswordRequest(model): ActionResult<ApiResponse>
    }

    class UsersController {
        -usersRepository: IUsersRepository
        +GetUser(id): ActionResult<ApiResponse<UserDTO>>
        +SearchUsers(query): ActionResult<ApiResponse<List<UserDTO>>>
        +UpdateUser(dto): ActionResult<ApiResponse<UserDTO>>
        +CheckUsernameAvailability(username): ActionResult<ApiResponse<bool>>
    }
}

package "Repositories" {
    interface IUsersRepository {
        +GetUserById(id): Task<User?>
        +SearchUsers(query): Task<List<User>>
        +UpdateUser(user): Task<User>
        +IsUsernameAvailable(username): Task<bool>
    }

    class UsersRepository {
        -dbContext: ApplicationDbContext
        +GetUserById(id): Task<User?>
        +SearchUsers(query): Task<List<User>>
        +UpdateUser(user): Task<User>
        +IsUsernameAvailable(username): Task<bool>
    }
}

' Relationships
IAuthenticationService <|.. JWTAuthenticationService
IUsersRepository <|.. UsersRepository

AuthenticationController --> IAuthenticationService
UsersController --> IUsersRepository

JWTAuthenticationService --> TokenService
JWTAuthenticationService ..> User
JWTAuthenticationService ..> RefreshToken

LoginResult *-- UserDTO
LoginResult *-- TokenModel

User ..> UserDTO : creates
User ..> RegisterModel : from

@enduml
```

---

## 3. Quiz Management Module Class Diagram

```plantuml
@startuml Quiz_Management_Module
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Models" {
    class Quiz {
        +Id: int
        +Title: string
        +StartDate: DateTime
        +EndDate: DateTime
        +Duration: TimeSpan
        +Code: string
        +ExaminerId: string
        +AllowMultipleAttempts: bool
        +TotalPoints: float
        +GlobalQuestionConfiguration: QuestionConfiguration
        +Examiner: User
        +Questions: List<Question>
        +Attempts: List<Attempt>
        +ToExaminerQuiz(): ExaminerQuiz
        +ToExamineeQuiz(): ExamineeQuiz
    }

    class Question {
        +Id: int
        +Statement: string
        +EditorCode: string
        +QuizId: int
        +Order: int
        +Points: float
        +QuestionConfiguration: QuestionConfiguration?
        +TestCases: List<TestCase>
        +Quiz: Quiz
        +ToDTO(): QuestionDTO
    }

    class QuestionConfiguration {
        +Language: string
        +AllowExecution: bool
        +ShowOutput: bool
        +ShowError: bool
    }

    class Attempt {
        +Id: int
        +StartTime: DateTime
        +EndTime: DateTime?
        +QuizId: int
        +ExamineeId: string
        +Quiz: Quiz
        +Solutions: List<Solution>
        +Examinee: User
        +ToExamineeAttempt(): ExamineeAttempt
        +ToExaminerAttempt(): ExaminerAttempt
    }

    class Solution {
        +Id: int
        +Code: string
        +QuestionId: int
        +AttemptId: int
        +EvaluatedBy: string?
        +ReceivedGrade: float?
        +EvaluationResults: List<EvaluationResult>?
        +Question: Question
        +Attempt: Attempt
        +ToDTO(): SolutionDTO
    }
}

package "DTOs" {
    class NewQuizModel {
        +Title: string
        +StartDate: DateTime
        +EndDate: DateTime
        +Duration: TimeSpan
        +ExaminerId: string
        +AllowMultipleAttempts: bool
        +GlobalQuestionConfiguration: QuestionConfiguration
        +Questions: List<NewQuestionModel>
        +Validate(): List<string>
    }

    class NewQuestionModel {
        +Statement: string
        +EditorCode: string
        +Order: int
        +Points: float
        +QuestionConfiguration: QuestionConfiguration?
        +TestCases: List<TestCase>
    }

    class ExaminerQuiz {
        +Id: int
        +Title: string
        +Code: string
        +AttemptsCount: int
        +SubmittedAttemptsCount: int
        +AverageAttemptScore: float
        +Questions: List<QuestionDTO>
    }

    class ExamineeQuiz {
        +Id: int
        +Title: string
        +Code: string
        +Examiner: UserDTO
        +Questions: List<QuestionDTO>
    }

    class ExaminerAttempt {
        +Id: int
        +StartTime: DateTime
        +EndTime: DateTime?
        +Examinee: UserDTO
        +Solutions: List<SolutionDTO>
    }

    class ExamineeAttempt {
        +Id: int
        +StartTime: DateTime
        +EndTime: DateTime?
        +Quiz: ExamineeQuiz
        +Solutions: List<SolutionDTO>
    }

    class QuestionDTO {
        +Id: int
        +Statement: string
        +EditorCode: string
        +Order: int
        +Points: float
        +QuestionConfiguration: QuestionConfiguration
        +TestCases: List<TestCase>
    }

    class SolutionDTO {
        +Id: int
        +Code: string
        +QuestionId: int
        +AttemptId: int
        +EvaluatedBy: string?
        +ReceivedGrade: float?
        +EvaluationResults: List<EvaluationResult>?
    }
}

package "Services" {
    interface IQuizzesService {
        +CreateQuiz(model): Task<ExaminerQuiz>
        +UpdateQuiz(quiz): Task<ExaminerQuiz>
        +GetUserQuizzes(userId): Task<List<ExaminerQuiz>>
        +DeleteQuiz(id): Task
        +GetQuizByCode(code): Task<ExamineeQuiz>
        +GetQuizAttempts(quizId): Task<List<ExaminerAttempt>>
    }

    class QuizzesService {
        -dbContext: ApplicationDbContext
        -quizCodeGenerator: QuizCodeGenerator
        +CreateQuiz(model): Task<ExaminerQuiz>
        +UpdateQuiz(quiz): Task<ExaminerQuiz>
        +GetUserQuizzes(userId): Task<List<ExaminerQuiz>>
        +DeleteQuiz(id): Task
        +GetQuizByCode(code): Task<ExamineeQuiz>
        +GetQuizAttempts(quizId): Task<List<ExaminerAttempt>>
    }

    interface IAttemptsService {
        +BeginAttempt(code, examineeId): Task<ExamineeAttempt>
        +SubmitAttempt(attemptId): Task<ExamineeAttempt>
        +UpdateSolution(solution): Task<SolutionDTO>
        +GetExamineeAttempts(examineeId): Task<List<ExamineeAttempt>>
        +EvaluateAttempt(attemptId): Task
    }

    class AttemptsService {
        -dbContext: ApplicationDbContext
        -evaluator: IEvaluator
        -attemptsHubContext: IHubContext<AttemptsHub>
        +BeginAttempt(code, examineeId): Task<ExamineeAttempt>
        +SubmitAttempt(attemptId): Task<ExamineeAttempt>
        +UpdateSolution(solution): Task<SolutionDTO>
        +GetExamineeAttempts(examineeId): Task<List<ExamineeAttempt>>
        +EvaluateAttempt(attemptId): Task
        -GradeAttemptSolutions(attempt): void
        -TestCasesPassed(quiz, question, solution): bool
    }

    class AttemptTimerService {
        -serviceProvider: IServiceProvider
        -attemptsHubContext: IHubContext<AttemptsHub>
        -checkInterval: TimeSpan
        #ExecuteAsync(token): Task
        -AutoSubmitExpiredAttemptsAsync(): Task
    }

    class QuizCodeGenerator {
        +GenerateCode(): string
    }
}

package "Controllers" {
    class QuizzesController {
        -quizzesService: IQuizzesService
        +CreateQuiz(model): ActionResult<ApiResponse<ExaminerQuiz>>
        +UpdateQuiz(id, quiz): ActionResult<ApiResponse<ExaminerQuiz>>
        +GetUserQuizzes(): ActionResult<ApiResponse<List<ExaminerQuiz>>>
        +DeleteQuiz(id): ActionResult<ApiResponse>
        +GetQuizByCode(code): ActionResult<ApiResponse<ExamineeQuiz>>
        +GetQuizAttempts(quizId): ActionResult<ApiResponse<List<ExaminerAttempt>>>
    }

    class AttemptsController {
        -attemptsService: IAttemptsService
        +BeginAttempt(request): ActionResult<ApiResponse<ExamineeAttempt>>
        +SubmitAttempt(attemptId): ActionResult<ApiResponse<ExamineeAttempt>>
        +UpdateSolution(solution): ActionResult<ApiResponse<SolutionDTO>>
        +GetUserAttempts(): ActionResult<ApiResponse<List<ExamineeAttempt>>>
    }
}

package "Hubs" {
    class AttemptsHub {
        +OnConnectedAsync(): Task
        +OnDisconnectedAsync(exception): Task
    }

    class QuizzesHub {
        +OnConnectedAsync(): Task
        +OnDisconnectedAsync(exception): Task
    }
}

' Relationships
IQuizzesService <|.. QuizzesService
IAttemptsService <|.. AttemptsService

QuizzesController --> IQuizzesService
AttemptsController --> IAttemptsService

QuizzesService --> QuizCodeGenerator
AttemptsService --> IEvaluator
AttemptsService --> AttemptsHub

Quiz "1" *-- "*" Question
Quiz "1" *-- "1" QuestionConfiguration
Quiz "1" -- "*" Attempt
Attempt "1" *-- "*" Solution

@enduml
```

---

## 4. Code Execution Module Class Diagram

```plantuml
@startuml Code_Execution_Module
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Models" {
    class TestCase {
        +TestCaseNumber: int
        +Input: List<string>
        +ExpectedOutput: string
    }

    class EvaluationResult {
        +TestCase: TestCase
        +Output: string
        +IsSuccessful: bool
    }

    class CodeRunnerResult {
        +Success: bool
        +Output: string?
        +Error: string?
    }

    class CodeRunnerOptions {
        +Input: List<string>
        +ContainOutput: bool
        +ContainError: bool
    }

    class RunCodeRequest {
        +Language: string
        +Code: string
        +Input: List<string>
        +ContainOutput: bool
        +ContainError: bool
    }

    class SandboxConfiguration {
        +TempCodePath: string
        +TimeoutSeconds: int
        +MemoryLimitBytes: long
        +LanguageConfigs: Dictionary<string, LanguageConfig>
    }

    class SandboxRequest {
        +DockerImage: string
        +Command: string
        +Arguments: string[]
        +CodeFilePath: string
        +ContainerWorkDir: string
        +Input: List<string>
        +TimeoutSeconds: int
        +MemoryLimitBytes: long
        +CpuQuota: long
    }

    class SandboxResult {
        +Success: bool
        +Output: string?
        +Error: string?
        +TimedOut: bool
    }
}

package "Services" {
    interface ICodeRunner {
        +Language: string
        +RunCodeAsync(code, options): Task<CodeRunnerResult>
    }

    interface ICodeRunnerFactory {
        +Create(language): ICodeRunner
    }

    interface IEvaluator {
        +EvaluateAsync(language, code, testCase): Task<EvaluationResult>
    }

    interface IDockerSandbox {
        +ExecuteAsync(request, token): Task<SandboxResult>
    }

    class CSharpCodeRunner {
        +Language: string
        +RunCodeAsync(code, options): Task<CodeRunnerResult>
    }

    class SandboxedCodeRunner {
        -innerRunner: ICodeRunner
        -sandbox: IDockerSandbox
        -config: SandboxConfiguration
        -logger: IAppLogger
        +Language: string
        +RunCodeAsync(code, options): Task<CodeRunnerResult>
    }

    class CodeRunnerFactory {
        -serviceProvider: IServiceProvider
        +Create(language): ICodeRunner
    }

    class Evaluator {
        -codeRunnerFactory: ICodeRunnerFactory
        +EvaluateAsync(language, code, testCase): Task<EvaluationResult>
    }

    class DockerSandbox {
        -dockerClient: DockerClient
        -logger: IAppLogger
        +ExecuteAsync(request, token): Task<SandboxResult>
    }
}

package "Controllers" {
    class ExecutionController {
        -codeRunnerFactory: ICodeRunnerFactory
        +RunCode(request): ActionResult<ApiResponse<CodeRunnerResult>>
    }
}

' Relationships
ICodeRunner <|.. CSharpCodeRunner
ICodeRunnerFactory <|.. CodeRunnerFactory
IEvaluator <|.. Evaluator
IDockerSandbox <|.. DockerSandbox

ExecutionController --> ICodeRunnerFactory
CodeRunnerFactory ..> ICodeRunner : creates

SandboxedCodeRunner --> ICodeRunner : wraps
SandboxedCodeRunner --> IDockerSandbox
SandboxedCodeRunner --> SandboxConfiguration

Evaluator --> ICodeRunnerFactory
Evaluator ..> EvaluationResult : creates

DockerSandbox ..> SandboxRequest : uses
DockerSandbox ..> SandboxResult : returns

EvaluationResult --> TestCase

@enduml
```

---

## 5. MAUI Application Class Diagram

```plantuml
@startuml MAUI_Application
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "ViewModels" {
    abstract class BaseViewModel {
        #OnPropertyChanged(name): void
    }

    class LoginVM {
        +Username: string
        +Password: string
        +LoginCommand: ICommand
        +OpenRegisterPageCommand: ICommand
        -Login(): void
        -OpenRegisterPage(): void
    }

    class RegisterVM {
        +FirstName: string
        +LastName: string
        +Username: string
        +Email: string
        +Password: string
        +RegisterCommand: ICommand
        -Register(): void
    }

    class DashboardVM {
        +LoggedInUser: User
        +LogoutCommand: ICommand
        -Logout(): void
    }

    class CreateQuizVM {
        +QuizTitle: string
        +QuizDurationInMinutes: string
        +AvailableFromDate: DateTime
        +AvailableToDate: DateTime
        +ProgrammingLanguage: string
        +AllowMultipleAttempts: bool
        +QuestionModels: ObservableCollection<NewQuestionModel>
        +AddQuestionCommand: ICommand
        +PublishCommand: ICommand
        +DeleteQuestionCommand: ICommand
        -AddQuestion(): void
        -CreateAndPublishQuiz(): void
        -ValidateUserInput(): List<string>
    }

    class CreatedQuizzesVM {
        +Quizzes: ObservableCollection<ExaminerQuiz>
        +SelectedQuiz: ExaminerQuiz
        +ViewQuizCommand: ICommand
        +DeleteQuizCommand: ICommand
        -LoadQuizzes(): void
    }

    class JoinQuizVM {
        +Attempt: ExamineeAttempt
        +RemainingTime: TimeSpan
        +SelectedQuestion: Question
        +CodeInEditor: string
        +Input: string
        +Output: string
        +SubmitQuizCommand: ICommand
        +RunCommand: ICommand
        +NextQuestionCommand: ICommand
        +PreviousQuestionCommand: ICommand
        -SaveSolution(): void
        -SubmitQuiz(): void
        -Run(): void
        -CalculateRemainingTime(): void
    }

    class JoinedQuizzesVM {
        +Attempts: ObservableCollection<ExamineeAttempt>
        +SelectedAttempt: ExamineeAttempt
        +ContinueAttemptCommand: ICommand
        +ViewGradesCommand: ICommand
        -LoadAttempts(): void
    }

    class ExaminerViewQuizVM {
        +Quiz: ExaminerQuiz
        +Attempts: ObservableCollection<ExaminerAttempt>
        +SelectedAttempt: ExaminerAttempt
        -LoadAttempts(): void
    }
}

package "Repositories" {
    interface IAuthenticationRepository {
        +LoggedInUser: User?
        +Login(model): Task<LoginResult>
        +Register(model): Task<User>
        +Logout(): Task
        +ForgotPassword(model): Task
        +ResetPassword(model): Task
    }

    interface IQuizzesRepository {
        +CreateQuiz(model): Task<ExaminerQuiz>
        +GetUserQuizzes(): Task<List<ExaminerQuiz>>
        +DeleteQuiz(id): Task
        +GetQuizByCode(code): Task<ExamineeQuiz>
        +GetQuizAttempts(quizId): Task<List<ExaminerAttempt>>
    }

    interface IAttemptsRepository {
        +BeginAttempt(request): Task<ExamineeAttempt>
        +GetUserAttempts(): Task<List<ExamineeAttempt>>
        +SubmitAttempt(attemptId): Task<ExamineeAttempt>
        +UpdateSolution(solution): Task<Solution>
        +SubscribeUpdate(callback): void
        +SubscribeCreate(callback): void
    }

    interface IExecutionRepository {
        +RunCode(request): Task<CodeRunnerResult>
    }

    class AuthenticationRepository {
        -authAPI: IAuthAPI
        -tokenService: ITokenService
        +LoggedInUser: User?
        +Login(model): Task<LoginResult>
        +Register(model): Task<User>
        +Logout(): Task
    }

    class QuizzesRepository {
        -quizzesAPI: IQuizzesAPI
        +CreateQuiz(model): Task<ExaminerQuiz>
        +GetUserQuizzes(): Task<List<ExaminerQuiz>>
        +DeleteQuiz(id): Task
    }

    class AttemptsRepository {
        -attemptsAPI: IAttemptsAPI
        -connection: HubConnection
        +BeginAttempt(request): Task<ExamineeAttempt>
        +GetUserAttempts(): Task<List<ExamineeAttempt>>
        +SubmitAttempt(attemptId): Task<ExamineeAttempt>
        +UpdateSolution(solution): Task<Solution>
        -Initialize(): void
    }

    class ExecutionRepository {
        -executionAPI: IExecutionAPI
        +RunCode(request): Task<CodeRunnerResult>
    }
}

package "APIs" {
    interface IAuthAPI {
        +Register(model): Task<ApiResponse<User>>
        +Login(model): Task<ApiResponse<LoginResult>>
        +ResetPassword(model): Task<ApiResponse>
        +ForgetPasswordRequest(model): Task<ApiResponse>
    }

    interface IQuizzesAPI {
        +CreateQuiz(model): Task<ApiResponse<ExaminerQuiz>>
        +GetUserQuizzes(): Task<ApiResponse<List<ExaminerQuiz>>>
        +DeleteQuiz(id): Task<ApiResponse>
        +GetQuizByCode(code): Task<ApiResponse<ExamineeQuiz>>
        +GetQuizAttempts(quizId): Task<ApiResponse<List<ExaminerAttempt>>>
    }

    interface IAttemptsAPI {
        +BeginAttempt(request): Task<ApiResponse<ExamineeAttempt>>
        +GetUserAttempts(): Task<ApiResponse<List<ExamineeAttempt>>>
        +SubmitAttempt(attemptId): Task<ApiResponse<ExamineeAttempt>>
        +UpdateSolution(solution): Task<ApiResponse<Solution>>
    }

    interface IExecutionAPI {
        +RunCode(request): Task<ApiResponse<CodeRunnerResult>>
    }
}

package "Services" {
    interface ITokenService {
        +SaveTokens(model): Task
        +GetAccessToken(): Task<string?>
        +GetRefreshToken(): Task<string?>
        +DeleteSavedTokens(): Task
    }

    class TokenService {
        +SaveTokens(model): Task
        +GetAccessToken(): Task<string?>
        +GetRefreshToken(): Task<string?>
        +DeleteSavedTokens(): Task
    }
}

' Inheritance
BaseViewModel <|-- LoginVM
BaseViewModel <|-- RegisterVM
BaseViewModel <|-- DashboardVM
BaseViewModel <|-- CreateQuizVM
BaseViewModel <|-- CreatedQuizzesVM
BaseViewModel <|-- JoinQuizVM
BaseViewModel <|-- JoinedQuizzesVM
BaseViewModel <|-- ExaminerViewQuizVM

' Interface implementations
IAuthenticationRepository <|.. AuthenticationRepository
IQuizzesRepository <|.. QuizzesRepository
IAttemptsRepository <|.. AttemptsRepository
IExecutionRepository <|.. ExecutionRepository
ITokenService <|.. TokenService

' Dependencies
LoginVM --> IAuthenticationRepository
LoginVM --> ITokenService
RegisterVM --> IAuthenticationRepository
DashboardVM --> IAuthenticationRepository

CreateQuizVM --> IQuizzesRepository
CreateQuizVM --> IAuthenticationRepository
CreatedQuizzesVM --> IQuizzesRepository

JoinQuizVM --> IAttemptsRepository
JoinQuizVM --> IExecutionRepository
JoinedQuizzesVM --> IAttemptsRepository

ExaminerViewQuizVM --> IQuizzesRepository

AuthenticationRepository --> IAuthAPI
AuthenticationRepository --> ITokenService
QuizzesRepository --> IQuizzesAPI
AttemptsRepository --> IAttemptsAPI
ExecutionRepository --> IExecutionAPI

@enduml
```

---

## 6. System Architecture Class Diagram

```plantuml
@startuml System_Architecture
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Presentation Layer" {
    package "MAUI App" {
        class Views <<boundary>>
        class ViewModels <<control>>
        class Repositories <<control>>
        class APIs <<boundary>>
    }
}

package "Application Layer" {
    package "Backend Server" {
        class Controllers <<boundary>>
        class Services <<control>>
        class Hubs <<boundary>>
    }
}

package "Domain Layer" {
    class "Domain Models" <<entity>>
    class DTOs <<entity>>
}

package "Infrastructure Layer" {
    class "Database Context" <<entity>>
    class "Docker Sandbox" <<boundary>>
    class "Email Service" <<boundary>>
}

' Relationships
Views --> ViewModels
ViewModels --> Repositories
Repositories --> APIs
APIs --> Controllers

Controllers --> Services
Controllers --> Hubs
Services --> "Domain Models"
Services --> DTOs
Services --> "Database Context"
Services --> "Docker Sandbox"
Services --> "Email Service"

"Domain Models" --> "Database Context"

@enduml
```

---

## 7. Entity Relationship Diagram

```plantuml
@startuml Entity_Relationship
skinparam linetype ortho

entity "User" as user {
    *Id : string <<PK>>
    --
    FirstName : string
    LastName : string
    UserName : string
    Email : string
    JoinDate : DateTime
    ProfilePicture : string?
    PasswordHash : string
}

entity "RefreshToken" as token {
    *Id : int <<PK>>
    --
    Token : string
    *UserId : string <<FK>>
    ExpiryDate : DateTime
    IsRevoked : bool
}

entity "Quiz" as quiz {
    *Id : int <<PK>>
    --
    Title : string
    StartDate : DateTime
    EndDate : DateTime
    Duration : TimeSpan
    Code : string <<unique>>
    *ExaminerId : string <<FK>>
    AllowMultipleAttempts : bool
    TotalPoints : float
    GlobalQuestionConfiguration : JSON
}

entity "Question" as question {
    *Id : int <<PK>>
    --
    Statement : string
    EditorCode : string
    *QuizId : int <<FK>>
    Order : int
    Points : float
    QuestionConfiguration : JSON?
    TestCases : JSON
}

entity "Attempt" as attempt {
    *Id : int <<PK>>
    --
    StartTime : DateTime
    EndTime : DateTime?
    *QuizId : int <<FK>>
    *ExamineeId : string <<FK>>
}

entity "Solution" as solution {
    *Id : int <<PK>>
    --
    Code : string
    *QuestionId : int <<FK>>
    *AttemptId : int <<FK>>
    EvaluatedBy : string?
    ReceivedGrade : float?
    EvaluationResults : JSON?
}

' Relationships
user ||--o{ token : "has"
user ||--o{ quiz : "creates"
user ||--o{ attempt : "takes"

quiz ||--|{ question : "contains"
quiz ||--o{ attempt : "has"

attempt ||--|{ solution : "contains"
question ||--o{ solution : "answered by"

@enduml
```

---

## 8. Repository Pattern Class Diagram

```plantuml
@startuml Repository_Pattern
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Repositories" {
    interface IUsersRepository {
        +GetUserById(id): Task<User?>
        +SearchUsers(query): Task<List<User>>
        +UpdateUser(user): Task<User>
        +IsUsernameAvailable(username): Task<bool>
    }

    interface IQuizzesRepository {
        +CreateQuiz(quiz): Task<Quiz>
        +GetQuizById(id): Task<Quiz?>
        +GetQuizByCode(code): Task<Quiz?>
        +GetUserQuizzes(userId): Task<List<Quiz>>
        +UpdateQuiz(quiz): Task<Quiz>
        +DeleteQuiz(id): Task
    }

    class UsersRepository {
        -dbContext: ApplicationDbContext
        +GetUserById(id): Task<User?>
        +SearchUsers(query): Task<List<User>>
        +UpdateUser(user): Task<User>
        +IsUsernameAvailable(username): Task<bool>
    }

    class QuizzesRepository {
        -dbContext: ApplicationDbContext
        +CreateQuiz(quiz): Task<Quiz>
        +GetQuizById(id): Task<Quiz?>
        +GetQuizByCode(code): Task<Quiz?>
        +GetUserQuizzes(userId): Task<List<Quiz>>
        +UpdateQuiz(quiz): Task<Quiz>
        +DeleteQuiz(id): Task
    }
}

package "Data" {
    class ApplicationDbContext {
        +Users: DbSet<User>
        +RefreshTokens: DbSet<RefreshToken>
        +Quizzes: DbSet<Quiz>
        +Questions: DbSet<Question>
        +Attempts: DbSet<Attempt>
        +Solutions: DbSet<Solution>
        #OnModelCreating(builder): void
    }
}

' Relationships
IUsersRepository <|.. UsersRepository
IQuizzesRepository <|.. QuizzesRepository

UsersRepository --> ApplicationDbContext
QuizzesRepository --> ApplicationDbContext

@enduml
```

---

## 9. Observer Pattern (SignalR) Class Diagram

```plantuml
@startuml Observer_Pattern_SignalR
skinparam classAttributeIconSize 0
skinparam linetype ortho

package "Backend (Subject)" {
    class AttemptsService {
        -attemptsHubContext: IHubContext<AttemptsHub>
        +BeginAttempt(): Task<ExamineeAttempt>
        +SubmitAttempt(): Task<ExamineeAttempt>
        -NotifyAttemptCreated(attempt): void
        -NotifyAttemptUpdated(attempt): void
    }

    class AttemptTimerService {
        -attemptsHubContext: IHubContext<AttemptsHub>
        -AutoSubmitExpiredAttemptsAsync(): Task
        -NotifyAttemptAutoSubmitted(attempt): void
    }

    class AttemptsHub {
        +OnConnectedAsync(): Task
        +OnDisconnectedAsync(): Task
    }

    interface "IHubContext<AttemptsHub>" as IHubContext {
        +Clients: IHubClients
    }
}

package "MAUI App (Observer)" {
    abstract class BaseObservableRepository<T> {
        #createSubscribers: List<Action<T>>
        #updateSubscribers: List<Action<T>>
        #deleteSubscribers: List<Action<int>>
        +SubscribeCreate(callback): void
        +SubscribeUpdate(callback): void
        +SubscribeDelete(callback): void
        #NotifyCreate(item): void
        #NotifyUpdate(item): void
        #NotifyDelete(id): void
    }

    class AttemptsRepository {
        -connection: HubConnection
        -Initialize(): void
        +BeginAttempt(): Task<ExamineeAttempt>
        +SubmitAttempt(): Task<ExamineeAttempt>
    }

    class JoinQuizVM {
        +Attempt: ExamineeAttempt
        +CheckAndUpdate(attempt): void
    }

    class JoinedQuizzesVM {
        +Attempts: ObservableCollection<ExamineeAttempt>
        +OnAttemptUpdated(attempt): void
    }
}

' Relationships
AttemptsService --> IHubContext
AttemptTimerService --> IHubContext
IHubContext --> AttemptsHub

BaseObservableRepository <|-- AttemptsRepository

AttemptsRepository ..> JoinQuizVM : notifies
AttemptsRepository ..> JoinedQuizzesVM : notifies

note right of AttemptsHub
  SignalR Hub broadcasts events:
  - AttemptCreated
  - AttemptUpdated
  - AttemptAutoSubmitted
  - AttemptDeleted
end note

@enduml
```

---

## 10. Factory Pattern (Code Runner) Class Diagram

```plantuml
@startuml Factory_Pattern_CodeRunner
skinparam classAttributeIconSize 0
skinparam linetype ortho

interface ICodeRunner {
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

interface ICodeRunnerFactory {
    +Create(language): ICodeRunner
}

class CodeRunnerFactory {
    -serviceProvider: IServiceProvider
    -sandboxConfig: SandboxConfiguration
    +Create(language): ICodeRunner
}

class CSharpCodeRunner {
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

class SandboxedCodeRunner {
    -innerRunner: ICodeRunner
    -sandbox: IDockerSandbox
    -config: SandboxConfiguration
    -logger: IAppLogger
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

class ExecutionController {
    -codeRunnerFactory: ICodeRunnerFactory
    +RunCode(request): ActionResult
}

class Evaluator {
    -codeRunnerFactory: ICodeRunnerFactory
    +EvaluateAsync(language, code, testCase): Task<EvaluationResult>
}

' Relationships
ICodeRunnerFactory <|.. CodeRunnerFactory
ICodeRunner <|.. CSharpCodeRunner
ICodeRunner <|.. SandboxedCodeRunner

CodeRunnerFactory ..> ICodeRunner : creates
SandboxedCodeRunner o-- ICodeRunner : wraps

ExecutionController --> ICodeRunnerFactory
Evaluator --> ICodeRunnerFactory

note bottom of CodeRunnerFactory
  Factory creates appropriate
  CodeRunner based on language.
  Wraps with SandboxedCodeRunner
  for secure execution.
end note

@enduml
```

---

## 11. Decorator Pattern (Sandboxed Code Runner) Class Diagram

```plantuml
@startuml Decorator_Pattern_CodeRunner
skinparam classAttributeIconSize 0
skinparam linetype ortho

interface ICodeRunner {
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note top of ICodeRunner
  Component Interface:
  Defines the common interface
  for all code runners
end note

class CSharpCodeRunner {
    +Language: string = "CSharp"
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note bottom of CSharpCodeRunner
  Concrete Component:
  Basic implementation
  without decoration
end note

abstract class CodeRunnerDecorator {
    #innerRunner: ICodeRunner
    +Language: string
    +{abstract} RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note right of CodeRunnerDecorator
  Base Decorator:
  Abstract class maintaining 
  reference to wrapped component.
  To be implemented for
  additional decorators.
end note

class SandboxedCodeRunner {
    -innerRunner: ICodeRunner
    -sandbox: IDockerSandbox
    -config: SandboxConfiguration
    -logger: IAppLogger
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
    -PrepareCodeFile(code): string
    -ExecuteInSandbox(filePath, options): Task<SandboxResult>
    -CleanupTempFiles(filePath): void
}

note bottom of SandboxedCodeRunner
  Concrete Decorator:
  Adds sandboxing behavior
  - Executes code in Docker container
  - Enforces resource limits
  - Handles timeouts
  - Cleans up temp files
end note

interface IDockerSandbox {
    +ExecuteAsync(request, token): Task<SandboxResult>
}

class DockerSandbox {
    -dockerClient: DockerClient
    -logger: IAppLogger
    +ExecuteAsync(request, token): Task<SandboxResult>
    -CreateContainer(request): Task<string>
    -StartContainer(containerId): Task
    -WaitForCompletion(containerId, timeout): Task<bool>
    -GetOutput(containerId): Task<string>
    -RemoveContainer(containerId): Task
}

class SandboxConfiguration {
    +TempCodePath: string
    +TimeoutSeconds: int
    +MemoryLimitBytes: long
    +CpuQuota: long
    +LanguageConfigs: Dictionary<string, LanguageConfig>
}

class LanguageConfig {
    +DockerImage: string
    +FileExtension: string
    +Command: string
    +PrepareCode(code): string
    +GetArguments(fileName): string[]
}

' Interface implementations
ICodeRunner <|.. CSharpCodeRunner
ICodeRunner <|.. SandboxedCodeRunner

' Decorator hierarchy (SandboxedCodeRunner acts as decorator)
CodeRunnerDecorator <|-- SandboxedCodeRunner : extends (future)

' Composition relationships
SandboxedCodeRunner o--> ICodeRunner : wraps

' Dependencies
SandboxedCodeRunner --> IDockerSandbox
SandboxedCodeRunner --> SandboxConfiguration
IDockerSandbox <|.. DockerSandbox
SandboxConfiguration "1" *-- "*" LanguageConfig

@enduml
```

### Decorator Pattern Explanation

The **Decorator Pattern** is used in the Code Execution module to dynamically add responsibilities to code runners without modifying their structure.

#### Pattern Components:

| Component | Class | Responsibility |
|-----------|-------|----------------|
| **Component Interface** | `ICodeRunner` | Defines the interface for objects that can have responsibilities added dynamically |
| **Concrete Component** | `CSharpCodeRunner` | Basic implementation that executes code directly |
| **Base Decorator** | `CodeRunnerDecorator` | Abstract class maintaining a reference to a component object (to be implemented) |
| **Concrete Decorator** | `SandboxedCodeRunner` | Adds sandboxing behavior to the wrapped component |

#### Benefits in CodeQuiz:

1. **Single Responsibility**: Each decorator handles one concern (sandboxing)
2. **Open/Closed Principle**: New behaviors can be added without modifying existing code
3. **Flexible Composition**: Decorators can be stacked in the future
4. **Runtime Configuration**: Decoration can be applied conditionally based on configuration

#### Current Implementation:

```csharp
// Create base runner
ICodeRunner baseRunner = new CSharpCodeRunner();

// Wrap with sandbox decorator
ICodeRunner sandboxedRunner = new SandboxedCodeRunner(
    baseRunner, 
    dockerSandbox, 
    sandboxConfig, 
    logger
);

// Execute code - sandbox decorator is applied transparently
var result = await sandboxedRunner.RunCodeAsync(code, options);
```

---

## 12. Strategy Pattern (Code Runner) Class Diagram

```plantuml
@startuml Strategy_Pattern_CodeRunner
skinparam classAttributeIconSize 0
skinparam linetype ortho

interface ICodeRunner {
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note top of ICodeRunner
  Strategy Interface:
  Defines the contract for
  all code execution strategies
end note

class CSharpCodeRunner {
    +Language: string = "CSharp"
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note bottom of CSharpCodeRunner
  Concrete Strategy:
  Executes C# code
end note

class SandboxedCodeRunner {
    -innerRunner: ICodeRunner
    -sandbox: IDockerSandbox
    -config: SandboxConfiguration
    -logger: IAppLogger
    +Language: string
    +RunCodeAsync(code, options): Task<CodeRunnerResult>
}

note bottom of SandboxedCodeRunner
  Concrete Strategy:
  Wraps another strategy
  with sandboxing
end note

class Evaluator {
    -codeRunnerFactory: ICodeRunnerFactory
    +EvaluateAsync(language, code, testCase): Task<EvaluationResult>
}

note right of Evaluator
  Context:
  Uses ICodeRunner strategy
  to execute and evaluate code
end note

class ExecutionController {
    -codeRunnerFactory: ICodeRunnerFactory
    +RunCode(request): ActionResult<ApiResponse<CodeRunnerResult>>
}

note right of ExecutionController
  Context:
  Uses ICodeRunner strategy
  to run user code
end note

class CodeRunnerFactory {
    -serviceProvider: IServiceProvider
    +Create(language): ICodeRunner
}

note bottom of CodeRunnerFactory
  Factory:
  Creates appropriate strategy
  based on language parameter
end note

' Interface implementations
ICodeRunner <|.. CSharpCodeRunner
ICodeRunner <|.. SandboxedCodeRunner

' Context uses Strategy
Evaluator --> ICodeRunner : uses
ExecutionController --> ICodeRunner : uses

' Factory creates strategies
CodeRunnerFactory ..> ICodeRunner : creates
Evaluator --> CodeRunnerFactory
ExecutionController --> CodeRunnerFactory

@enduml
```

### Strategy Pattern Explanation

The **Strategy Pattern** is used in the Code Execution module to define a family of algorithms (code runners), encapsulate each one, and make them interchangeable.

#### Pattern Components:

| Component | Class | Responsibility |
|-----------|-------|----------------|
| **Strategy Interface** | `ICodeRunner` | Declares the interface common to all supported algorithms |
| **Concrete Strategies** | `CSharpCodeRunner`, `SandboxedCodeRunner` | Implement the algorithm using the Strategy interface |
| **Context** | `Evaluator`, `ExecutionController` | Uses a Strategy to execute code |
| **Factory** | `CodeRunnerFactory` | Creates the appropriate strategy at runtime |

#### How It's Used in CodeQuiz:

**1. ExecutionController - Running User Code:**
```csharp
[HttpPost("run")]
public async Task<ActionResult<ApiResponse<CodeRunnerResult>>> RunCode([FromBody] RunCodeRequest request)
{
    // Factory selects the appropriate strategy based on language
    var codeRunner = codeRunnerFactory.Create(request.Language);
    
    // Strategy is executed - caller doesn't know which implementation
    var result = await codeRunner.RunCodeAsync(request.Code, new CodeRunnerOptions
    {
        Input = request.Input,
        ContainOutput = request.ContainOutput,
        ContainError = request.ContainError
    });
    
    return Ok(new ApiResponse<CodeRunnerResult> { Success = true, Data = result });
}
```

**2. Evaluator - Evaluating Solutions Against Test Cases:**
```csharp
public async Task<EvaluationResult> EvaluateAsync(string language, string code, TestCase testCase)
{
    // Factory creates the appropriate strategy
    var codeRunner = codeRunnerFactory.Create(language);
    
    // Execute using the strategy
    var result = await codeRunner.RunCodeAsync(code, new CodeRunnerOptions
    {
        Input = testCase.Input,
        ContainOutput = true,
        ContainError = true
    });

    return new EvaluationResult
    {
        TestCase = testCase,
        Output = result.Output ?? string.Empty,
        IsSuccessful = result.Success && (result.Output?.Trim() == testCase.ExpectedOutput.Trim())
    };
}
```

**3. AttemptsService - Grading Solutions:**
```csharp
private bool TestCasesPassed(Quiz quiz, Question question, Solution solution)
{
    var questionConfig = question.QuestionConfiguration ?? quiz.GlobalQuestionConfiguration;
    
    foreach (var testCase in question.TestCases)
    {
        // Evaluator uses the strategy pattern internally
        var result = evaluator.EvaluateAsync(questionConfig.Language, solution.Code, testCase).Result;
        if (!result.IsSuccessful)
        {
            return false;
        }
    }
    return true;
}
```

#### Benefits in CodeQuiz:

1. **Language Independence**: The system can support multiple programming languages by adding new `ICodeRunner` implementations
2. **Runtime Selection**: The factory selects the appropriate strategy at runtime based on the quiz configuration
3. **Easy Extension**: Adding support for a new language (e.g., Python, Java) requires only implementing `ICodeRunner`
4. **Testability**: Strategies can be mocked for unit testing
5. **Separation of Concerns**: Each language's execution logic is encapsulated in its own class

#### Future Extension Example:

```csharp
// Adding Python support - just implement the interface
public class PythonCodeRunner : ICodeRunner
{
    public string Language => "Python";
    
    public async Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null)
    {
        // Python-specific execution logic
    }
}

// Register in DI container
services.AddTransient<PythonCodeRunner>();

// Factory automatically handles it based on language parameter
var runner = codeRunnerFactory.Create("Python"); // Returns PythonCodeRunner
```

---

## Class Diagram Summary Table

| Diagram | Description | Key Classes |
|---------|-------------|-------------|
| 1. Domain Model Overview | Core business entities | Quiz, Question, Attempt, Solution, User |
| 2. Authentication Module | User authentication & authorization | User, JWTAuthenticationService, TokenService |
| 3. Quiz Management Module | Quiz CRUD and attempt management | Quiz, QuizzesService, AttemptsService |
| 4. Code Execution Module | Code running and evaluation | ICodeRunner, Evaluator, DockerSandbox |
| 5. MAUI Application | Client-side architecture | ViewModels, Repositories, APIs |
| 6. System Architecture | Layered architecture overview | Presentation, Application, Domain, Infrastructure |
| 7. Entity Relationship | Database schema | All entities with relationships |
| 8. Repository Pattern | Data access abstraction | IUsersRepository, IQuizzesRepository |
| 9. Observer Pattern | Real-time updates with SignalR | AttemptsHub, BaseObservableRepository |
| 10. Factory Pattern | Code runner creation | ICodeRunnerFactory, CodeRunnerFactory |
| 11. Decorator Pattern | Sandboxed code execution | SandboxedCodeRunner, CodeRunnerDecorator |
| 12. Strategy Pattern | Interchangeable code execution | ICodeRunner, CSharpCodeRunner, Evaluator |

---

## How to Render PlantUML Diagrams
