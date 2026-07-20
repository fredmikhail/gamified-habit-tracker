using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddXpTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "xp_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    habit_completion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_xp_transactions", x => x.id);
                    table.CheckConstraint("ck_xp_transactions_amount", "\"amount\" > 0");
                    table.CheckConstraint("ck_xp_transactions_attribute_type", "\"attribute_type\" IN (\n    'Discipline',\n    'Fitness',\n    'Vitality',\n    'Focus',\n    'Mind',\n    'Resilience',\n    'Social',\n    'Purpose'\n)");
                    table.ForeignKey(
                        name: "fk_xp_transactions_habit_completions_habit_completion_id",
                        column: x => x.habit_completion_id,
                        principalTable: "habit_completions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_xp_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_xp_transactions_habit_completion_id_attribute_type",
                table: "xp_transactions",
                columns: new[] { "habit_completion_id", "attribute_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_xp_transactions_user_id_created_at_utc",
                table: "xp_transactions",
                columns: new[] { "user_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "xp_transactions");
        }
    }
}
