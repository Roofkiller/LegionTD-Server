using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedFractionData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FractionDatas",
                columns: table => new
                {
                    PlayerId = table.Column<long>(nullable: false),
                    FractionName = table.Column<string>(nullable: false),
                    Killed = table.Column<long>(nullable: false),
                    Played = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FractionDatas", x => new { x.PlayerId, x.FractionName });
                    table.ForeignKey(
                        name: "FK_FractionDatas_Fractions_FractionName",
                        column: x => x.FractionName,
                        principalTable: "Fractions",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FractionDatas_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "SteamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FractionDatas_FractionName",
                table: "FractionDatas",
                column: "FractionName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FractionDatas");
        }
    }
}
