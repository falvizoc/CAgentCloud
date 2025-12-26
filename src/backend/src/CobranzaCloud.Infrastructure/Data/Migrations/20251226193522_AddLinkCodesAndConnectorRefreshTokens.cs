using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CobranzaCloud.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkCodesAndConnectorRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ConnectorId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "link_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MachineFingerprint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConnectorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ConnectorVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_link_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_link_codes_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_link_codes_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ConnectorId",
                table: "refresh_tokens",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_link_codes_Code",
                table: "link_codes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_link_codes_Code_Used_ExpiresAt",
                table: "link_codes",
                columns: new[] { "Code", "Used", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_link_codes_CreatedByUserId",
                table: "link_codes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_link_codes_OrganizationId",
                table: "link_codes",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_connectors_ConnectorId",
                table: "refresh_tokens",
                column: "ConnectorId",
                principalTable: "connectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_connectors_ConnectorId",
                table: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "link_codes");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_ConnectorId",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "ConnectorId",
                table: "refresh_tokens");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
