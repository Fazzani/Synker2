using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class playlistUniqueIdFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UniqueId",
                table: "Playlist",
                column: "UniqueId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Playlist_UniqueId",
                table: "Playlist");
        }
    }
}
