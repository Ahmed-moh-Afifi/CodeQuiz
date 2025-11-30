using CodeQuizBackend.Authentication.Models;
using CodeQuizBackend.Authentication.Repositories;
using CodeQuizBackend.Authentication.Services;
using CodeQuizBackend.Core.Data;
using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Core.Middlewares;
using CodeQuizBackend.Execution.Services;
using CodeQuizBackend.Quiz.Hubs;
using CodeQuizBackend.Quiz.Repositories;
using CodeQuizBackend.Quiz.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
    c.SwaggerDoc("v1", new() { Title = "CodeQuizBackend", Version = "v1" });
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
                builder.WithOrigins("http://localhost:4200")
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

// Add services here
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
builder.Services.AddScoped<IAuthenticationService, JWTAuthenticationService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IQuizzesRepository, QuizzesRepository>();
builder.Services.AddScoped<IQuizzesService, QuizzesService>();
builder.Services.AddScoped<IAttemptsService, AttemptsService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<QuizCodeGenerator>();

// Running code services
builder.Services.AddScoped<ICodeRunner, CSharpCodeRunner>();
builder.Services.AddScoped<ICodeRunnerFactory, CodeRunnerFactory>();
builder.Services.AddScoped<IEvaluator, Evaluator>();

// Background services
builder.Services.AddHostedService<AttemptTimerService>();

builder.Services.AddSignalR();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<AttemptsHub>("/hubs/Attempts");

app.Run();
