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
    public class AttemptsRepository : BaseObservableRepository<ExamineeAttempt>, IAttemptsRepository
    {
        private readonly IAttemptsAPI attemptsAPI;
        private readonly IAuthenticationRepository _authRepository;
        private HubConnection? _connection;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private string? _currentUserId;
        
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
                            }
                            catch { /* Ignore errors when leaving groups */ }
                        }
                        await _connection.DisposeAsync();
                    }

                    _connection = new HubConnectionBuilder()
                        .WithUrl($"{Config.HUB}/Attempts")
                        .WithAutomaticReconnect()
                        .Build();
                        
                    _connection.On<ExamineeAttempt>("AttemptAutoSubmitted", NotifyUpdate);
                    _connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptCreated", (era, eea) => NotifyCreate(eea));
                    _connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptUpdated", (era, eea) => NotifyUpdate(eea));
                    _connection.On<int>("AttemptDeleted", NotifyDelete);
                    
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
    }
}
