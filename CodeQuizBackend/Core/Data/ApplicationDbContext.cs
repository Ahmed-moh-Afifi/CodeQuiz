using Microsoft.EntityFrameworkCore;

namespace CodeQuizBackend.Core.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
    }
}
