using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddThing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Things",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    ImageURL = table.Column<string>(type: "text", nullable: true),
                    SubmitterId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Things", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Things_AspNetUsers_SubmitterId",
                        column: x => x.SubmitterId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Things_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "truquest",
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evidence",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginURL = table.Column<string>(type: "text", nullable: false),
                    TruURL = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evidence_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThingAttachedTag",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingAttachedTag", x => new { x.ThingId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ThingAttachedTag_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "truquest",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThingAttachedTag_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ThingId",
                schema: "truquest",
                table: "Evidence",
                column: "ThingId");

            migrationBuilder.CreateIndex(
                name: "IX_ThingAttachedTag_TagId",
                schema: "truquest",
                table: "ThingAttachedTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Things_SubjectId",
                schema: "truquest",
                table: "Things",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Things_SubmitterId",
                schema: "truquest",
                table: "Things",
                column: "SubmitterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evidence",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "ThingAttachedTag",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Things",
                schema: "truquest");
        }
    }
}
