using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    current_xp = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_attributes", x => x.id);
                    table.CheckConstraint("ck_user_attributes_attribute_type", "\"attribute_type\" IN (\n    'Discipline',\n    'Fitness',\n    'Vitality',\n    'Focus',\n    'Mind',\n    'Resilience',\n    'Social',\n    'Purpose'\n)");
                    table.CheckConstraint("ck_user_attributes_current_xp", "\"current_xp\" >= 0");
                    table.ForeignKey(
                        name: "fk_user_attributes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_attributes_user_id_attribute_type",
                table: "user_attributes",
                columns: new[] { "user_id", "attribute_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_attributes");
        }
    }
}
