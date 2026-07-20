using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitAttributeRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "habit_attribute_rewards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    habit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    xp_amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_habit_attribute_rewards", x => x.id);
                    table.CheckConstraint("ck_habit_attribute_rewards_attribute_type", "\"attribute_type\" IN (\n    'Discipline',\n    'Fitness',\n    'Vitality',\n    'Focus',\n    'Mind',\n    'Resilience',\n    'Social',\n    'Purpose'\n)");
                    table.CheckConstraint("ck_habit_attribute_rewards_xp_amount", "\"xp_amount\" > 0");
                    table.ForeignKey(
                        name: "fk_habit_attribute_rewards_habits_habit_id",
                        column: x => x.habit_id,
                        principalTable: "habits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_habit_attribute_rewards_habit_id_attribute_type",
                table: "habit_attribute_rewards",
                columns: new[] { "habit_id", "attribute_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "habit_attribute_rewards");
        }
    }
}
