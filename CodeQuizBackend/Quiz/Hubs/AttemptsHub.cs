using CodeQuizBackend.Quiz.Models.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CodeQuizBackend.Quiz.Hubs
{
    public class AttemptsHub : Hub
    {
        /// <summary>
        /// Join user-specific group to receive updates for own attempts
        /// </summary>
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        /// <summary>
        /// Leave user-specific group
        /// </summary>
        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        /// <summary>
        /// Join quiz-specific group to receive updates for a specific quiz (for examiners)
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

        /// <summary>
        /// Join examiner group to receive updates for all quizzes created by an examiner
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
    }
}
