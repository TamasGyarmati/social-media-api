using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Database;

public class SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // Cascade : Deletes all post made by the creator
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(e => e.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Cascade : Deletes all comment made by the creator

            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
        });
    }
}