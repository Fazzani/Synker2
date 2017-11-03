using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace hfa.WebApi.Migrations
{
    public partial class playlistSynkConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cron",
                table: "Playlist",
                newName: "SynkConfig_Cron");

            migrationBuilder.AddColumn<bool>(
                name: "SynkConfig_CleanName",
                table: "Playlist",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SynkConfig_SynkEpg",
                table: "Playlist",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "SynkConfig_SynkGroup",
                table: "Playlist",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "SynkConfig_SynkLogos",
                table: "Playlist",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SynkConfig_CleanName",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "SynkConfig_SynkEpg",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "SynkConfig_SynkGroup",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "SynkConfig_SynkLogos",
                table: "Playlist");

            migrationBuilder.RenameColumn(
                name: "SynkConfig_Cron",
                table: "Playlist",
                newName: "Cron");
        }
    }
}
