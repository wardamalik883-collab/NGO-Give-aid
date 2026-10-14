using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAid.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammeDatesAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Programmes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Programmes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Programmes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Programmes");
        }
    }
}
