using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class groupModelUpdateAndNotifPlaylist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SynkConfig_Cron",
                table: "Playlist");

            migrationBuilder.AddColumn<int>(
                name: "SynkConfig_NotifcationTypeInsertedMedia",
                table: "Playlist",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SynkConfig_NotifcationTypeInsertedMedia",
                table: "Playlist");

            migrationBuilder.AddColumn<string>(
                name: "SynkConfig_Cron",
                table: "Playlist",
                maxLength: 10,
                nullable: true);
        }
    }
}
