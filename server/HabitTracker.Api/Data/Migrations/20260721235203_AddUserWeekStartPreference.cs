using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWeekStartPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "week_starts_on",
                table: "user_settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Monday");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_settings_week_starts_on",
                table: "user_settings",
                sql: "\"week_starts_on\" IN (\n    'Monday',\n    'Tuesday',\n    'Wednesday',\n    'Thursday',\n    'Friday',\n    'Saturday',\n    'Sunday'\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_settings_week_starts_on",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "week_starts_on",
                table: "user_settings");
        }
    }
}
