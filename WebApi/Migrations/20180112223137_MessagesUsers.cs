using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class MessagesUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_UserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Messages",
                maxLength: 64,
                nullable: true);
        }
    }
}
