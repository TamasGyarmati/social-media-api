using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Database;

public class SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.HasOne(p => p.Creator)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(p => p.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            // Against duplicates
            entity.HasKey(pl => new { pl.PostId, pl.UserId });

            entity.HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.ParentComment)
                .WithMany(pc => pc.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(cl => new { cl.CommentId, cl.UserId });

            entity.HasOne(cl => cl.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cl => cl.User)
                .WithMany()
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasKey(uf => new { uf.FollowerId, uf.FollowedId });

            entity.HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uf => uf.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}