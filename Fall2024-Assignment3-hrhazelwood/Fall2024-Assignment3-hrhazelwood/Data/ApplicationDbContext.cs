using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_hrhazelwood.Models;

namespace Fall2024_Assignment3_hrhazelwood.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

public DbSet<Fall2024_Assignment3_hrhazelwood.Models.Movie> Movie { get; set; } = default!;

public DbSet<Fall2024_Assignment3_hrhazelwood.Models.Actor> Actor { get; set; } = default!;

public DbSet<Fall2024_Assignment3_hrhazelwood.Models.MovieActor> MovieActor { get; set; } = default!;
}

