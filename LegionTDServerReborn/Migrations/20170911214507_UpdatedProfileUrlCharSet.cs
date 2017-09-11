using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class UpdatedProfileUrlCharSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProfileUrl",
                table: "Players",
                type: "VARCHAR(511) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProfileUrl",
                table: "Players",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(511) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci",
                oldNullable: true);
        }
    }
}
