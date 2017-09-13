using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedUnitStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitStatistics",
                columns: table => new
                {
                    UnitName = table.Column<string>(type: "varchar(127)", nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Build = table.Column<int>(type: "int", nullable: false),
                    GamesBuild = table.Column<int>(type: "int", nullable: false),
                    GamesEvaluated = table.Column<int>(type: "int", nullable: false),
                    GamesWon = table.Column<int>(type: "int", nullable: false),
                    Killed = table.Column<int>(type: "int", nullable: false),
                    Leaked = table.Column<int>(type: "int", nullable: false),
                    Send = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitStatistics", x => new { x.UnitName, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_UnitStatistics_Units_UnitName",
                        column: x => x.UnitName,
                        principalTable: "Units",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitStatistics");
        }
    }
}
