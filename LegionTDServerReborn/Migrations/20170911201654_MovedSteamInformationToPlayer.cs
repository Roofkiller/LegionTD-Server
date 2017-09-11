using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class MovedSteamInformationToPlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamInformation");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Players",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonaName",
                table: "Players",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileUrl",
                table: "Players",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RealName",
                table: "Players",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PersonaName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ProfileUrl",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RealName",
                table: "Players");

            migrationBuilder.CreateTable(
                name: "SteamInformation",
                columns: table => new
                {
                    SteamId = table.Column<long>(nullable: false),
                    Time = table.Column<DateTimeOffset>(nullable: false),
                    Avatar = table.Column<string>(nullable: true),
                    PersonaName = table.Column<string>(nullable: true),
                    ProfileUrl = table.Column<string>(nullable: true),
                    RealName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamInformation", x => new { x.SteamId, x.Time });
                    table.ForeignKey(
                        name: "FK_SteamInformation_Players_SteamId",
                        column: x => x.SteamId,
                        principalTable: "Players",
                        principalColumn: "SteamId",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
