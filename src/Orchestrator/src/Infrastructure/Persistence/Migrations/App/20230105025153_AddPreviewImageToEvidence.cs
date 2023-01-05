using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddPreviewImageToEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "truquest",
                table: "Things",
                newName: "ImageIpfsCid");

            migrationBuilder.RenameColumn(
                name: "TruUrl",
                schema: "truquest",
                table: "SupportingEvidence",
                newName: "PreviewImageIpfsCid");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "truquest",
                table: "Subjects",
                newName: "ImageIpfsCid");

            migrationBuilder.RenameColumn(
                name: "TruUrl",
                schema: "truquest",
                table: "Evidence",
                newName: "PreviewImageIpfsCid");

            migrationBuilder.AddColumn<string>(
                name: "IpfsCid",
                schema: "truquest",
                table: "SupportingEvidence",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IpfsCid",
                schema: "truquest",
                table: "Evidence",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpfsCid",
                schema: "truquest",
                table: "SupportingEvidence");

            migrationBuilder.DropColumn(
                name: "IpfsCid",
                schema: "truquest",
                table: "Evidence");

            migrationBuilder.RenameColumn(
                name: "ImageIpfsCid",
                schema: "truquest",
                table: "Things",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "PreviewImageIpfsCid",
                schema: "truquest",
                table: "SupportingEvidence",
                newName: "TruUrl");

            migrationBuilder.RenameColumn(
                name: "ImageIpfsCid",
                schema: "truquest",
                table: "Subjects",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "PreviewImageIpfsCid",
                schema: "truquest",
                table: "Evidence",
                newName: "TruUrl");
        }
    }
}
