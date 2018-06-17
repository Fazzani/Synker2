using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace hfa.WebApi.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "ConnectionState",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        UserName = table.Column<string>(maxLength: 512, nullable: false),
            //        Password = table.Column<string>(maxLength: 512, nullable: false),
            //        LastConnection = table.Column<DateTime>(nullable: false),
            //        RefreshToken = table.Column<string>(maxLength: 255, nullable: true),
            //        AccessToken = table.Column<string>(nullable: true),
            //        Disabled = table.Column<bool>(nullable: false),
            //        Approved = table.Column<bool>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ConnectionState", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Hosts",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        Name = table.Column<string>(nullable: false),
            //        Address = table.Column<string>(nullable: false),
            //        Authentication_CertPath = table.Column<string>(nullable: true),
            //        Authentication_Username = table.Column<string>(nullable: true),
            //        Authentication_Password = table.Column<string>(nullable: true),
            //        Port = table.Column<string>(nullable: true),
            //        Comments = table.Column<string>(nullable: true),
            //        Enabled = table.Column<bool>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Hosts", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Roles",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        Name = table.Column<string>(maxLength: 32, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Roles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        FirstName = table.Column<string>(maxLength: 64, nullable: false),
            //        LastName = table.Column<string>(maxLength: 64, nullable: false),
            //        Email = table.Column<string>(nullable: false),
            //        Photo = table.Column<string>(nullable: true),
            //        BirthDay = table.Column<DateTime>(nullable: false),
            //        Gender = table.Column<byte>(nullable: false),
            //        ConnectionStateId = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Users_ConnectionState_ConnectionStateId",
            //            column: x => x.ConnectionStateId,
            //            principalTable: "ConnectionState",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "WebGrabConfigDockers",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        MountSourcePath = table.Column<string>(nullable: true),
            //        DockerImage = table.Column<string>(nullable: false),
            //        WebgrabConfigUrl = table.Column<string>(nullable: false),
            //        HostId = table.Column<int>(nullable: false),
            //        Cron = table.Column<string>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_WebGrabConfigDockers", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_WebGrabConfigDockers_Hosts_HostId",
            //            column: x => x.HostId,
            //            principalTable: "Hosts",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Command",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        UserId = table.Column<int>(nullable: false),
            //        CommandText = table.Column<string>(nullable: false),
            //        TreatedDate = table.Column<DateTime>(nullable: true),
            //        TreatingDate = table.Column<DateTime>(nullable: true),
            //        Comments = table.Column<string>(nullable: true),
            //        Status = table.Column<byte>(nullable: false),
            //        Priority = table.Column<int>(nullable: false),
            //        Interpreter = table.Column<byte>(nullable: false),
            //        ReplayCount = table.Column<int>(nullable: false),
            //        CommandExecutingType = table.Column<byte>(nullable: false),
            //        Cron = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Command", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Command_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Messages",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        Content = table.Column<string>(nullable: true),
            //        TimeStamp = table.Column<DateTime>(nullable: false),
            //        MessageType = table.Column<int>(nullable: false),
            //        Status = table.Column<int>(nullable: false),
            //        UserId = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Messages", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Messages_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Playlist",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
            //        UniqueId = table.Column<Guid>(nullable: false),
            //        UserId = table.Column<int>(nullable: false),
            //        Freindlyname = table.Column<string>(maxLength: 100, nullable: false),
            //        SynkConfig_SynkLogos = table.Column<bool>(nullable: false),
            //        SynkConfig_SynkEpg = table.Column<bool>(nullable: false),
            //        SynkConfig_SynkGroup = table.Column<byte>(nullable: false),
            //        SynkConfig_CleanName = table.Column<bool>(nullable: false),
            //        SynkConfig_Url = table.Column<string>(nullable: true),
            //        SynkConfig_Provider = table.Column<string>(nullable: true),
            //        SynkConfig_NotifcationTypeInsertedMedia = table.Column<int>(nullable: true),
            //        Status = table.Column<byte>(nullable: false),
            //        Medias = table.Column<string>(type: "jsonb", nullable: true),
            //        Tags = table.Column<string>(type: "jsonb", nullable: true),
            //        TvgSitesString = table.Column<string>(nullable: false),
            //        Favorite = table.Column<bool>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Playlist", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Playlist_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserRole",
            //    columns: table => new
            //    {
            //        UpdatedDate = table.Column<DateTime>(nullable: false),
            //        CreatedDate = table.Column<DateTime>(nullable: false),
            //        UserId = table.Column<int>(nullable: false),
            //        RoleId = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_UserRole_Roles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "Roles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_UserRole_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Command_UserId",
            //    table: "Command",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Hosts_Address_Port",
            //    table: "Hosts",
            //    columns: new[] { "Address", "Port" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Messages_UserId",
            //    table: "Messages",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Playlist_UniqueId",
            //    table: "Playlist",
            //    column: "UniqueId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Playlist_UserId",
            //    table: "Playlist",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserRole_RoleId",
            //    table: "UserRole",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_ConnectionStateId",
            //    table: "Users",
            //    column: "ConnectionStateId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_Email",
            //    table: "Users",
            //    column: "Email",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_FirstName_LastName",
            //    table: "Users",
            //    columns: new[] { "FirstName", "LastName" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_WebGrabConfigDockers_HostId",
            //    table: "WebGrabConfigDockers",
            //    column: "HostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Command");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "WebGrabConfigDockers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.DropTable(
                name: "ConnectionState");
        }
    }
}
