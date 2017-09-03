using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddFractionStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FractionStatistic",
                columns: table => new
                {
                    FractionName = table.Column<string>(type: "varchar(127)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LostGames = table.Column<int>(type: "int", nullable: false),
                    PickRate = table.Column<float>(type: "float", nullable: false),
                    WonGames = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FractionStatistic", x => new { x.FractionName, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_FractionStatistic_Fractions_FractionName",
                        column: x => x.FractionName,
                        principalTable: "Fractions",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FractionStatistic");
        }
    }
}
