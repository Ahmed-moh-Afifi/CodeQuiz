namespace CodeQuizBackend.Services
{
    public interface IMailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <param name="isHtml">Whether the body is HTML formatted</param>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Sends welcome email to a newly registered user.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="firstName">User's first name</param>
        Task SendWelcomeEmailAsync(string toEmail, string firstName);

        /// <summary>
        /// Sends password reset email with a reset link.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="resetLink">Password reset link</param>
        Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink);

        /// <summary>
        /// Sends attempt feedback email with grades and details.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="quizTitle">Title of the quiz</param>
        /// <param name="examineeGrade">Grade received by the examinee</param>
        /// <param name="totalGrade">Total possible grade</param>
        /// <param name="startTime">Attempt start time</param>
        /// <param name="finishTime">Attempt finish time</param>
        Task SendAttemptFeedbackAsync(string toEmail, string firstName, string quizTitle, float examineeGrade, float totalGrade, DateTime startTime, DateTime finishTime);

        /// <summary>
        /// Sends quiz completion summary email to the examiner when a quiz becomes unavailable.
        /// </summary>
        /// <param name="toEmail">Examiner's email address</param>
        /// <param name="examinerName">Examiner's first name</param>
        /// <param name="stats">Quiz statistics summary</param>
        Task SendQuizEndSummaryAsync(string toEmail, string examinerName, QuizEndStatistics stats);
    }

    /// <summary>
    /// Statistics for a quiz that has ended.
    /// </summary>
    public class QuizEndStatistics
    {
        public required string QuizTitle { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required int TotalAttempts { get; set; }
        public required int SubmittedAttempts { get; set; }
        public required int InProgressAttempts { get; set; }
        public required float AverageGrade { get; set; }
        public required float HighestGrade { get; set; }
        public required float LowestGrade { get; set; }
        public required float PassRate { get; set; }
        public required int TotalAiAssessments { get; set; }
        public required int FlaggedSolutions { get; set; }
        public required float TotalPoints { get; set; }
    }
}
