using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class auditEntityUserRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_UserRole_Id",
            //    table: "UserRole");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserRole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserRole",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserRole_Id",
                table: "UserRole",
                column: "Id");
        }
    }
}
