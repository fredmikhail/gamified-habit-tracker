using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableCompletionUndo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_xp_transactions_habit_completion_id_attribute_type",
                table: "xp_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_xp_transactions_amount",
                table: "xp_transactions");

            migrationBuilder.DropIndex(
                name: "ix_habit_completions_habit_id_completed_date",
                table: "habit_completions");

            migrationBuilder.AddColumn<DateTime>(
                name: "undone_at_utc",
                table: "habit_completions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_xp_transactions_habit_completion_id_attribute_type_reason",
                table: "xp_transactions",
                columns: new[] { "habit_completion_id", "attribute_type", "reason" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_xp_transactions_amount",
                table: "xp_transactions",
                sql: "\"amount\" <> 0");

            migrationBuilder.CreateIndex(
                name: "ix_habit_completions_habit_id_completed_date",
                table: "habit_completions",
                columns: new[] { "habit_id", "completed_date" },
                unique: true,
                filter: "\"undone_at_utc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_xp_transactions_habit_completion_id_attribute_type_reason",
                table: "xp_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_xp_transactions_amount",
                table: "xp_transactions");

            migrationBuilder.DropIndex(
                name: "ix_habit_completions_habit_id_completed_date",
                table: "habit_completions");

            migrationBuilder.DropColumn(
                name: "undone_at_utc",
                table: "habit_completions");

            migrationBuilder.CreateIndex(
                name: "ix_xp_transactions_habit_completion_id_attribute_type",
                table: "xp_transactions",
                columns: new[] { "habit_completion_id", "attribute_type" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_xp_transactions_amount",
                table: "xp_transactions",
                sql: "\"amount\" > 0");

            migrationBuilder.CreateIndex(
                name: "ix_habit_completions_habit_id_completed_date",
                table: "habit_completions",
                columns: new[] { "habit_id", "completed_date" },
                unique: true);
        }
    }
}
