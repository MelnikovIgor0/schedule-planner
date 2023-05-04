using FluentMigrator;

namespace DAL.Migrations;

[Migration(20230104, TransactionBehavior.None)]
public sealed class InitSchema : Migration
{
    public override void Up()
    {
        Create.Table("schedules")
            .WithColumn("uuid").AsString(36).PrimaryKey("schedules_pk").NotNullable()
            .WithColumn("input_content").AsString(1048576).NotNullable()
            .WithColumn("output_content").AsString(1048576).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("schedules");
    }
}
