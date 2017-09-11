using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class RenamedSteamInformationKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamInformation_Players_PlayerId",
                table: "SteamInformation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamInformation",
                table: "SteamInformation");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "SteamInformation");

            migrationBuilder.AddColumn<long>(
                name: "SteamId",
                table: "SteamInformation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamInformation",
                table: "SteamInformation",
                columns: new[] { "SteamId", "Time" });

            migrationBuilder.AddForeignKey(
                name: "FK_SteamInformation_Players_SteamId",
                table: "SteamInformation",
                column: "SteamId",
                principalTable: "Players",
                principalColumn: "SteamId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamInformation_Players_SteamId",
                table: "SteamInformation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamInformation",
                table: "SteamInformation");

            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "SteamInformation");

            migrationBuilder.AddColumn<long>(
                name: "PlayerId",
                table: "SteamInformation",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamInformation",
                table: "SteamInformation",
                columns: new[] { "PlayerId", "Time" });

            migrationBuilder.AddForeignKey(
                name: "FK_SteamInformation_Players_PlayerId",
                table: "SteamInformation",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "SteamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
