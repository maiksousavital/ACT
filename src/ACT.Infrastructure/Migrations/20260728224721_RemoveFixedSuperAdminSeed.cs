using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFixedSuperAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Email", "IsActive", "PasswordHash", "Role" },
                values: new object[] { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@act.local", true, "$2a$11$UxNn19pLPpkpcqV/4rWT4OwNL8zxy9JA0oYtVzYhKgaveFZAVrkp.", 0 });
        }
    }
}
