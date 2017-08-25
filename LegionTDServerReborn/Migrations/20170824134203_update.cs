using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Builds",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Kills",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Leaks",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostDuels",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sends",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Won",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WonDuels",
                table: "PlayerMatchDatas",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Builds",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "Kills",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "Leaks",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "LostDuels",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "Sends",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "Won",
                table: "PlayerMatchDatas");

            migrationBuilder.DropColumn(
                name: "WonDuels",
                table: "PlayerMatchDatas");
        }
    }
}
