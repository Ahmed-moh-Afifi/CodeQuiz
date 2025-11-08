using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Execution.Exceptions;
using CodeQuizBackend.Execution.Models;
using System.Diagnostics;

namespace CodeQuizBackend.Execution.Services
{
    public class CSharpCodeRunner(IConfiguration configuration) : ICodeRunner
    {
        private readonly string baseCodeFilePath = configuration["CodeFilesPath"]!;
        public async Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null)
        {
            return await Task.Run(() => RunCode(code, options));
        }

        public CodeRunnerResult RunCode(string code, CodeRunnerOptions? options = null)
        {
            string codeFileName = Guid.NewGuid().ToString();
            string filePath = baseCodeFilePath + codeFileName + ".cs";
            try
            {
                options ??= new CodeRunnerOptions();

                code = "#pragma warning disable\n" + code;
                File.WriteAllText(filePath, code);

                Process process = new Process();
                process.StartInfo.WorkingDirectory = baseCodeFilePath;
                process.StartInfo.FileName = configuration["CSharpCompilerPath"];
                process.StartInfo.Arguments = $"run {filePath}";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();


                foreach (var inputLine in options.Input)
                {
                    process.StandardInput.WriteLine(inputLine);
                }
                process.StandardInput.Close();

                string output = process.StandardOutput.ReadToEnd().Trim();
                string errors = process.StandardError.ReadToEnd().Trim();

                process.WaitForExit();

                return new CodeRunnerResult
                {
                    Success = process.ExitCode == 0,
                    Output = options.ContainOutput ? output : null,
                    Error = options.ContainError ? errors : null
                };
            }
            catch (Exception)
            {
                throw new CodeRunnerException();
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
