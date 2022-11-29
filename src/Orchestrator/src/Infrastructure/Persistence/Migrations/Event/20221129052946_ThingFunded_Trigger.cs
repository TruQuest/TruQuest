using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class ThingFundedTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.HandleNewThingFundedEvent.HandleNewThingFundedEvent.v0.sql");
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Triggers.ThingFundedEvents.OnNew.OnNew.v0.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Triggers.ThingFundedEvents.OnNew.OnNew.v0.sql");
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.HandleNewThingFundedEvent.HandleNewThingFundedEvent.v0.sql");
        }
    }
}
