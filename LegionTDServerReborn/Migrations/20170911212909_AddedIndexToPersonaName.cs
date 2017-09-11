using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedIndexToPersonaName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RealName",
                table: "Players",
                type: "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PersonaName",
                table: "Players",
                type: "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_PersonaName",
                table: "Players",
                column: "PersonaName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_PersonaName",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "RealName",
                table: "Players",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PersonaName",
                table: "Players",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                oldNullable: true);
        }
    }
}
