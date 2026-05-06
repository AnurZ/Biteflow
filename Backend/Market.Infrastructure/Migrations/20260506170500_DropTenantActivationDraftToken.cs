using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260506170500_DropTenantActivationDraftToken")]
    public partial class DropTenantActivationDraftToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_TenantActivationRequest_DraftToken'
                      AND object_id = OBJECT_ID(N'[dbo].[TenantActivationRequest]')
                )
                BEGIN
                    DROP INDEX [IX_TenantActivationRequest_DraftToken]
                    ON [dbo].[TenantActivationRequest];
                END

                IF COL_LENGTH(N'dbo.TenantActivationRequest', N'DraftToken') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[TenantActivationRequest]
                    DROP COLUMN [DraftToken];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DraftToken",
                table: "TenantActivationRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_TenantActivationRequest_DraftToken",
                table: "TenantActivationRequest",
                column: "DraftToken",
                unique: true);
        }
    }
}
