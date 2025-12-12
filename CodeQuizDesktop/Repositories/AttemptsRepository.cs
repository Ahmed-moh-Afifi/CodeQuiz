using CodeQuizDesktop.APIs;
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
        public AttemptsRepository(IAttemptsAPI attemptsAPI)
        {
            this.attemptsAPI = attemptsAPI;
            Initialize();
        }

        public async void Initialize()
        {
            var connection = new HubConnectionBuilder().WithUrl($"{Config.HUB}/Attempts").WithAutomaticReconnect().Build();
            connection.On<ExamineeAttempt>("AttemptAutoSubmitted", NotifyUpdate);
            connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptCreated", (era, eea) => NotifyCreate(eea));
            connection.On<ExaminerAttempt, ExamineeAttempt>("AttemptUpdated", (era, eea) => NotifyUpdate(eea));
            connection.On<int>("AttemptDeleted", NotifyDelete);
            await connection.StartAsync();
        }

        public async Task<ExamineeAttempt> BeginAttempt(BeginAttemptRequest beginAttemptRequest)
        {
            try
            {
                var attempt = (await attemptsAPI.BeginAttempt(beginAttemptRequest)).Data!;
                NotifyCreate(attempt);
                return attempt;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<List<ExamineeAttempt>> GetUserAttempts()
        {
            try
            {
                return (await attemptsAPI.GetUserAttempts()).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<ExamineeAttempt> SubmitAttempt(int attemptId)
        {
            try
            {
                var attempt = (await attemptsAPI.SubmitAttempt(attemptId)).Data!;
                NotifyUpdate(attempt);
                return attempt;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }

        public async Task<Solution> UpdateSolution(Solution solution)
        {
            try
            {
                return (await attemptsAPI.UpdateSolution(solution)).Data!;
            }
            catch (Exception)
            {
                // Log exception here...
                throw; // Replace with a custom exception
            }
        }
    }
}
