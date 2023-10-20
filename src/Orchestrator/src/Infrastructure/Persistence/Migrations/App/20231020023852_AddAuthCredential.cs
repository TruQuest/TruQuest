using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddAuthCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthCredentials",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    SignCount = table.Column<int>(type: "integer", nullable: false),
                    Transports = table.Column<IReadOnlyList<int>>(type: "jsonb", nullable: true),
                    IsBackupEligible = table.Column<bool>(type: "boolean", nullable: false),
                    IsBackedUp = table.Column<bool>(type: "boolean", nullable: false),
                    AttestationObject = table.Column<string>(type: "text", nullable: false),
                    AttestationClientDataJSON = table.Column<string>(type: "text", nullable: false),
                    AttestationFormat = table.Column<string>(type: "text", nullable: false),
                    AddedAt = table.Column<long>(type: "bigint", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthCredentials_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthCredentials_UserId",
                schema: "truquest",
                table: "AuthCredentials",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthCredentials",
                schema: "truquest");
        }
    }
}
