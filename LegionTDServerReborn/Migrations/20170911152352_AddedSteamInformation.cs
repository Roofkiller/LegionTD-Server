using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddedSteamInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchDatas_Fractions_FractionName",
                table: "PlayerMatchDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchDatas_Matches_MatchId",
                table: "PlayerMatchDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchDatas_Players_PlayerId",
                table: "PlayerMatchDatas");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerUnitRelations_PlayerMatchDatas_MatchId_PlayerId",
                table: "PlayerUnitRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerMatchDatas",
                table: "PlayerMatchDatas");

            migrationBuilder.RenameTable(
                name: "PlayerMatchDatas",
                newName: "PlayerMatchData");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerMatchDatas_PlayerId",
                table: "PlayerMatchData",
                newName: "IX_PlayerMatchData_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerMatchDatas_FractionName",
                table: "PlayerMatchData",
                newName: "IX_PlayerMatchData_FractionName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerMatchData",
                table: "PlayerMatchData",
                columns: new[] { "MatchId", "PlayerId" });

            migrationBuilder.CreateTable(
                name: "SteamInformation",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Avatar = table.Column<string>(type: "longtext", nullable: true),
                    PersonaName = table.Column<string>(type: "longtext", nullable: true),
                    ProfileUrl = table.Column<string>(type: "longtext", nullable: true),
                    RealName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamInformation", x => new { x.PlayerId, x.Time });
                    table.ForeignKey(
                        name: "FK_SteamInformation_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "SteamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchData_Fractions_FractionName",
                table: "PlayerMatchData",
                column: "FractionName",
                principalTable: "Fractions",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchData_Matches_MatchId",
                table: "PlayerMatchData",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchData_Players_PlayerId",
                table: "PlayerMatchData",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "SteamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerUnitRelations_PlayerMatchData_MatchId_PlayerId",
                table: "PlayerUnitRelations",
                columns: new[] { "MatchId", "PlayerId" },
                principalTable: "PlayerMatchData",
                principalColumns: new[] { "MatchId", "PlayerId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchData_Fractions_FractionName",
                table: "PlayerMatchData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchData_Matches_MatchId",
                table: "PlayerMatchData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchData_Players_PlayerId",
                table: "PlayerMatchData");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerUnitRelations_PlayerMatchData_MatchId_PlayerId",
                table: "PlayerUnitRelations");

            migrationBuilder.DropTable(
                name: "SteamInformation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerMatchData",
                table: "PlayerMatchData");

            migrationBuilder.RenameTable(
                name: "PlayerMatchData",
                newName: "PlayerMatchDatas");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerMatchData_PlayerId",
                table: "PlayerMatchDatas",
                newName: "IX_PlayerMatchDatas_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerMatchData_FractionName",
                table: "PlayerMatchDatas",
                newName: "IX_PlayerMatchDatas_FractionName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerMatchDatas",
                table: "PlayerMatchDatas",
                columns: new[] { "MatchId", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchDatas_Fractions_FractionName",
                table: "PlayerMatchDatas",
                column: "FractionName",
                principalTable: "Fractions",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchDatas_Matches_MatchId",
                table: "PlayerMatchDatas",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchDatas_Players_PlayerId",
                table: "PlayerMatchDatas",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "SteamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerUnitRelations_PlayerMatchDatas_MatchId_PlayerId",
                table: "PlayerUnitRelations",
                columns: new[] { "MatchId", "PlayerId" },
                principalTable: "PlayerMatchDatas",
                principalColumns: new[] { "MatchId", "PlayerId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
