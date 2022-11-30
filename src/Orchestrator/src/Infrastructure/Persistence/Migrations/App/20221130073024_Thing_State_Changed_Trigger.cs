using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class ThingStateChangedTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.App.Scripts.Functions.HandleThingStateChanged.HandleThingStateChanged.v0.sql");
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.App.Scripts.Triggers.Things.OnStateChanged.OnStateChanged.v0.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.App.Scripts.Triggers.Things.OnStateChanged.OnStateChanged.v0.sql");
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.App.Scripts.Functions.HandleThingStateChanged.HandleThingStateChanged.v0.sql");
        }
    }
}
