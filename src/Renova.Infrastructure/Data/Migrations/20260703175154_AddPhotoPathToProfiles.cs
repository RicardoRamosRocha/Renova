using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Renova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Students",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Professionals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "FamilyMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "AspNetUsers");
        }
    }
}
