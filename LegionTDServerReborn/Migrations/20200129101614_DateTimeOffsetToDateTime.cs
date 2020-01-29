using Microsoft.EntityFrameworkCore.Migrations;

namespace LegionTDServerReborn.Migrations
{
    public partial class DateTimeOffsetToDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnitData",
                table: "PlayerMatchData",
                type: "JSON",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnitData",
                table: "PlayerMatchData",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "JSON",
                oldNullable: true);
        }
    }
}
