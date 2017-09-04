using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedAbilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FractionStatistic_Fractions_FractionName",
                table: "FractionStatistic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FractionStatistic",
                table: "FractionStatistic");

            migrationBuilder.DropColumn(
                name: "Ability1",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Ability2",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Ability3",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Ability4",
                table: "Units");

            migrationBuilder.RenameTable(
                name: "FractionStatistic",
                newName: "FractionStatistics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FractionStatistics",
                table: "FractionStatistics",
                columns: new[] { "FractionName", "TimeStamp" });

            migrationBuilder.CreateTable(
                name: "Abilities",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(127)", nullable: false),
                    CastRange = table.Column<float>(type: "float", nullable: false),
                    Cooldown = table.Column<float>(type: "float", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false),
                    GoldCost = table.Column<int>(type: "int", nullable: false),
                    ManaCost = table.Column<int>(type: "int", nullable: false),
                    UnitName = table.Column<string>(type: "varchar(127)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Abilities_Units_UnitName",
                        column: x => x.UnitName,
                        principalTable: "Units",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitAbilities",
                columns: table => new
                {
                    UnitName = table.Column<string>(type: "varchar(127)", nullable: false),
                    AbilityName = table.Column<string>(type: "varchar(127)", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_Abilities_UnitName",
                table: "Abilities",
                column: "UnitName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitAbilities_AbilityName",
                table: "UnitAbilities",
                column: "AbilityName");

            migrationBuilder.AddForeignKey(
                name: "FK_FractionStatistics_Fractions_FractionName",
                table: "FractionStatistics",
                column: "FractionName",
                principalTable: "Fractions",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FractionStatistics_Fractions_FractionName",
                table: "FractionStatistics");

            migrationBuilder.DropTable(
                name: "UnitAbilities");

            migrationBuilder.DropTable(
                name: "Abilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FractionStatistics",
                table: "FractionStatistics");

            migrationBuilder.RenameTable(
                name: "FractionStatistics",
                newName: "FractionStatistic");

            migrationBuilder.AddColumn<string>(
                name: "Ability1",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability2",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability3",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability4",
                table: "Units",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FractionStatistic",
                table: "FractionStatistic",
                columns: new[] { "FractionName", "TimeStamp" });

            migrationBuilder.AddForeignKey(
                name: "FK_FractionStatistic_Fractions_FractionName",
                table: "FractionStatistic",
                column: "FractionName",
                principalTable: "Fractions",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
