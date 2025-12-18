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
    }
}
