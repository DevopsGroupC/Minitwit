﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using csharp_minitwit;

#nullable disable

namespace csharp_minitwit.Migrations
{
    [DbContext(typeof(MinitwitContext))]
    [Migration("20240401101614_MetaData")]
    partial class MetaData
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("csharp_minitwit.Models.Entities.Follower", b =>
                {
                    b.Property<int>("FollowerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("WhoId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("who_id");

                    b.Property<int>("WhomId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("whom_id");

                    b.HasKey("FollowerId");

                    b.HasIndex("WhoId");

                    b.HasIndex("WhomId");

                    b.ToTable("follower", (string)null);
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.Message", b =>
                {
                    b.Property<int>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("author_id");

                    b.Property<int>("Flagged")
                        .HasColumnType("INTEGER")
                        .HasColumnName("flagged");

                    b.Property<int>("PubDate")
                        .HasColumnType("INTEGER")
                        .HasColumnName("pub_date");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.HasKey("MessageId");

                    b.HasIndex("AuthorId");

                    b.ToTable("message", (string)null);
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.MetaData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Latest")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Latest");

                    b.ToTable("metadata", (string)null);
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("PwHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pw_hash");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("UserId");

                    b.HasIndex("Username");

                    b.ToTable("user", (string)null);
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.Follower", b =>
                {
                    b.HasOne("csharp_minitwit.Models.Entities.User", "Who")
                        .WithMany("Following")
                        .HasForeignKey("WhoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Follower_Who");

                    b.HasOne("csharp_minitwit.Models.Entities.User", "Whom")
                        .WithMany("Followers")
                        .HasForeignKey("WhomId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Follower_Whom");

                    b.Navigation("Who");

                    b.Navigation("Whom");
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.Message", b =>
                {
                    b.HasOne("csharp_minitwit.Models.Entities.User", "Author")
                        .WithMany("Messages")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Message_User");

                    b.Navigation("Author");
                });

            modelBuilder.Entity("csharp_minitwit.Models.Entities.User", b =>
                {
                    b.Navigation("Followers");

                    b.Navigation("Following");

                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
