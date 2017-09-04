using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class ReaddedUnitAbilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "UnitAbilities",
                columns: table => new
                {
                    UnitName = table.Column<string>(type: "varchar(127)", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    AbilityName = table.Column<string>(type: "varchar(127)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitAbilities", x => new { x.UnitName, x.Slot });
                    table.ForeignKey(
                        name: "FK_UnitAbilities_Abilities_AbilityName",
                        column: x => x.AbilityName,
                        principalTable: "Abilities",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitAbilities");
        }
    }
}
