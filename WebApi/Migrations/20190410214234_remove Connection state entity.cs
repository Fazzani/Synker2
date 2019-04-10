using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace hfa.WebApi.Migrations
{
    public partial class removeConnectionstateentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ConnectionState_ConnectionStateId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ConnectionState");

            migrationBuilder.DropIndex(
                name: "IX_Users_ConnectionStateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConnectionStateId",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConnectionStateId",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConnectionState",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AccessToken = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Disabled = table.Column<bool>(nullable: false),
                    LastConnection = table.Column<DateTime>(nullable: false),
                    Password = table.Column<string>(maxLength: 512, nullable: false),
                    RefreshToken = table.Column<string>(maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    UserName = table.Column<string>(maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionState", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ConnectionStateId",
                table: "Users",
                column: "ConnectionStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ConnectionState_ConnectionStateId",
                table: "Users",
                column: "ConnectionStateId",
                principalTable: "ConnectionState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
