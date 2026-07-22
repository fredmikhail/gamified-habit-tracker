using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkHabitCompletionsToConfigurationVersions
        : Migration
    {
        /// <inheritdoc />
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "habit_configuration_version_id",
                table: "habit_completions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE habit_completions AS completion
                SET habit_configuration_version_id =
                    configuration.id
                FROM habit_configuration_versions
                    AS configuration
                WHERE configuration.habit_id =
                        completion.habit_id
                    AND configuration.effective_from_date
                        <= completion.completed_date
                    AND (
                        configuration
                            .effective_to_date_exclusive
                            IS NULL
                        OR completion.completed_date
                            < configuration
                                .effective_to_date_exclusive
                    );
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM habit_completions
                        WHERE habit_configuration_version_id
                            IS NULL
                    ) THEN
                        RAISE EXCEPTION
                            'A habit completion could not be matched to an effective habit configuration version.';
                    END IF;
                END
                $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "habit_configuration_version_id",
                table: "habit_completions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_habit_completions_habit_configuration_version_id",
                table: "habit_completions",
                column: "habit_configuration_version_id");

            migrationBuilder.AddForeignKey(
                name: "fk_habit_completions_habit_configuration_versions_habit_config",
                table: "habit_completions",
                column: "habit_configuration_version_id",
                principalTable:
                    "habit_configuration_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_habit_completions_habit_configuration_versions_habit_config",
                table: "habit_completions");

            migrationBuilder.DropIndex(
                name: "ix_habit_completions_habit_configuration_version_id",
                table: "habit_completions");

            migrationBuilder.DropColumn(
                name: "habit_configuration_version_id",
                table: "habit_completions");
        }
    }
}
