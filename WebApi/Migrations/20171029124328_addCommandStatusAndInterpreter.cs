using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class addCommandStatusAndInterpreter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Interpreter",
                table: "Command",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Command",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "Command",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TreatingDate",
                table: "Command",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interpreter",
                table: "Command");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Command");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Command");

            migrationBuilder.DropColumn(
                name: "TreatingDate",
                table: "Command");
        }
    }
}
