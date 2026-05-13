using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class AddExportacionesYClimaRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Tareas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionViento",
                table: "Revisiones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enjambrazon",
                table: "Revisiones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Humedad",
                table: "Revisiones",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NivelInfestacion",
                table: "Revisiones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plagas",
                table: "Revisiones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Presion",
                table: "Revisiones",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoTratamiento",
                table: "Revisiones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Temperatura",
                table: "Revisiones",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VelocidadViento",
                table: "Revisiones",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Exportaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CantidadBarriles = table.Column<int>(type: "int", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exportaciones", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exportaciones");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "DireccionViento",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "Enjambrazon",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "Humedad",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "NivelInfestacion",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "Plagas",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "Presion",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "ResultadoTratamiento",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "Temperatura",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "VelocidadViento",
                table: "Revisiones");
        }
    }
}
