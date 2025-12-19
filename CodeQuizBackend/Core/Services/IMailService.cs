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
    }
}
