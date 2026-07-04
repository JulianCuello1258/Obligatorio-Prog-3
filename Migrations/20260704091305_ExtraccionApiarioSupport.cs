using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class ExtraccionApiarioSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Extracciones_Colmenas_ColmenaId",
                table: "Extracciones");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Trashumancias",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaProgramada",
                table: "Tareas",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProximaDosis",
                table: "Revisiones",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Revisiones",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Reinas",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Extracciones",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "ColmenaId",
                table: "Extracciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ApiarioId",
                table: "Extracciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Exportaciones",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Colmenas",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Extracciones_ApiarioId",
                table: "Extracciones",
                column: "ApiarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Extracciones_Apiarios_ApiarioId",
                table: "Extracciones",
                column: "ApiarioId",
                principalTable: "Apiarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Extracciones_Colmenas_ColmenaId",
                table: "Extracciones",
                column: "ColmenaId",
                principalTable: "Colmenas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Extracciones_Apiarios_ApiarioId",
                table: "Extracciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Extracciones_Colmenas_ColmenaId",
                table: "Extracciones");

            migrationBuilder.DropIndex(
                name: "IX_Extracciones_ApiarioId",
                table: "Extracciones");

            migrationBuilder.DropColumn(
                name: "ApiarioId",
                table: "Extracciones");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Trashumancias",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaProgramada",
                table: "Tareas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProximaDosis",
                table: "Revisiones",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Revisiones",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Reinas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Extracciones",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<int>(
                name: "ColmenaId",
                table: "Extracciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Exportaciones",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Colmenas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_Extracciones_Colmenas_ColmenaId",
                table: "Extracciones",
                column: "ColmenaId",
                principalTable: "Colmenas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
