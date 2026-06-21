using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Renova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11111111-1111-1111-1111-111111111111", "11111111-1111-1111-1111-111111111111", "Administrador", "ADMINISTRADOR" },
                    { "22222222-2222-2222-2222-222222222222", "22222222-2222-2222-2222-222222222222", "Coordenador", "COORDENADOR" },
                    { "33333333-3333-3333-3333-333333333333", "33333333-3333-3333-3333-333333333333", "Profissional", "PROFISSIONAL" },
                    { "44444444-4444-4444-4444-444444444444", "44444444-4444-4444-4444-444444444444", "Atendente", "ATENDENTE" },
                    { "55555555-5555-5555-5555-555555555555", "55555555-5555-5555-5555-555555555555", "Professor", "PROFESSOR" },
                    { "66666666-6666-6666-6666-666666666666", "66666666-6666-6666-6666-666666666666", "Aluno", "ALUNO" },
                    { "77777777-7777-7777-7777-777777777777", "77777777-7777-7777-7777-777777777777", "Familiar", "FAMILIAR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "44444444-4444-4444-4444-444444444444");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "55555555-5555-5555-5555-555555555555");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "66666666-6666-6666-6666-666666666666");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "77777777-7777-7777-7777-777777777777");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");
        }
    }
}
