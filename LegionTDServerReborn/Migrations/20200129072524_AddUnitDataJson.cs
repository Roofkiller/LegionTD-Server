using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class AddUnitDataJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitData",
                table: "PlayerMatchData",
                type: "json",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitData",
                table: "PlayerMatchData");
        }
    }
}
