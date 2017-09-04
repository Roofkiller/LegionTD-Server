using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class RemovedUnitAbilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitAbilities");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Abilities_UnitName",
                table: "Abilities");

            migrationBuilder.CreateTable(
                name: "UnitAbilities",
                columns: table => new
                {
                    UnitName = table.Column<string>(nullable: false),
                    AbilityName = table.Column<string>(nullable: false),
                    Slot = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitAbilities", x => new { x.UnitName, x.AbilityName });
                    table.ForeignKey(
                        name: "FK_UnitAbilities_Abilities_AbilityName",
                        column: x => x.AbilityName,
                        principalTable: "Abilities",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitAbilities_Units_UnitName",
                        column: x => x.UnitName,
                        principalTable: "Units",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitAbilities_AbilityName",
                table: "UnitAbilities",
                column: "AbilityName");
        }
    }
}
