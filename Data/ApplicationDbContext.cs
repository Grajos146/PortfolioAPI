using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Model.Entities;

namespace PortfolioAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
}
