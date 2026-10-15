using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAid.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammeInterestFieldsAndUserRel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ProgrammeInterests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestType",
                table: "ProgrammeInterests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ProgrammeInterests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ProgrammeInterests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammeInterests_UserId",
                table: "ProgrammeInterests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgrammeInterests_AspNetUsers_UserId",
                table: "ProgrammeInterests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgrammeInterests_AspNetUsers_UserId",
                table: "ProgrammeInterests");

            migrationBuilder.DropIndex(
                name: "IX_ProgrammeInterests_UserId",
                table: "ProgrammeInterests");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ProgrammeInterests");

            migrationBuilder.DropColumn(
                name: "InterestType",
                table: "ProgrammeInterests");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ProgrammeInterests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ProgrammeInterests");
        }
    }
}
