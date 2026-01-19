using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Repositories;
using CodeQuizBackend.Authentication.Services;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Core.Middlewares;
using CodeQuizBackend.Execution.Models;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Repositories;
using CodeQuizBackend.Quiz.Services;
using CodeQuizBackend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CodeQuiz API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(
            builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

// Configure Entity Framework with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(cfg =>
    cfg.UseMySql(builder.Configuration["ConnectionString"],
    ServerVersion.AutoDetect(builder.Configuration["ConnectionString"]),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

// Conigure Identity and Authentication services
builder.Services.AddIdentity<User, IdentityRole>(options => options.User.RequireUniqueEmail = true)
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Issuer"],
          ValidAudience = builder.Configuration["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"]!))
      };
  });

builder.Services.AddRateLimiter(options =>
{
    // Applies to ALL requests if not overridden
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 25,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // Can be applied to specific endpoints
    options.AddFixedWindowLimiter("StrictPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Custom Rejection Status (Default is 503, commonly changed to 429)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services here
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
builder.Services.AddScoped<IAuthenticationService, JWTAuthenticationService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IQuizzesRepository, QuizzesRepository>();
builder.Services.AddScoped<IQuizzesService, QuizzesService>();
builder.Services.AddScoped<IAttemptsService, AttemptsService>();
builder.Services.AddScoped<IMailService, SmtpMailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IQuizCodeGenerator, QuizCodeGenerator>();

// Running code services
builder.Services.AddScoped<ICodeRunner, CSharpCodeRunner>();
builder.Services.AddScoped<ICodeRunner, PythonCodeRunner>();
builder.Services.AddScoped<ICodeRunnerFactory, CodeRunnerFactory>();
builder.Services.AddScoped<IEvaluator, Evaluator>();

// Sandboxed code execution services
builder.Services.AddSingleton(new SandboxConfiguration
{
    TempCodePath = builder.Configuration["CodeFilesPath"] ?? "/tmp/code",
    TimeoutSeconds = int.Parse(builder.Configuration["Sandbox:TimeoutSeconds"] ?? "10"),
    MemoryLimitBytes = long.Parse(builder.Configuration["Sandbox:MemoryLimitMb"] ?? "128") * 1024 * 1024,
    LanguageConfigs = new Dictionary<string, LanguageSandboxConfig>
    {
        ["CSharp"] = new()
        {
            DockerImage = "mcr.microsoft.com/dotnet/sdk:10.0",
            Command = "dotnet",
            FileExtension = ".cs",
            ArgumentTemplate = ["run", "/sandbox/{filename}"],
            CodePrefix = "#pragma warning disable\n"
        },
        ["Python"] = new()
        {
            DockerImage = "python:3.12-slim",
            Command = "python -u",
            FileExtension = ".py",
            ArgumentTemplate = ["/sandbox/{filename}"]
        }
    }
});

builder.Services.AddSingleton<IDockerSandbox, DockerSandbox>();
builder.Services.AddSingleton<CSharpCodeRunner>();
builder.Services.AddSingleton<SandboxedCodeRunnerFactory>(sp => innerRunner => new SandboxedCodeRunner(
    innerRunner,
    sp.GetRequiredService<IDockerSandbox>(),
    sp.GetRequiredService<SandboxConfiguration>(),
    sp.GetRequiredService<IAppLogger<SandboxedCodeRunner>>()
));

// Background services
builder.Services.AddHostedService<AttemptTimerService>();

builder.Services.AddSignalR();

builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["SentryDsn"];
    o.Debug = true;
    o.TracesSampleRate = 1.0;
});

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapScalarApiReference(options =>
        {
            // Important: Tell Scalar to read the file generated by Swagger
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");

            options.WithTitle("CodeQuiz API");
            options.WithTheme(ScalarTheme.DeepSpace);
            options.AddPreferredSecuritySchemes("Bearer");
        });
    }
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<AttemptsHub>("/hubs/Attempts");
app.MapHub<QuizzesHub>("/hubs/Quizzes");

app.UseRateLimiter();

app.MapControllers();

app.Run();
