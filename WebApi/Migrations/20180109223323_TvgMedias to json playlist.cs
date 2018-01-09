using Microsoft.EntityFrameworkCore.Migrations;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class TvgMediastojsonplaylist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonObject<List<TvgMedia>>>(
                name: "Medias",
                table: "Playlist",
                type: "json",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Medias",
                table: "Playlist");
        }
    }
}
