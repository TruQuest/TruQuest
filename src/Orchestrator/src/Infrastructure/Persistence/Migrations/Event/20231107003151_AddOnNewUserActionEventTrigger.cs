using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddOnNewUserActionEventTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.AddUserIdToEvent.v0.sql");
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Triggers.OnNewUserActionEvent.v0.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
