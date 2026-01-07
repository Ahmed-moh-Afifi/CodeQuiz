using Microsoft.AspNetCore.SignalR;

namespace CodeQuizBackend.Quiz.Hubs
{
    public class QuizzesHub : Hub
    {
        /// <summary>
        /// Join examiner group to receive updates for quizzes created by this examiner
        /// </summary>
        public async Task JoinExaminerGroup(string examinerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"examiner_{examinerId}");
        }

        /// <summary>
        /// Leave examiner group
        /// </summary>
        public async Task LeaveExaminerGroup(string examinerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"examiner_{examinerId}");
        }

        /// <summary>
        /// Join quiz-specific group to receive updates for a specific quiz (for examinees)
        /// </summary>
        public async Task JoinQuizGroup(int quizId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"quiz_{quizId}");
        }

        /// <summary>
        /// Leave quiz-specific group
        /// </summary>
        public async Task LeaveQuizGroup(int quizId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"quiz_{quizId}");
        }
    }
}
