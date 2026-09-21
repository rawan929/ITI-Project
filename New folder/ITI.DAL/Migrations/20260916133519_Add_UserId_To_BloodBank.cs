using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserId_To_BloodBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "BloodBanks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BloodBanks_UserId",
                table: "BloodBanks",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BloodBanks_ApplicationUsers_UserId",
                table: "BloodBanks",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodBanks_ApplicationUsers_UserId",
                table: "BloodBanks");

            migrationBuilder.DropIndex(
                name: "IX_BloodBanks_UserId",
                table: "BloodBanks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BloodBanks");
        }
    }
}
