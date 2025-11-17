namespace CodeQuizBackend.Quiz.Repositories
{
    public interface IQuizzesRepository
    {
        public Task<Models.Quiz> GetQuizByIdAsync(int id);
        public Task<List<Models.Quiz>> GetUserQuizzesAsync(string userId);
        public Task<Models.Quiz> CreateQuizAsync(Models.Quiz quiz);
        public Task<Models.Quiz> UpdateQuizAsync(Models.Quiz quiz);
        public Task DeleteQuizAsync(int id);
        public Task<Models.Quiz> GetQuizByCodeAsync(string code);
    }
}
