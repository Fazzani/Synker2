﻿// <auto-generated />
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace hfa.WebApi.Migrations
{
    [DbContext(typeof(SynkerDbContext))]
    [Migration("20171023175250_commandsMigrations")]
    partial class commandsMigrations
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("Hfa.SyncLibrary.Messages.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author")
                        .HasMaxLength(64);

                    b.Property<string>("Content");

                    b.Property<int>("MessageType");

                    b.Property<int>("Status");

                    b.Property<DateTime>("TimeStamp");

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.Command", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommandText")
                        .IsRequired();

                    b.Property<string>("Comments");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<DateTime>("TreatedDate");

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Command");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.ConnectionState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessToken");

                    b.Property<bool>("Approved");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("Disabled");

                    b.Property<DateTime>("LastConnection");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(512);

                    b.Property<string>("RefreshToken")
                        .HasMaxLength(255);

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(512);

                    b.HasKey("Id");

                    b.ToTable("ConnectionState");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Libelle")
                        .HasMaxLength(32);

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BirthDay");

                    b.Property<int>("ConnectionStateId");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<byte>("Gender");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<string>("Photo");

                    b.Property<DateTime>("UpdatedDate");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionStateId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("FirstName", "LastName");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.Command", b =>
                {
                    b.HasOne("hfa.WebApi.Dal.Entities.User", "User")
                        .WithMany("Commands")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.Role", b =>
                {
                    b.HasOne("hfa.WebApi.Dal.Entities.User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("hfa.WebApi.Dal.Entities.User", b =>
                {
                    b.HasOne("hfa.WebApi.Dal.Entities.ConnectionState", "ConnectionState")
                        .WithMany()
                        .HasForeignKey("ConnectionStateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
