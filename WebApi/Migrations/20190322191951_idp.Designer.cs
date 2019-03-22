﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using hfa.Synker.Services.Dal;

namespace hfa.WebApi.Migrations
{
    [DbContext(typeof(SynkerDbContext))]
    [Migration("20190322191951_idp")]
    partial class idp
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("hfa.Synker.Service.Entities.Auth.Command", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("CommandExecutingType");

                    b.Property<string>("CommandText")
                        .IsRequired();

                    b.Property<string>("Comments");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Cron");

                    b.Property<byte>("Interpreter");

                    b.Property<int>("Priority");

                    b.Property<int>("ReplayCount");

                    b.Property<byte>("Status");

                    b.Property<DateTime?>("TreatedDate");

                    b.Property<DateTime?>("TreatingDate");

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Command");
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Auth.ConnectionState", b =>
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

            modelBuilder.Entity("hfa.Synker.Service.Entities.Auth.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ConnectionStateId");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<DateTime>("UpdatedDate");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionStateId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Host", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .IsRequired();

                    b.Property<string>("Comments");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("Enabled");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Port");

                    b.Property<DateTime>("UpdatedDate");

                    b.HasKey("Id");

                    b.HasIndex("Address", "Port");

                    b.ToTable("Hosts");
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Playlists.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("Favorite");

                    b.Property<string>("Freindlyname")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("Medias")
                        .HasColumnType("jsonb");

                    b.Property<byte>("Status");

                    b.Property<string>("TagsString")
                        .HasColumnName("Tags")
                        .HasColumnType("jsonb");

                    b.Property<string>("TvgSitesString")
                        .IsRequired();

                    b.Property<Guid>("UniqueId");

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UniqueId")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Playlist");
                });

            modelBuilder.Entity("hfa.synker.entities.Notifications.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("PushAuth")
                        .IsRequired();

                    b.Property<string>("PushEndpoint")
                        .IsRequired();

                    b.Property<string>("PushP256DH")
                        .IsRequired();

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("hfa.synker.entities.WebGrabConfigDocker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("Cron")
                        .IsRequired();

                    b.Property<string>("DockerImage")
                        .IsRequired();

                    b.Property<int>("HostId");

                    b.Property<string>("MountSourcePath");

                    b.Property<DateTime>("UpdatedDate");

                    b.Property<string>("WebgrabConfigUrl")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("HostId");

                    b.ToTable("WebGrabConfigDockers");
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Auth.Command", b =>
                {
                    b.HasOne("hfa.Synker.Service.Entities.Auth.User", "User")
                        .WithMany("Commands")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Auth.User", b =>
                {
                    b.HasOne("hfa.Synker.Service.Entities.Auth.ConnectionState", "ConnectionState")
                        .WithMany()
                        .HasForeignKey("ConnectionStateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Host", b =>
                {
                    b.OwnsOne("hfa.Synker.Service.Entities.Authentication", "Authentication", b1 =>
                        {
                            b1.Property<int>("HostId");

                            b1.Property<string>("CertPath");

                            b1.Property<string>("Password");

                            b1.Property<string>("Username");

                            b1.HasKey("HostId");

                            b1.ToTable("Hosts");

                            b1.HasOne("hfa.Synker.Service.Entities.Host")
                                .WithOne("Authentication")
                                .HasForeignKey("hfa.Synker.Service.Entities.Authentication", "HostId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("hfa.Synker.Service.Entities.Playlists.Playlist", b =>
                {
                    b.HasOne("hfa.Synker.Service.Entities.Auth.User", "User")
                        .WithMany("Playlists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("hfa.Synker.Service.Entities.Playlists.SynkConfig", "SynkConfig", b1 =>
                        {
                            b1.Property<int>("PlaylistId");

                            b1.Property<bool>("AutoSynchronize");

                            b1.Property<bool>("CleanName");

                            b1.Property<int?>("NotifcationTypeInsertedMedia");

                            b1.Property<string>("Provider");

                            b1.Property<bool>("SynkEpg");

                            b1.Property<byte>("SynkGroup");

                            b1.Property<bool>("SynkLogos");

                            b1.Property<string>("Url");

                            b1.HasKey("PlaylistId");

                            b1.ToTable("Playlist");

                            b1.HasOne("hfa.Synker.Service.Entities.Playlists.Playlist")
                                .WithOne("SynkConfig")
                                .HasForeignKey("hfa.Synker.Service.Entities.Playlists.SynkConfig", "PlaylistId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("hfa.synker.entities.Notifications.Device", b =>
                {
                    b.HasOne("hfa.Synker.Service.Entities.Auth.User", "User")
                        .WithMany("Devices")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("hfa.synker.entities.WebGrabConfigDocker", b =>
                {
                    b.HasOne("hfa.Synker.Service.Entities.Host", "RunnableHost")
                        .WithMany("WebGrabConfigDockers")
                        .HasForeignKey("HostId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
