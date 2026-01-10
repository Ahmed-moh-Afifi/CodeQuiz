using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Resources;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class AttemptsRepository : BaseTwoTypesObservableRepository<ExaminerAttempt, ExamineeAttempt>, IAttemptsRepository
    {
        private readonly IAttemptsAPI attemptsAPI;
        private readonly IAuthenticationRepository _authRepository;
        private HubConnection? _connection;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private string? _currentUserId;
        private readonly HashSet<int> _joinedQuizGroups = [];
        
        public AttemptsRepository(IAttemptsAPI attemptsAPI, IAuthenticationRepository authRepository)
        {
            this.attemptsAPI = attemptsAPI;
            this._authRepository = authRepository;
        }

        private async Task EnsureInitializedAsync()
        {
            var userId = _authRepository.LoggedInUser?.Id;
            
            // If user changed or not connected, reinitialize
            if (_currentUserId != userId || _connection?.State != HubConnectionState.Connected)
            {
                await _initLock.WaitAsync();
                try
                {
                    // Double-check after acquiring lock
                    if (_currentUserId == userId && _connection?.State == HubConnectionState.Connected)
                    {
                        return;
                    }

                    // Clean up old connection
                    if (_connection != null)
                    {
                        // Leave old groups if we were in them
                        if (_currentUserId != null && _connection.State == HubConnectionState.Connected)
                        {
                            try
                            {
                                await _connection.InvokeAsync("LeaveUserGroup", _currentUserId);
                                await _connection.InvokeAsync("LeaveExaminerGroup", _currentUserId);
                                
                                // Leave all joined quiz groups
                                foreach (var quizId in _joinedQuizGroups)
                                {
                                    await _connection.InvokeAsync("LeaveQuizGroup", quizId);
                                }
                            }
                            catch { /* Ignore errors when leaving groups */ }
                        }
                        await _connection.DisposeAsync();
                        _joinedQuizGroups.Clear();
                    }

                    _connection = new HubConnectionBuilder()
                        .WithUrl($"{Config.HUB}/Attempts")
                        .WithAutomaticReconnect()
                        .Build();
                    
                    // Handle reconnection to re-join groups
                    _connection.Reconnected += OnReconnectedAsync;
                        
                    // Handle both ExaminerAttempt and ExamineeAttempt notifications
                    _connection.On<ExamineeAttempt>("AttemptAutoSubmitted", eea =>
                    {
                        NotifyUpdate(eea);
                    });
                    
                    _connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptCreated", (era, eea) =>
                    {
                        NotifyCreate(era);
                        NotifyCreate(eea);
                    });
                    
                    _connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptUpdated", (era, eea) =>
                    {
                        NotifyUpdate(era);
                        NotifyUpdate(eea);
                    });
                    
                    _connection.On<int>("AttemptDeleted", id =>
                    {
                        NotifyDelete<ExaminerAttempt>(id);
                        NotifyDelete<ExamineeAttempt>(id);
                    });
                    
                    await _connection.StartAsync();
                    
                    // Join user-specific and examiner groups for the logged-in user
                    if (userId != null)
                    {
                        await _connection.InvokeAsync("JoinUserGroup", userId);
                        await _connection.InvokeAsync("JoinExaminerGroup", userId);
                        _currentUserId = userId;
                    }
                    
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize SignalR connection: {ex.Message}");
                    // Don't throw - allow operations to continue without real-time updates
                }
                finally
                {
                    _initLock.Release();
                }
            }
        }
        
        /// <summary>
        /// Handles reconnection by re-joining all groups
        /// </summary>
        private async Task OnReconnectedAsync(string? connectionId)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR reconnected with connection ID: {connectionId}");
            
            try
            {
                if (_currentUserId != null)
                {
                    await _connection!.InvokeAsync("JoinUserGroup", _currentUserId);
                    await _connection!.InvokeAsync("JoinExaminerGroup", _currentUserId);
                    
                    // Re-join all quiz groups
                    foreach (var quizId in _joinedQuizGroups)
                    {
                        await _connection!.InvokeAsync("JoinQuizGroup", quizId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to re-join groups after reconnection: {ex.Message}");
            }
        }

        /// <summary>
        /// Join a quiz-specific SignalR group to receive real-time updates for attempts in that quiz.
        /// Used by examiners viewing a specific quiz's attempts.
        /// </summary>
        public async Task JoinQuizGroupAsync(int quizId)
        {
            await EnsureInitializedAsync();
            
            if (_connection?.State == HubConnectionState.Connected && !_joinedQuizGroups.Contains(quizId))
            {
                try
                {
                    await _connection.InvokeAsync("JoinQuizGroup", quizId);
                    _joinedQuizGroups.Add(quizId);
                    System.Diagnostics.Debug.WriteLine($"Joined quiz group: quiz_{quizId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to join quiz group {quizId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Leave a quiz-specific SignalR group when no longer viewing that quiz's attempts.
        /// </summary>
        public async Task LeaveQuizGroupAsync(int quizId)
        {
            if (_connection?.State == HubConnectionState.Connected && _joinedQuizGroups.Contains(quizId))
            {
                try
                {
                    await _connection.InvokeAsync("LeaveQuizGroup", quizId);
                    _joinedQuizGroups.Remove(quizId);
                    System.Diagnostics.Debug.WriteLine($"Left quiz group: quiz_{quizId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to leave quiz group {quizId}: {ex.Message}");
                }
            }
        }

        public async Task<ExamineeAttempt> BeginAttempt(BeginAttemptRequest beginAttemptRequest)
        {
            try
            {
                await EnsureInitializedAsync();
                var attempt = (await attemptsAPI.BeginAttempt(beginAttemptRequest)).Data!;
                NotifyCreate(attempt);
                return attempt;
            }
            catch (Exception ex)
            {
                throw ApiServiceException.FromException(ex, "Failed to begin attempt.");
            }
        }

        public async Task<List<ExamineeAttempt>> GetUserAttempts()
        {
            try
            {
                await EnsureInitializedAsync();
                return (await attemptsAPI.GetUserAttempts()).Data!;
            }
            catch (Exception ex)
            {
                throw ApiServiceException.FromException(ex, "Failed to load attempts.");
            }
        }

        public async Task<ExamineeAttempt> SubmitAttempt(int attemptId)
        {
            try
            {
                await EnsureInitializedAsync();
                var attempt = (await attemptsAPI.SubmitAttempt(attemptId)).Data!;
                NotifyUpdate(attempt);
                return attempt;
            }
            catch (Exception ex)
            {
                throw ApiServiceException.FromException(ex, "Failed to submit attempt.");
            }
        }

        public async Task<Solution> UpdateSolution(Solution solution)
        {
            try
            {
                return (await attemptsAPI.UpdateSolution(solution)).Data!;
            }
            catch (Exception ex)
            {
                throw ApiServiceException.FromException(ex, "Failed to save solution.");
            }
        }

        /// <summary>
        /// Updates a solution's grade (instructor grading)
        /// </summary>
        public async Task<Solution> UpdateSolutionGrade(UpdateSolutionGradeRequest request)
        {
            try
            {
                return (await attemptsAPI.UpdateSolutionGrade(request)).Data!;
            }
            catch (Exception ex)
            {
                throw ApiServiceException.FromException(ex, "Failed to update solution grade.");
            }
        }
    }
}
