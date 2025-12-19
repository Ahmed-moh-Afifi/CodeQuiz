using CodeQuizDesktop.APIs;
using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Models;
using CodeQuizDesktop.Resources;
using Microsoft.AspNetCore.SignalR.Client;

namespace CodeQuizDesktop.Repositories;

public class QuizzesRepository(IQuizzesAPI quizzesAPI) : BaseTwoTypesObservableRepository<ExaminerQuiz, ExamineeQuiz>, IQuizzesRepository
{
    public async void Initialize()
    {
        var connection = new HubConnectionBuilder().WithUrl($"{Config.HUB}/Attempts").WithAutomaticReconnect().Build();
        connection.On<ExaminerQuiz, ExamineeQuiz>("QuizCreated", (erq, eeq) => NotifyCreate(eeq));
        connection.On<ExaminerQuiz, ExamineeQuiz>("QuizUpdated", (erq, eeq) => NotifyUpdate(eeq));
        await connection.StartAsync();
    }

    public async Task<ExaminerQuiz> CreateQuiz(NewQuizModel newQuizModel)
    {
        try
        {
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
            return (await quizzesAPI.GetQuizAttempts(quizId)).Data!;
        }
        catch (Exception ex)
        {
            throw ApiServiceException.FromException(ex, "Failed to load quiz attempts.");
        }
    }
}
