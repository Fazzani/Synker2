using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class requiredRoleName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 32,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 32);
        }
    }
}
