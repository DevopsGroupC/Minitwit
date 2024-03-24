using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace csharp_minitwit;

public partial class MinitwitContext : DbContext
{
    private readonly string _connectionString;

    public MinitwitContext() { }

    public MinitwitContext(DbContextOptions<MinitwitContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.FollowerId).HasName("PK_Follower");
            entity.ToTable("follower");

            entity.HasOne(e => e.Who)
                  .WithMany(u => u.Following)
                  .HasForeignKey(e => e.WhoId)
                  .OnDelete(DeleteBehavior.Restrict) // Prevent cascading delete
                  .HasConstraintName("FK_Follower_Who");

            entity.HasOne(e => e.Whom)
                  .WithMany(u => u.Followers)
                  .HasForeignKey(e => e.WhomId)
                  .OnDelete(DeleteBehavior.Restrict) // Prevent cascading delete
                  .HasConstraintName("FK_Follower_Whom");

            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.WhoId).HasColumnName("who_id");
            entity.Property(e => e.WhomId).HasColumnName("whom_id");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("message");

            entity.HasOne(m => m.Author)
                  .WithMany(u => u.Messages)
                  .HasForeignKey(m => m.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade) // Prevent cascading delete
                  .HasConstraintName("FK_Message_User");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Flagged).HasColumnName("flagged");
            entity.Property(e => e.PubDate).HasColumnName("pub_date");
            entity.Property(e => e.Text)
                .HasColumnType("text")
                .HasColumnName("text");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasColumnType("text")
                .HasColumnName("email");
            entity.Property(e => e.PwHash)
                .HasColumnType("text")
                .HasColumnName("pw_hash");
            entity.Property(e => e.Username)
                .HasColumnType("text")
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}