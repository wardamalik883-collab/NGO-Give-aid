using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAid.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryReplyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminReply",
                table: "ContactQueries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepliedAt",
                table: "ContactQueries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactQueries_UserId",
                table: "ContactQueries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactQueries_AspNetUsers_UserId",
                table: "ContactQueries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactQueries_AspNetUsers_UserId",
                table: "ContactQueries");

            migrationBuilder.DropIndex(
                name: "IX_ContactQueries_UserId",
                table: "ContactQueries");

            migrationBuilder.DropColumn(
                name: "AdminReply",
                table: "ContactQueries");

            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "ContactQueries");
        }
    }
}
