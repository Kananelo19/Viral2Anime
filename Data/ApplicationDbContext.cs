using Microsoft.EntityFrameworkCore;
using Viral2Anime.Models.Project;

namespace Viral2Anime.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ViralProject> ViralProjects =>
        Set<ViralProject>();
}
