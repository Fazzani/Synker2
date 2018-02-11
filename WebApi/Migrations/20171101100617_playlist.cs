using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class playlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(type: "JSON", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Cron = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    Freindlyname = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlist_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UserId",
                table: "Playlist",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Playlist");
        }
    }
}
