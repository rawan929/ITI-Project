using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_MedicalInfo_To_Donor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChronicConditions",
                table: "Donors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "Donors",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KnownAllergies",
                table: "Donors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "Donors",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChronicConditions",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "KnownAllergies",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Donors");
        }
    }
}
