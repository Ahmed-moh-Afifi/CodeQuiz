using CodeQuizBackend.Core.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddSwaggerGen();

// Configure Entity Framework with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(cfg =>
    cfg.UseMySql(builder.Configuration["ConnectionString"],
    ServerVersion.AutoDetect(builder.Configuration["ConnectionString"]),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
