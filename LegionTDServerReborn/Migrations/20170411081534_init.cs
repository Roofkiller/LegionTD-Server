using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fractions",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fractions", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<float>(nullable: false),
                    LastWave = table.Column<int>(nullable: false),
                    Winner = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    SteamId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.SteamId);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Experience = table.Column<int>(nullable: false),
                    FractionName = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Units_Fractions_FractionName",
                        column: x => x.FractionName,
                        principalTable: "Fractions",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Duels",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<float>(nullable: false),
                    Winner = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duels", x => new { x.MatchId, x.Order });
                    table.ForeignKey(
                        name: "FK_Duels_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchDatas",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<long>(nullable: false),
                    Abandoned = table.Column<bool>(nullable: false),
                    EarnedGold = table.Column<int>(nullable: false),
                    EarnedTangos = table.Column<int>(nullable: false),
                    FractionName = table.Column<string>(nullable: true),
                    RatingChange = table.Column<int>(nullable: false),
                    Team = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchDatas", x => new { x.MatchId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayerMatchDatas_Fractions_FractionName",
                        column: x => x.FractionName,
                        principalTable: "Fractions",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchDatas_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerMatchDatas_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "SteamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerUnitRelations",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<long>(nullable: false),
                    UnitName = table.Column<string>(nullable: false),
                    Build = table.Column<int>(nullable: false),
                    Killed = table.Column<int>(nullable: false),
                    Leaked = table.Column<int>(nullable: false),
                    Send = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerUnitRelations", x => new { x.MatchId, x.PlayerId, x.UnitName });
                    table.ForeignKey(
                        name: "FK_PlayerUnitRelations_Units_UnitName",
                        column: x => x.UnitName,
                        principalTable: "Units",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerUnitRelations_PlayerMatchDatas_MatchId_PlayerId",
                        columns: x => new { x.MatchId, x.PlayerId },
                        principalTable: "PlayerMatchDatas",
                        principalColumns: new[] { "MatchId", "PlayerId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchDatas_FractionName",
                table: "PlayerMatchDatas",
                column: "FractionName");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchDatas_PlayerId",
                table: "PlayerMatchDatas",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerUnitRelations_UnitName",
                table: "PlayerUnitRelations",
                column: "UnitName");

            migrationBuilder.CreateIndex(
                name: "IX_Units_FractionName",
                table: "Units",
                column: "FractionName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Duels");

            migrationBuilder.DropTable(
                name: "PlayerUnitRelations");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "PlayerMatchDatas");

            migrationBuilder.DropTable(
                name: "Fractions");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
