using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_SAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class accesscodeStuff2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessGroupId",
                table: "AccessCodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "AccessCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AccessCodes_AccessGroupId",
                table: "AccessCodes",
                column: "AccessGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessCodes_AccessGroups_AccessGroupId",
                table: "AccessCodes",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessCodes_AccessGroups_AccessGroupId",
                table: "AccessCodes");

            migrationBuilder.DropIndex(
                name: "IX_AccessCodes_AccessGroupId",
                table: "AccessCodes");

            migrationBuilder.DropColumn(
                name: "AccessGroupId",
                table: "AccessCodes");

            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "AccessCodes");
        }
    }
}
