using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertHabitCategoryToControlledValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE habits
                SET category = CASE category
                    WHEN 'Fitness' THEN 'FitnessAndMovement'
                    WHEN 'Learning' THEN 'LearningAndSkills'
                    WHEN 'Recovery' THEN 'HealthAndRecovery'
                END
                WHERE category IN (
                    'Fitness',
                    'Learning',
                    'Recovery'
                );
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM habits
                        WHERE category IS NULL
                           OR category NOT IN (
                               'FitnessAndMovement',
                               'HealthAndRecovery',
                               'LearningAndSkills',
                               'WorkAndCareer',
                               'DailyLifeAndOrganization',
                               'MoneyAndFinance',
                               'RelationshipsAndCommunity',
                               'EmotionalWellBeing',
                               'SpiritualityAndPurpose',
                               'CreativityAndRecreation',
                               'SelfControlAndBoundaries',
                               'GeneralGrowth'
                           )
                    ) THEN
                        RAISE EXCEPTION
                            'Cannot convert habits.category because one or more rows contain NULL or an unsupported category value.';
                    END IF;
                END
                $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "habits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "habits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.Sql(
                """
                UPDATE habits
                SET category = CASE category
                    WHEN 'FitnessAndMovement' THEN 'Fitness'
                    WHEN 'LearningAndSkills' THEN 'Learning'
                    WHEN 'HealthAndRecovery' THEN 'Recovery'
                END
                WHERE category IN (
                    'FitnessAndMovement',
                    'LearningAndSkills',
                    'HealthAndRecovery'
                );
                """);
        }
    }
}