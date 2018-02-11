using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class Tagsplaylist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonObject<string>>(
                name: "Tags",
                table: "Playlist",
                type: "json",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Playlist");
        }
    }
}
