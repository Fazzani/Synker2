using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class fixRoleLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Libelle",
                table: "Role",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Role",
                newName: "Libelle");
        }
    }
}
