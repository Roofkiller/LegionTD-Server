using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class IdForUnitRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            SET FOREIGN_KEY_CHECKS=0;
            ALTER TABLE `PlayerUnitRelations`
                DROP FOREIGN KEY `FK_PlayerUnitRelations_PlayerMatchData_MatchId_PlayerId`,
                DROP PRIMARY KEY,
                ADD `Id` bigint NOT NULL AUTO_INCREMENT FIRST,
                ADD CONSTRAINT `PK_PlayerUnitRelations` PRIMARY KEY (`Id`),
                ADD CONSTRAINT `FK_PlayerUnitRelations_Units_UnitName` FOREIGN KEY (`UnitName`) REFERENCES `Units` (`Name`) ON DELETE RESTRICT,
                ADD CONSTRAINT `FK_PlayerUnitRelations_PlayerMatchData_MatchId_PlayerId` FOREIGN KEY (`MatchId`, `PlayerId`) REFERENCES `PlayerMatchData` (`MatchId`, `PlayerId`) ON DELETE RESTRICT;
            SET FOREIGN_KEY_CHECKS=1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerUnitRelations_MatchId_PlayerId",
                table: "PlayerUnitRelations",
                columns: new[] { "MatchId", "PlayerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerUnitRelations",
                table: "PlayerUnitRelations");

            migrationBuilder.DropIndex(
                name: "IX_PlayerUnitRelations_MatchId_PlayerId",
                table: "PlayerUnitRelations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlayerUnitRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerUnitRelations",
                table: "PlayerUnitRelations",
                columns: new[] { "MatchId", "PlayerId", "UnitName" });
        }
    }
}
