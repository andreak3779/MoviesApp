using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Models;

namespace MoviesApi.Data;

public class MoviesDbContext : IdentityDbContext<ApplicationUser>
{
    public MoviesDbContext(DbContextOptions<MoviesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<MediaType> MediaTypes { get; set; } = null!;
    public DbSet<FilmGenre> FilmGenres { get; set; } = null!;
    public DbSet<MovieMediaType> MovieMediaTypes { get; set; } = null!;
    public DbSet<MovieFilmGenre> MovieFilmGenres { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.ToTable("Movies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ReleaseYear).IsRequired();
            
            // Configure many-to-many relationships
            entity.HasMany(m => m.MovieMediaTypes)
                  .WithOne(mm => mm.Movie)
                  .HasForeignKey(mm => mm.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(m => m.MovieFilmGenres)
                  .WithOne(mf => mf.Movie)
                  .HasForeignKey(mf => mf.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MediaType configuration
        modelBuilder.Entity<MediaType>(entity =>
        {
            entity.ToTable("MediaTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // FilmGenre configuration
        modelBuilder.Entity<FilmGenre>(entity =>
        {
            entity.ToTable("FilmGenres");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // MovieMediaType junction table configuration
        modelBuilder.Entity<MovieMediaType>(entity =>
        {
            entity.ToTable("MovieMediaTypes");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.MovieId, e.MediaTypeId }).IsUnique();
            
            entity.HasOne(e => e.Movie)
                  .WithMany(m => m.MovieMediaTypes)
                  .HasForeignKey(e => e.MovieId);
            
            entity.HasOne(e => e.MediaType)
                  .WithMany(m => m.MovieMediaTypes)
                  .HasForeignKey(e => e.MediaTypeId);
        });

        // MovieFilmGenre junction table configuration
        modelBuilder.Entity<MovieFilmGenre>(entity =>
        {
            entity.ToTable("MovieFilmGenres");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.MovieId, e.FilmGenreId }).IsUnique();
            
            entity.HasOne(e => e.Movie)
                  .WithMany(m => m.MovieFilmGenres)
                  .HasForeignKey(e => e.MovieId);
            
            entity.HasOne(e => e.FilmGenre)
                  .WithMany(m => m.MovieFilmGenres)
                  .HasForeignKey(e => e.FilmGenreId);
        });

        // Seed data for MediaTypes
        modelBuilder.Entity<MediaType>().HasData(
            new MediaType { Id = 1, Name = "DVD", Description = "Digital Versatile Disc" },
            new MediaType { Id = 2, Name = "Blu-Ray", Description = "High-definition optical disc" },
            new MediaType { Id = 3, Name = "4K Ultra HD Blu-Ray", Description = "Ultra high-definition optical disc" },
            new MediaType { Id = 4, Name = "Digital Download", Description = "Digital file download" },
            new MediaType { Id = 5, Name = "Digital Streaming", Description = "Online streaming service" },
            new MediaType { Id = 6, Name = "VHS", Description = "Video Home System (Legacy)" },
            new MediaType { Id = 7, Name = "LaserDisc", Description = "Optical disc storage (Legacy)" }
        );

        // Seed data for FilmGenres
        modelBuilder.Entity<FilmGenre>().HasData(
            new FilmGenre { Id = 1, Name = "Action", Description = "High-energy action sequences" },
            new FilmGenre { Id = 2, Name = "Adventure", Description = "Exciting journeys and quests" },
            new FilmGenre { Id = 3, Name = "Comedy", Description = "Humorous and entertaining" },
            new FilmGenre { Id = 4, Name = "Drama", Description = "Serious and emotional storytelling" },
            new FilmGenre { Id = 5, Name = "Horror", Description = "Scary and suspenseful" },
            new FilmGenre { Id = 6, Name = "Science Fiction", Description = "Futuristic and speculative" },
            new FilmGenre { Id = 7, Name = "Romance", Description = "Love and relationships" },
            new FilmGenre { Id = 8, Name = "Thriller", Description = "Suspenseful and exciting" },
            new FilmGenre { Id = 9, Name = "Mystery", Description = "Puzzles and investigations" },
            new FilmGenre { Id = 10, Name = "Documentary", Description = "Non-fiction factual content" },
            new FilmGenre { Id = 11, Name = "Animation", Description = "Animated films" },
            new FilmGenre { Id = 12, Name = "Fantasy", Description = "Magical and supernatural elements" },
            new FilmGenre { Id = 13, Name = "Crime", Description = "Criminal activities and investigations" }
        );

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
        });
    }
}
