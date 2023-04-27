using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "truquest");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ScheduledBlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<IReadOnlyDictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "truquest",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "truquest",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ImageIpfsCid = table.Column<string>(type: "text", nullable: false),
                    CroppedImageIpfsCid = table.Column<string>(type: "text", nullable: false),
                    SubmitterId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_SubmitterId",
                        column: x => x.SubmitterId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectAttachedTags",
                schema: "truquest",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectAttachedTags", x => new { x.SubjectId, x.TagId });
                    table.ForeignKey(
                        name: "FK_SubjectAttachedTags_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalSchema: "truquest",
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectAttachedTags_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "truquest",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Things",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    ImageIpfsCid = table.Column<string>(type: "text", nullable: true),
                    CroppedImageIpfsCid = table.Column<string>(type: "text", nullable: true),
                    SubmitterId = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteAggIpfsCid = table.Column<string>(type: "text", nullable: true),
                    AcceptedSettlementProposalId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    OriginUrl = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false),
                    PreviewImageIpfsCid = table.Column<string>(type: "text", nullable: false),
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
                name: "SettlementProposals",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Verdict = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    ImageIpfsCid = table.Column<string>(type: "text", nullable: true),
                    CroppedImageIpfsCid = table.Column<string>(type: "text", nullable: true),
                    SubmitterId = table.Column<string>(type: "text", nullable: false),
                    VoteAggIpfsCid = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementProposals_AspNetUsers_SubmitterId",
                        column: x => x.SubmitterId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementProposals_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThingAttachedTags",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingAttachedTags", x => new { x.ThingId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ThingAttachedTags_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "truquest",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThingAttachedTags_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThingVerifiers",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifierId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingVerifiers", x => new { x.ThingId, x.VerifierId });
                    table.ForeignKey(
                        name: "FK_ThingVerifiers_AspNetUsers_VerifierId",
                        column: x => x.VerifierId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThingVerifiers_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementProposalVerifiers",
                schema: "truquest",
                columns: table => new
                {
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifierId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposalVerifiers", x => new { x.SettlementProposalId, x.VerifierId });
                    table.ForeignKey(
                        name: "FK_SettlementProposalVerifiers_AspNetUsers_VerifierId",
                        column: x => x.VerifierId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementProposalVerifiers_SettlementProposals_SettlementP~",
                        column: x => x.SettlementProposalId,
                        principalSchema: "truquest",
                        principalTable: "SettlementProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportingEvidence",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginUrl = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false),
                    PreviewImageIpfsCid = table.Column<string>(type: "text", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportingEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportingEvidence_SettlementProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "truquest",
                        principalTable: "SettlementProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcceptancePollVotes",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcceptancePollVotes", x => new { x.ThingId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_AcceptancePollVotes_ThingVerifiers_ThingId_VoterId",
                        columns: x => new { x.ThingId, x.VoterId },
                        principalSchema: "truquest",
                        principalTable: "ThingVerifiers",
                        principalColumns: new[] { "ThingId", "VerifierId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentPollVotes",
                schema: "truquest",
                columns: table => new
                {
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentPollVotes", x => new { x.SettlementProposalId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_AssessmentPollVotes_SettlementProposalVerifiers_SettlementP~",
                        columns: x => new { x.SettlementProposalId, x.VoterId },
                        principalSchema: "truquest",
                        principalTable: "SettlementProposalVerifiers",
                        principalColumns: new[] { "SettlementProposalId", "VerifierId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "truquest",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "truquest",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "truquest",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "truquest",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ThingId",
                schema: "truquest",
                table: "Evidence",
                column: "ThingId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_SubmitterId",
                schema: "truquest",
                table: "SettlementProposals",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_ThingId",
                schema: "truquest",
                table: "SettlementProposals",
                column: "ThingId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposalVerifiers_VerifierId",
                schema: "truquest",
                table: "SettlementProposalVerifiers",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectAttachedTags_TagId",
                schema: "truquest",
                table: "SubjectAttachedTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SubmitterId",
                schema: "truquest",
                table: "Subjects",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingEvidence_ProposalId",
                schema: "truquest",
                table: "SupportingEvidence",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ThingAttachedTags_TagId",
                schema: "truquest",
                table: "ThingAttachedTags",
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

            migrationBuilder.CreateIndex(
                name: "IX_ThingVerifiers_VerifierId",
                schema: "truquest",
                table: "ThingVerifiers",
                column: "VerifierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcceptancePollVotes",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AssessmentPollVotes",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Evidence",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SubjectAttachedTags",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SupportingEvidence",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "ThingAttachedTags",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "ThingVerifiers",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SettlementProposalVerifiers",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SettlementProposals",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Things",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "Subjects",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "truquest");
        }
    }
}
