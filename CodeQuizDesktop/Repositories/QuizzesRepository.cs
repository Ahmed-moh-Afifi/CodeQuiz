using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Resources;
using Microsoft.AspNetCore.SignalR.Client;

namespace CodeQuizDesktop.Repositories;

public class QuizzesRepository : BaseTwoTypesObservableRepository<ExaminerQuiz, ExamineeQuiz>, IQuizzesRepository
{
    private readonly IQuizzesAPI quizzesAPI;
    private readonly IAuthenticationRepository _authRepository;
    private HubConnection? _connection;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private string? _currentUserId;

    public QuizzesRepository(IQuizzesAPI quizzesAPI, IAuthenticationRepository authRepository)
    {
        this.quizzesAPI = quizzesAPI;
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
                            await _connection.InvokeAsync("LeaveExaminerGroup", _currentUserId);
                        }
                        catch { /* Ignore errors when leaving groups */ }
                    }
                    await _connection.DisposeAsync();
                }

                _connection = new HubConnectionBuilder()
                    .WithUrl($"{Config.HUB}/Quizzes")
                    .WithAutomaticReconnect()
                    .Build();
                    
                _connection.On<ExaminerQuiz, ExamineeQuiz>("QuizCreated", (erq, eeq) => 
                {
                    NotifyCreate(erq);
                    NotifyCreate(eeq);
                });
                _connection.On<ExaminerQuiz, ExamineeQuiz>("QuizUpdated", (erq, eeq) => 
                {
                    NotifyUpdate(erq);
                    NotifyUpdate(eeq);
                });
                _connection.On<int>("QuizDeleted", id =>
                {
                    NotifyDelete<ExaminerQuiz>(id);
                    NotifyDelete<ExamineeQuiz>(id);
                });
                
                await _connection.StartAsync();
                
                // Join examiner group for the logged-in user
                // This ensures they only receive updates for their own quizzes
                if (userId != null)
                {
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

    public async Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel)
    {
        try
        {
            await EnsureInitializedAsync();
            var quiz = (await quizzesAPI.CreateQuiz(newQuizModel)).Data!;
            NotifyCreate(quiz!);
            return quiz;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to create quiz.");
        }
    }

    public async Task DeleteQuiz(int quizId)
    {
        try
        {
            await EnsureInitializedAsync();
            await quizzesAPI.DeleteQuiz(quizId);
            NotifyDelete<ExaminerQuiz>(quizId);
            NotifyDelete<ExamineeQuiz>(quizId);
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to delete quiz.");
        }
    }

    public async Task<ExamineeQuiz> GetQuizByCode(string code)
    {
        try
        {
            await EnsureInitializedAsync();
            return (await quizzesAPI.GetQuizByCode(code)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to find quiz.");
        }
    }

    public async Task<List<ExaminerQuiz>> GetUserQuizzes()
    {
        try
        {
            await EnsureInitializedAsync();
            return (await quizzesAPI.GetUserQuizzes()).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to load quizzes.");
        }
    }

    public async Task<List<ExaminerQuiz>> GetUserQuizzes(string userId)
    {
        try
        {
            await EnsureInitializedAsync();
            return (await quizzesAPI.GetUserQuizzes(userId)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to load quizzes.");
        }
    }

    public async Task<ExaminerQuiz> UpdateQuiz(int quizId, NewQuizModel newQuizModel)
    {
        try
        {
            await EnsureInitializedAsync();
            var qz = (await quizzesAPI.UpdateQuiz(quizId, newQuizModel)).Data!;
            // Get ExamineeQuiz counterpart and notify update
            var examineeQuiz = (await quizzesAPI.GetQuizByCode(qz.Code)).Data!;
            if (examineeQuiz.Id == qz.Id)
            {
                NotifyUpdate(examineeQuiz!);
            }
            NotifyUpdate(qz!);
            return qz;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to update quiz.");
        }
    }

    public async Task<List<ExaminerAttempt>> GetQuizAttempts(int quizId)
    {
        try
        {
            await EnsureInitializedAsync();
            return (await quizzesAPI.GetQuizAttempts(quizId)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to load quiz attempts.");
        }
    }
}
