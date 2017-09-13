using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedBuilders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Units_ParentName",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_ParentName",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Units");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Units",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Units",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Abilities",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Abilities");

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Units",
                nullable: true);

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
    }
}
