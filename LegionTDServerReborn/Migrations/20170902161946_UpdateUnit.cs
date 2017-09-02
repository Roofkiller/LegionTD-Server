using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class UpdateUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FractionDatas");

            migrationBuilder.AddColumn<string>(
                name: "Ability1",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability2",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability3",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ability4",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ArmorPhysical",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "AttackDamageMax",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "AttackDamageMin",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "AttackRange",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "AttackRate",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BountyGoldMax",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BountyGoldMin",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "LegionAttackType",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegionDefendType",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MagicResistance",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Units",
                type: "varchar(127)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "StatusHealth",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StatusHealthRegen",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StatusMana",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StatusManaRegen",
                table: "Units",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_Units_ParentName",
                table: "Units",
                column: "ParentName");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Units_ParentName",
                table: "Units",
                column: "ParentName",
                principalTable: "Units",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Units_ParentName",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_ParentName",
                table: "Units");

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

            migrationBuilder.DropColumn(
                name: "ArmorPhysical",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AttackDamageMax",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AttackDamageMin",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AttackRange",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AttackRate",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "BountyGoldMax",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "BountyGoldMin",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "LegionAttackType",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "LegionDefendType",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "MagicResistance",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StatusHealth",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StatusHealthRegen",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StatusMana",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StatusManaRegen",
                table: "Units");

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
    }
}
