using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExtractHUContext.WriteSide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHUExtractionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExtractionCompletedAt",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractionError",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExtractionStartedAt",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtractionStatus",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MaxHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MeanHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StandardDeviation",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VoxelCount",
                schema: "quantum_hu_context",
                table: "Medical_Studies",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractionCompletedAt",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "ExtractionError",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "ExtractionStartedAt",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "ExtractionStatus",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "MaxHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "MeanHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "MinHU",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "StandardDeviation",
                schema: "quantum_hu_context",
                table: "Medical_Studies");

            migrationBuilder.DropColumn(
                name: "VoxelCount",
                schema: "quantum_hu_context",
                table: "Medical_Studies");
        }
    }
}
