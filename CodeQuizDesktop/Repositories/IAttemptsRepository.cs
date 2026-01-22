using CodeQuizDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IAttemptsRepository : ITwoTypesObservableRepository<ExaminerAttempt, ExamineeAttempt>
    {
        public Task<ExamineeAttempt> BeginAttempt(BeginAttemptRequest beginAttemptRequest);
        public Task<ExamineeAttempt> SubmitAttempt(int attemptId);
        /// <summary>
        /// Updates a solution's code (student saving their work)
        /// </summary>
        public Task<Solution> UpdateSolution(Solution solution);
        /// <summary>
        /// Updates a solution's grade (instructor grading)
        /// </summary>
        public Task<Solution> UpdateSolutionGrade(UpdateSolutionGradeRequest request);
        public Task<List<ExamineeAttempt>> GetUserAttempts();

        /// <summary>
        /// Join a quiz-specific SignalR group to receive real-time updates for attempts in that quiz.
        /// Used by examiners viewing a specific quiz's attempts.
        /// </summary>
        Task JoinQuizGroupAsync(int quizId);

        /// <summary>
        /// Leave a quiz-specific SignalR group when no longer viewing that quiz's attempts.
        /// </summary>
        Task LeaveQuizGroupAsync(int quizId);

        /// <summary>
        /// Subscribe to evaluation status updates (EvaluationStarted, SystemGradingComplete, AiAssessmentComplete, EvaluationFailed).
        /// </summary>
        void SubscribeEvaluationStatus(Action<EvaluationStatusPayload> action);

        /// <summary>
        /// Unsubscribe from evaluation status updates.
        /// </summary>
        void UnsubscribeEvaluationStatus(Action<EvaluationStatusPayload> action);
    }
}
