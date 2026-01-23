using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Core.Logging;
using System.Net;
using System.Net.Mail;

namespace CodeQuizBackend.Services
{
    public class SmtpMailService(IConfiguration configuration, IAppLogger<SmtpMailService> logger) : IMailService
    {
        // CodeQuiz theme colors (dark red theme from MAUI app)
        private const string PrimaryColor = "#591c21";
        private const string PrimaryLightColor = "#70222a";
        private const string BackgroundDark = "#1e1e1e";
        private const string BackgroundDarker = "#171717";
        private const string TextColor = "#D4D4D4";
        private const string MutedTextColor = "#aaaaaa";
        private const string BorderColor = "#3b3b3b";

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var smtpHost = configuration["SMTPHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(configuration["SMTPPort"] ?? "587");
            var smtpUsername = configuration["SMTPUsername"];
            var smtpPassword = configuration["SMTPPassword"];

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                logger.LogError("SMTP credentials are not configured");
                throw new ServiceUnavailableException("Email service is temporarily unavailable. Please try again later.");
            }

            try
            {
                using var mailMessage = new MailMessage(
                    new MailAddress(smtpUsername),
                    new MailAddress(toEmail))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInfo($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to send email to {toEmail}", ex);
                throw new ServiceUnavailableException("Unable to send email at this time. Please try again later.");
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string firstName)
        {
            var subject = "Welcome to CodeQuiz!";
            var body = $"""
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: {TextColor}; margin: 0; padding: 0; background-color: {BackgroundDarker};">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <div style="background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                            <h1 style="color: white; margin: 0; font-size: 28px;">CodeQuiz</h1>
                            <p style="color: {TextColor}; margin: 10px 0 0 0; font-size: 14px;">Challenge Your Coding Skills</p>
                        </div>
                        <div style="background-color: {BackgroundDark}; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid {BorderColor}; border-top: none;">
                            <h2 style="color: {TextColor}; margin-top: 0;">Welcome, {firstName}!</h2>
                            <p style="color: {TextColor};">We're thrilled to have you join the CodeQuiz community! Get ready to:</p>
                            <ul style="color: {TextColor}; padding-left: 20px;">
                                <li style="margin-bottom: 10px;">Test your programming knowledge with exciting quizzes</li>
                                <li style="margin-bottom: 10px;">Compete with other developers and climb the leaderboard</li>
                                <li style="margin-bottom: 10px;">Learn new concepts while having fun</li>
                            </ul>
                            <div style="text-align: center; margin: 30px 0;">
                                <p style="color: {MutedTextColor};">Ready to start your coding journey?</p>
                            </div>
                            <hr style="border: none; border-top: 1px solid {BorderColor}; margin: 20px 0;">
                            <p style="font-size: 12px; color: {MutedTextColor}; text-align: center;">
                                This is an automated message from CodeQuiz. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink)
        {
            var subject = "Reset Your CodeQuiz Password";
            var body = $"""
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: {TextColor}; margin: 0; padding: 0; background-color: {BackgroundDarker};">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <div style="background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                            <h1 style="color: white; margin: 0; font-size: 28px;">CodeQuiz</h1>
                            <p style="color: {TextColor}; margin: 10px 0 0 0; font-size: 14px;">Password Reset Request</p>
                        </div>
                        <div style="background-color: {BackgroundDark}; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid {BorderColor}; border-top: none;">
                            <h2 style="color: {TextColor}; margin-top: 0;">Hi {firstName},</h2>
                            <p style="color: {TextColor};">We received a request to reset your password for your CodeQuiz account.</p>
                            <p style="color: {TextColor};">Click the button below to create a new password:</p>
                            <div style="text-align: center; margin: 30px 0;">
                                <a href="{resetLink}" 
                                   style="display: inline-block; background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); color: white; text-decoration: none; padding: 14px 40px; border-radius: 20px; font-weight: bold; font-size: 16px;">
                                    Reset Password
                                </a>
                            </div>
                            <p style="color: {MutedTextColor}; font-size: 14px;">Or copy and paste this link into your browser:</p>
                            <div style="background-color: {BackgroundDarker}; padding: 12px; border-radius: 6px; word-break: break-all; font-size: 13px; color: {TextColor}; border: 1px solid {BorderColor};">
                                {resetLink}
                            </div>
                            <div style="background-color: {BackgroundDarker}; border-left: 4px solid {PrimaryColor}; padding: 12px; margin: 20px 0; border-radius: 0 6px 6px 0;">
                                <p style="margin: 0; color: {MutedTextColor}; font-size: 14px;">
                                    <strong style="color: {TextColor};">Security Notice:</strong> If you didn't request this password reset, please ignore this email. Your password will remain unchanged.
                                </p>
                            </div>
                            <hr style="border: none; border-top: 1px solid {BorderColor}; margin: 20px 0;">
                            <p style="font-size: 12px; color: {MutedTextColor}; text-align: center;">
                                This is an automated message from CodeQuiz. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendAttemptFeedbackAsync(string toEmail, string firstName, string quizTitle, float examineeGrade, float totalGrade, DateTime startTime, DateTime finishTime)
        {
            var subject = $"Quiz Results: {quizTitle}";
            var percentage = totalGrade > 0 ? (examineeGrade / totalGrade) * 100 : 0;
            var gradeColor = percentage >= 70 ? "#4CAF50" : (percentage >= 40 ? "#FFC107" : "#F44336"); // Green, Amber, Red

            var body = $"""
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: {TextColor}; margin: 0; padding: 0; background-color: {BackgroundDarker};">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <div style="background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                            <h1 style="color: white; margin: 0; font-size: 28px;">CodeQuiz</h1>
                            <p style="color: {TextColor}; margin: 10px 0 0 0; font-size: 14px;">Quiz Attempt Results</p>
                        </div>
                        <div style="background-color: {BackgroundDark}; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid {BorderColor}; border-top: none;">
                            <h2 style="color: {TextColor}; margin-top: 0;">Hello {firstName},</h2>
                            <p style="color: {TextColor};">Here are the results for your attempt on <strong>{quizTitle}</strong>.</p>
                            
                            <div style="background-color: {BackgroundDarker}; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid {BorderColor}; text-align: center;">
                                <p style="margin: 0; font-size: 14px; color: {MutedTextColor};">Your Score</p>
                                <h1 style="margin: 10px 0; font-size: 48px; color: {gradeColor};">{examineeGrade:0.##} <span style="font-size: 24px; color: {MutedTextColor};">/ {totalGrade:0.##}</span></h1>
                                <p style="margin: 0; font-size: 16px; color: {TextColor}; font-weight: bold;">{percentage:0.##}%</p>
                            </div>

                            <div style="margin: 20px 0;">
                                <p style="color: {TextColor}; margin-bottom: 5px;"><strong>Attempt Details:</strong></p>
                                <ul style="color: {TextColor}; padding-left: 20px; margin-top: 5px;">
                                    <li style="margin-bottom: 5px;">Started: {startTime:g}</li>
                                    <li style="margin-bottom: 5px;">Finished: {finishTime:g}</li>
                                    <li style="margin-bottom: 5px;">Duration: {(finishTime - startTime).ToString(@"hh\:mm\:ss")}</li>
                                </ul>
                            </div>

                            <div style="text-align: center; margin: 30px 0;">
                                <p style="color: {MutedTextColor};">Keep practicing to improve your skills!</p>
                            </div>
                            <hr style="border: none; border-top: 1px solid {BorderColor}; margin: 20px 0;">
                            <p style="font-size: 12px; color: {MutedTextColor}; text-align: center;">
                                This is an automated message from CodeQuiz. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendQuizEndSummaryAsync(string toEmail, string examinerName, QuizEndStatistics stats)
        {
            var subject = $"Quiz Ended: {stats.QuizTitle}";
            var passRateColor = stats.PassRate >= 70 ? "#4CAF50" : (stats.PassRate >= 40 ? "#FFC107" : "#F44336");
            var flaggedColor = stats.FlaggedSolutions > 0 ? "#F44336" : "#4CAF50";

            var body = $"""
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: {TextColor}; margin: 0; padding: 0; background-color: {BackgroundDarker};">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <div style="background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                            <h1 style="color: white; margin: 0; font-size: 28px;">CodeQuiz</h1>
                            <p style="color: {TextColor}; margin: 10px 0 0 0; font-size: 14px;">Quiz Summary Report</p>
                        </div>
                        <div style="background-color: {BackgroundDark}; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid {BorderColor}; border-top: none;">
                            <h2 style="color: {TextColor}; margin-top: 0;">Hello {examinerName},</h2>
                            <p style="color: {TextColor};">Your quiz <strong style="color: white;">{stats.QuizTitle}</strong> has ended. Here's a summary of the results:</p>
                            
                            <!-- Quiz Duration Info -->
                            <div style="background-color: {BackgroundDarker}; padding: 16px; border-radius: 8px; margin: 20px 0; border: 1px solid {BorderColor};">
                                <p style="margin: 0; font-size: 13px; color: {MutedTextColor};">
                                    <strong style="color: {TextColor};">Quiz Period:</strong> {stats.StartDate:MMM dd, yyyy HH:mm} - {stats.EndDate:MMM dd, yyyy HH:mm}
                                </p>
                            </div>

                            <!-- Statistics Grid -->
                            <table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                                <tr>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; border-radius: 8px 0 0 0; text-align: center; width: 33%;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: {TextColor};">{stats.TotalAttempts}</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Total Attempts</p>
                                    </td>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; text-align: center; width: 33%;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: #4CAF50;">{stats.SubmittedAttempts}</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Submitted</p>
                                    </td>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; border-radius: 0 8px 0 0; text-align: center; width: 33%;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: #FFC107;">{stats.InProgressAttempts}</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Auto-Submitted</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; border-radius: 0 0 0 8px; text-align: center;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: {passRateColor};">{stats.PassRate:F0}%</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Pass Rate</p>
                                    </td>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; text-align: center;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: {TextColor};">{stats.AverageGrade:F1}</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Average Grade</p>
                                    </td>
                                    <td style="padding: 12px; background-color: {BackgroundDarker}; border: 1px solid {BorderColor}; border-radius: 0 0 8px 0; text-align: center;">
                                        <p style="margin: 0; font-size: 28px; font-weight: bold; color: {TextColor};">{stats.TotalPoints:F0}</p>
                                        <p style="margin: 4px 0 0 0; font-size: 12px; color: {MutedTextColor};">Total Points</p>
                                    </td>
                                </tr>
                            </table>

                            <!-- Grade Details -->
                            <div style="background-color: {BackgroundDarker}; padding: 16px; border-radius: 8px; margin: 20px 0; border: 1px solid {BorderColor};">
                                <h3 style="color: {TextColor}; margin: 0 0 12px 0; font-size: 14px;">Grade Details</h3>
                                <table style="width: 100%;">
                                    <tr>
                                        <td style="padding: 6px 0; color: {TextColor};">Highest Grade</td>
                                        <td style="padding: 6px 0; text-align: right; color: #4CAF50; font-weight: bold;">{stats.HighestGrade:F1}</td>
                                    </tr>
                                    <tr>
                                        <td style="padding: 6px 0; color: {TextColor};">Lowest Grade</td>
                                        <td style="padding: 6px 0; text-align: right; color: #F44336; font-weight: bold;">{stats.LowestGrade:F1}</td>
                                    </tr>
                                </table>
                            </div>

                            <!-- AI Assessment Summary (if any) -->
                            {(stats.TotalAiAssessments > 0 ? $"""
                            <div style="background-color: {BackgroundDarker}; padding: 16px; border-radius: 8px; margin: 20px 0; border: 1px solid {BorderColor};">
                                <h3 style="color: {TextColor}; margin: 0 0 12px 0; font-size: 14px;">🤖 AI Assessment Summary</h3>
                                <table style="width: 100%;">
                                    <tr>
                                        <td style="padding: 6px 0; color: {TextColor};">Solutions Assessed</td>
                                        <td style="padding: 6px 0; text-align: right; color: {TextColor}; font-weight: bold;">{stats.TotalAiAssessments}</td>
                                    </tr>
                                    <tr>
                                        <td style="padding: 6px 0; color: {TextColor};">Flagged Solutions</td>
                                        <td style="padding: 6px 0; text-align: right; color: {flaggedColor}; font-weight: bold;">{stats.FlaggedSolutions}</td>
                                    </tr>
                                </table>
                            </div>
                            """ : "")}

                            <div style="text-align: center; margin: 30px 0;">
                                <p style="color: {MutedTextColor};">Log in to CodeQuiz to view detailed results and grade submissions.</p>
                            </div>
                            <hr style="border: none; border-top: 1px solid {BorderColor}; margin: 20px 0;">
                            <p style="font-size: 12px; color: {MutedTextColor}; text-align: center;">
                                This is an automated message from CodeQuiz. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public async Task SendGradeUpdateEmailAsync(string toEmail, string firstName, string quizTitle, string instructorName, List<GradeUpdateInfo> gradeUpdates)
        {
            var subject = $"Grade Update: {quizTitle}";

            // Build the grade updates HTML
            var updatesHtml = new System.Text.StringBuilder();
            foreach (var update in gradeUpdates)
            {
                var gradeChangeHtml = "";
                var feedbackChangeHtml = "";

                if (update.GradeChanged)
                {
                    var oldGradeStr = update.OldGrade.HasValue ? $"{update.OldGrade:0.##}" : "Not graded";
                    var newGradeStr = update.NewGrade.HasValue ? $"{update.NewGrade:0.##}" : "Not graded";
                    var changeColor = (update.NewGrade ?? 0) >= (update.OldGrade ?? 0) ? "#4CAF50" : "#F44336";
                    gradeChangeHtml = $"""
                        <p style="margin: 4px 0; color: {TextColor};">
                            <strong>Grade:</strong> 
                            <span style="color: {MutedTextColor}; text-decoration: line-through;">{oldGradeStr}</span> 
                            → <span style="color: {changeColor}; font-weight: bold;">{newGradeStr}</span> / {update.TotalPoints:0.##}
                        </p>
                        """;
                }

                if (update.FeedbackChanged)
                {
                    var feedbackText = !string.IsNullOrEmpty(update.NewFeedback)
                        ? update.NewFeedback
                        : "<em style=\"color: " + MutedTextColor + ";\">No feedback</em>";
                    feedbackChangeHtml = $"""
                        <div style="margin-top: 8px; padding: 10px; background-color: {BackgroundDarker}; border-radius: 6px; border-left: 3px solid {PrimaryColor};">
                            <p style="margin: 0; font-size: 12px; color: {MutedTextColor}; margin-bottom: 4px;">Instructor Feedback:</p>
                            <p style="margin: 0; color: {TextColor}; font-size: 14px;">{feedbackText}</p>
                        </div>
                        """;
                }

                updatesHtml.Append($"""
                    <div style="background-color: {BackgroundDarker}; padding: 16px; border-radius: 8px; margin-bottom: 12px; border: 1px solid {BorderColor};">
                        <h4 style="margin: 0 0 8px 0; color: {TextColor};">Question {update.QuestionNumber}</h4>
                        {gradeChangeHtml}
                        {feedbackChangeHtml}
                    </div>
                    """);
            }

            var body = $"""
                <html>
                <body style="font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: {TextColor}; margin: 0; padding: 0; background-color: {BackgroundDarker};">
                    <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                        <div style="background: linear-gradient(135deg, {PrimaryLightColor}, {PrimaryColor}); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                            <h1 style="color: white; margin: 0; font-size: 28px;">CodeQuiz</h1>
                            <p style="color: {TextColor}; margin: 10px 0 0 0; font-size: 14px;">Grade Update Notification</p>
                        </div>
                        <div style="background-color: {BackgroundDark}; padding: 30px; border-radius: 0 0 12px 12px; border: 1px solid {BorderColor}; border-top: none;">
                            <h2 style="color: {TextColor}; margin-top: 0;">Hello {firstName},</h2>
                            <p style="color: {TextColor};">Your grades for <strong style="color: white;">{quizTitle}</strong> have been updated by <strong style="color: white;">{instructorName}</strong>.</p>
                            
                            <h3 style="color: {TextColor}; margin-top: 24px; margin-bottom: 16px;">Updates:</h3>
                            {updatesHtml}

                            <div style="text-align: center; margin: 30px 0;">
                                <p style="color: {MutedTextColor};">Log in to CodeQuiz to view the full details of your submission.</p>
                            </div>
                            <hr style="border: none; border-top: 1px solid {BorderColor}; margin: 20px 0;">
                            <p style="font-size: 12px; color: {MutedTextColor}; text-align: center;">
                                This is an automated message from CodeQuiz. Please do not reply to this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
    }
}
