# CodeQuiz - UML Sequence Diagrams

## 1. User Registration Sequence Diagram

```mermaid
sequenceDiagram
    actor User
    participant App as MAUI App
    participant Backend as Backend Server
    participant DB as Database

    User->>App: Enter registration details
    App->>Backend: POST /api/Authentication/Register
    Backend->>Backend: Validate input
    Backend->>DB: Create user record
    DB-->>Backend: User created
    Backend-->>App: UserDTO
    App-->>User: Registration successful
```

---

## 2. User Login Sequence Diagram

```mermaid
sequenceDiagram
    actor User
    participant App as MAUI App
    participant Backend as Backend Server
    participant TokenService as Token Service
    participant DB as Database

    User->>App: Enter credentials
    App->>Backend: POST /api/Authentication/Login
    Backend->>DB: Find user by username
    DB-->>Backend: User record
    Backend->>Backend: Verify password
    Backend->>TokenService: Generate access token
    TokenService-->>Backend: Access token
    Backend->>TokenService: Generate refresh token
    TokenService-->>Backend: Refresh token
    Backend->>DB: Store refresh token
    Backend-->>App: LoginResult (User + Tokens)
    App->>App: Save tokens locally
    App-->>User: Login successful
```

---

## 3. Token Refresh Sequence Diagram

```mermaid
sequenceDiagram
    actor User
    participant App as MAUI App
    participant Backend as Backend Server
    participant TokenService as Token Service
    participant DB as Database

    Note over App: Access token expired
    App->>Backend: POST /api/Authentication/Refresh
    Backend->>TokenService: Validate expired token
    TokenService-->>Backend: Claims extracted
    Backend->>DB: Find refresh token
    DB-->>Backend: Refresh token record
    Backend->>Backend: Validate refresh token
    Backend->>TokenService: Generate new access token
    Backend->>TokenService: Generate new refresh token
    Backend->>DB: Update refresh token
    Backend-->>App: New TokenModel
    App->>App: Save new tokens
    App-->>User: Session continued
```

---

## 4. Create Quiz Sequence Diagram

```mermaid
sequenceDiagram
    actor Examiner
    participant App as MAUI App
    participant Backend as Backend Server
    participant QuizService as Quiz Service
    participant DB as Database

    Examiner->>App: Fill quiz details
    Examiner->>App: Add questions
    Examiner->>App: Configure settings
    Examiner->>App: Click "Publish"
    App->>Backend: POST /api/Quizzes (NewQuizModel)
    Backend->>Backend: Validate quiz data
    Backend->>QuizService: Create quiz
    QuizService->>QuizService: Generate unique quiz code
    QuizService->>DB: Save quiz with questions
    DB-->>QuizService: Quiz saved
    QuizService-->>Backend: ExaminerQuiz
    Backend-->>App: Quiz created response
    App-->>Examiner: Quiz published with code
```

---

## 5. Join Quiz Sequence Diagram

```mermaid
sequenceDiagram
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant QuizService as Quiz Service
    participant DB as Database

    Examinee->>App: Enter quiz code
    App->>Backend: GET /api/Quizzes/code/{code}
    Backend->>QuizService: Get quiz by code
    QuizService->>DB: Find quiz
    DB-->>QuizService: Quiz record
    QuizService->>QuizService: Validate availability period
    QuizService-->>Backend: ExamineeQuiz
    Backend-->>App: Quiz details
    App-->>Examinee: Display quiz info
```

---

## 6. Begin Quiz Attempt Sequence Diagram

```mermaid
sequenceDiagram
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant AttemptService as Attempts Service
    participant SignalR as SignalR Hub
    participant DB as Database

    Examinee->>App: Click "Start Quiz"
    App->>Backend: POST /api/Attempts/begin
    Backend->>AttemptService: Begin attempt
    AttemptService->>DB: Check existing attempts
    DB-->>AttemptService: Attempt status
    alt No active attempt
        AttemptService->>DB: Create new attempt
        AttemptService->>DB: Create empty solutions
        DB-->>AttemptService: Attempt created
    end
    AttemptService->>SignalR: Notify AttemptCreated
    AttemptService-->>Backend: ExamineeAttempt
    Backend-->>App: Attempt details
    App->>App: Start countdown timer
    App-->>Examinee: Display questions
```

---

## 7. Answer Question & Save Solution Sequence Diagram

```mermaid
sequenceDiagram
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant AttemptService as Attempts Service
    participant DB as Database

    Examinee->>App: Write code solution
    Examinee->>App: Navigate to next question
    App->>Backend: PUT /api/Attempts/solutions
    Backend->>AttemptService: Update solution
    AttemptService->>DB: Update solution code
    DB-->>AttemptService: Solution updated
    AttemptService-->>Backend: SolutionDTO
    Backend-->>App: Solution saved
    App-->>Examinee: Continue with quiz
```

---

## 8. Run Code Sequence Diagram

```mermaid
sequenceDiagram
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant CodeRunner as Code Runner
    participant Sandbox as Docker Sandbox

    Examinee->>App: Click "Run"
    App->>Backend: POST /api/Execution/run
    Backend->>CodeRunner: Execute code
    CodeRunner->>CodeRunner: Prepare code file
    CodeRunner->>Sandbox: Execute in container
    Sandbox->>Sandbox: Run with timeout
    Sandbox-->>CodeRunner: Execution result
    CodeRunner->>CodeRunner: Clean up temp files
    CodeRunner-->>Backend: CodeRunnerResult
    Backend-->>App: Output/Error
    App-->>Examinee: Display result
```

---

## 9. Submit Quiz Attempt Sequence Diagram

```mermaid
sequenceDiagram
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant AttemptService as Attempts Service
    participant Evaluator as Evaluator
    participant SignalR as SignalR Hub
    participant DB as Database

    Examinee->>App: Click "Submit"
    App->>Backend: POST /api/Attempts/{id}/submit
    Backend->>AttemptService: Submit attempt
    AttemptService->>DB: Set end time
    AttemptService->>SignalR: Notify AttemptUpdated
    AttemptService->>Evaluator: Evaluate solutions
    loop For each solution
        Evaluator->>Evaluator: Run test cases
        Evaluator->>DB: Update grades
    end
    AttemptService->>SignalR: Notify AttemptUpdated
    AttemptService-->>Backend: ExamineeAttempt
    Backend-->>App: Submission confirmed
    App-->>Examinee: Quiz submitted
```

---

## 10. Auto-Submit on Timeout Sequence Diagram

```mermaid
sequenceDiagram
    participant Timer as Timer Service
    participant AttemptService as Attempts Service
    participant Evaluator as Evaluator
    participant SignalR as SignalR Hub
    participant DB as Database
    participant App as MAUI App
    actor Examinee

    Timer->>Timer: Check every 10 seconds
    Timer->>DB: Find expired attempts
    DB-->>Timer: Expired attempts list
    loop For each expired attempt
        Timer->>DB: Set end time
        Timer->>SignalR: Notify AttemptAutoSubmitted
        SignalR-->>App: AttemptAutoSubmitted event
        App-->>Examinee: Quiz auto-submitted
        Timer->>AttemptService: Evaluate attempt
        AttemptService->>Evaluator: Grade solutions
        Evaluator-->>AttemptService: Grades calculated
        AttemptService->>DB: Save grades
        AttemptService->>SignalR: Notify AttemptUpdated
    end
```

---

## 11. View Quiz Attempts (Examiner) Sequence Diagram

```mermaid
sequenceDiagram
    actor Examiner
    participant App as MAUI App
    participant Backend as Backend Server
    participant QuizService as Quiz Service
    participant DB as Database

    Examiner->>App: Select quiz
    App->>Backend: GET /api/Quizzes/{id}/attempts
    Backend->>QuizService: Get quiz attempts
    QuizService->>DB: Fetch attempts with solutions
    DB-->>QuizService: Attempts list
    QuizService-->>Backend: List<ExaminerAttempt>
    Backend-->>App: Attempts data
    App-->>Examiner: Display attempts & grades
```

---

## 12. Code Evaluation Sequence Diagram

```mermaid
sequenceDiagram
    participant AttemptService as Attempts Service
    participant Evaluator as Evaluator
    participant CodeRunner as Code Runner
    participant Sandbox as Docker Sandbox
    participant DB as Database

    AttemptService->>Evaluator: Evaluate solution
    loop For each test case
        Evaluator->>CodeRunner: Run code with input
        CodeRunner->>Sandbox: Execute in container
        Sandbox-->>CodeRunner: Output
        CodeRunner-->>Evaluator: Result
        Evaluator->>Evaluator: Compare with expected output
    end
    Evaluator-->>AttemptService: All tests passed/failed
    AttemptService->>DB: Update solution grade
```

---

## 13. Real-time Updates via SignalR Sequence Diagram

```mermaid
sequenceDiagram
    participant App1 as Examiner App
    participant App2 as Examinee App
    participant SignalR as SignalR Hub
    participant Backend as Backend Server

    App1->>SignalR: Connect to AttemptsHub
    App2->>SignalR: Connect to AttemptsHub
    
    Note over Backend: Attempt Created/Updated
    Backend->>SignalR: Send AttemptUpdated
    SignalR-->>App1: AttemptUpdated (ExaminerAttempt)
    SignalR-->>App2: AttemptUpdated (ExamineeAttempt)
    App1->>App1: Update UI
    App2->>App2: Update UI
```

---

## 14. Password Reset Sequence Diagram

```mermaid
sequenceDiagram
    actor User
    participant App as MAUI App
    participant Backend as Backend Server
    participant AuthService as Auth Service
    participant Email as Email Service
    participant DB as Database

    User->>App: Request password reset
    App->>Backend: POST /api/Authentication/ForgetPasswordRequest
    Backend->>AuthService: Process request
    AuthService->>DB: Find user by email
    DB-->>AuthService: User record
    AuthService->>AuthService: Generate reset token
    AuthService->>Email: Send reset email
    Email-->>User: Reset link received
    
    User->>App: Enter new password with token
    App->>Backend: PUT /api/Authentication/ResetPasswordTn
    Backend->>AuthService: Reset password
    AuthService->>DB: Update password
    DB-->>AuthService: Password updated
    AuthService-->>Backend: Success
    Backend-->>App: Password reset confirmed
    App-->>User: Password changed
```

---

## 15. Complete Quiz Flow Sequence Diagram (Overview)

```mermaid
sequenceDiagram
    actor Examiner
    actor Examinee
    participant App as MAUI App
    participant Backend as Backend Server
    participant System as System Services

    Note over Examiner,System: Quiz Creation Phase
    Examiner->>App: Create quiz
    App->>Backend: Create quiz with questions
    Backend-->>App: Quiz code generated

    Note over Examiner,System: Quiz Participation Phase
    Examiner-->>Examinee: Share quiz code
    Examinee->>App: Join with code
    App->>Backend: Begin attempt
    Backend-->>App: Attempt started

    Note over Examiner,System: Quiz Taking Phase
    loop For each question
        Examinee->>App: Write solution
        Examinee->>App: Run code (optional)
        App->>Backend: Execute code
        Backend-->>App: Output
        App->>Backend: Save solution
    end

    Note over Examiner,System: Submission Phase
    alt Manual submit
        Examinee->>App: Submit attempt
    else Time expired
        System->>Backend: Auto-submit
    end
    Backend->>System: Evaluate solutions
    System-->>Backend: Grades calculated

    Note over Examiner,System: Review Phase
    Examiner->>App: View attempts
    App->>Backend: Get quiz attempts
    Backend-->>App: Attempts with grades
    Examinee->>App: View grades
    App->>Backend: Get user attempts
    Backend-->>App: Grades & results
```

---

## Sequence Diagram Summary Table

| Diagram | Actors | Primary Flow |
|---------|--------|--------------|
| 1. Registration | User | User creates account |
| 2. Login | User | User authenticates |
| 3. Token Refresh | User | Session renewal |
| 4. Create Quiz | Examiner | Quiz publication |
| 5. Join Quiz | Examinee | Quiz discovery |
| 6. Begin Attempt | Examinee | Start quiz session |
| 7. Save Solution | Examinee | Code persistence |
| 8. Run Code | Examinee | Code execution |
| 9. Submit Attempt | Examinee | Manual submission |
| 10. Auto-Submit | System | Timeout submission |
| 11. View Attempts | Examiner | Progress monitoring |
| 12. Evaluation | System | Automated grading |
| 13. Real-time Updates | All | SignalR notifications |
| 14. Password Reset | User | Account recovery |
| 15. Complete Flow | All | End-to-end overview |
