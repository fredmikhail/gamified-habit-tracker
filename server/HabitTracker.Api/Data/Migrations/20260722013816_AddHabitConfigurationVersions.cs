using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitConfigurationVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "habit_configuration_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),
                    habit_id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false),
                    version_number = table.Column<int>(
                        type: "integer",
                        nullable: false),
                    category = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false),
                    frequency_type = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false),
                    target_count = table.Column<int>(
                        type: "integer",
                        nullable: false),
                    difficulty = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false),
                    effective_from_date = table.Column<DateOnly>(
                        type: "date",
                        nullable: false),
                    effective_to_date_exclusive = table.Column<DateOnly>(
                        type: "date",
                        nullable: true),
                    created_at_utc = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_habit_configuration_versions",
                        x => x.id);

                    table.CheckConstraint(
                        "ck_habit_configuration_versions_difficulty",
                        """
                        "difficulty" IN ('Easy', 'Medium', 'Hard', 'Elite')
                        """);

                    table.CheckConstraint(
                        "ck_habit_configuration_versions_effective_date_range",
                        """
                        "effective_to_date_exclusive" IS NULL
                        OR
                        "effective_to_date_exclusive" > "effective_from_date"
                        """);

                    table.CheckConstraint(
                        "ck_habit_configuration_versions_frequency_target_count",
                        """
                        ("frequency_type" = 'Daily' AND "target_count" = 1)
                        OR
                        ("frequency_type" = 'Weekly' AND "target_count" BETWEEN 1 AND 7)
                        """);

                    table.CheckConstraint(
                        "ck_habit_configuration_versions_version_number",
                        """
                        "version_number" >= 1
                        """);

                    table.ForeignKey(
                        name: "fk_habit_configuration_versions_habits_habit_id",
                        column: x => x.habit_id,
                        principalTable: "habits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO habit_configuration_versions (
                    id,
                    habit_id,
                    version_number,
                    category,
                    frequency_type,
                    target_count,
                    difficulty,
                    effective_from_date,
                    effective_to_date_exclusive,
                    created_at_utc
                )
                SELECT
                    habits.id,
                    habits.id,
                    1,
                    habits.category,
                    habits.frequency_type,
                    habits.target_count,
                    habits.difficulty,
                    (
                        habits.created_at_utc
                        AT TIME ZONE user_settings.time_zone
                    )::date,
                    NULL,
                    habits.created_at_utc
                FROM habits
                INNER JOIN user_settings
                    ON user_settings.user_id = habits.user_id;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_habit_configuration_versions_habit_id",
                table: "habit_configuration_versions",
                column: "habit_id",
                unique: true,
                filter: "\"effective_to_date_exclusive\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_habit_configuration_versions_habit_id_effective_from_date",
                table: "habit_configuration_versions",
                columns: new[]
                {
                    "habit_id",
                    "effective_from_date"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_habit_configuration_versions_habit_id_version_number",
                table: "habit_configuration_versions",
                columns: new[]
                {
                    "habit_id",
                    "version_number"
                },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "habit_configuration_versions");
        }
    }
}