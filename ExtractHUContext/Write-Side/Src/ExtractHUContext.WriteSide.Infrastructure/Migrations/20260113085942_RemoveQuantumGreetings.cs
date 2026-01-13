using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExtractHUContext.WriteSide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuantumGreetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quantum_Greetings",
                schema: "quantum_hu_context");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quantum_Greetings",
                schema: "quantum_hu_context",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quantum_Greetings", x => x.Id);
                });
        }
    }
}
