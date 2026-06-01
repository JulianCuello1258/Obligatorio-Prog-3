using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeccionPolicial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrashumanciaHabilitada = table.Column<bool>(type: "bit", nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paraje = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apiarios", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Colmenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiarioId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Poblacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperamento = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colmenas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colmenas_Apiarios_ApiarioId",
                        column: x => x.ApiarioId,
                        principalTable: "Apiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trashumancias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiarioOrigenId = table.Column<int>(type: "int", nullable: false),
                    ApiarioDestinoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistanciaKm = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trashumancias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trashumancias_Apiarios_ApiarioDestinoId",
                        column: x => x.ApiarioDestinoId,
                        principalTable: "Apiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trashumancias_Apiarios_ApiarioOrigenId",
                        column: x => x.ApiarioOrigenId,
                        principalTable: "Apiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Extracciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColmenaId = table.Column<int>(type: "int", nullable: false),
                    CantidadKg = table.Column<double>(type: "float", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExportacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extracciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Extracciones_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Extracciones_Exportaciones_ExportacionId",
                        column: x => x.ExportacionId,
                        principalTable: "Exportaciones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reinas",
                columns: table => new
                {
                    ColmenaId = table.Column<int>(type: "int", nullable: false),
                    Salud = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Presencia = table.Column<bool>(type: "bit", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reinas", x => x.ColmenaId);
                    table.ForeignKey(
                        name: "FK_Reinas_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Revisiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColmenaId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sintomas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enfermedades = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProximaDosis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PoblacionEstimada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReinaPresente = table.Column<bool>(type: "bit", nullable: false),
                    ReinaSalud = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HayCrias = table.Column<bool>(type: "bit", nullable: false),
                    Plagas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NivelInfestacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultadoTratamiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enjambrazon = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revisiones_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiarioId = table.Column<int>(type: "int", nullable: true),
                    ColmenaId = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Apiarios_ApiarioId",
                        column: x => x.ApiarioId,
                        principalTable: "Apiarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tareas_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Clima",
                columns: table => new
                {
                    RevisionId = table.Column<int>(type: "int", nullable: false),
                    Temperatura = table.Column<double>(type: "float", nullable: true),
                    Humedad = table.Column<double>(type: "float", nullable: true),
                    Presion = table.Column<double>(type: "float", nullable: true),
                    VelocidadViento = table.Column<double>(type: "float", nullable: true),
                    DireccionViento = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clima", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_Clima_Revisiones_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Revisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colmenas_ApiarioId",
                table: "Colmenas",
                column: "ApiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Extracciones_ColmenaId",
                table: "Extracciones",
                column: "ColmenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Extracciones_ExportacionId",
                table: "Extracciones",
                column: "ExportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Revisiones_ColmenaId",
                table: "Revisiones",
                column: "ColmenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_ApiarioId",
                table: "Tareas",
                column: "ApiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_ColmenaId",
                table: "Tareas",
                column: "ColmenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Trashumancias_ApiarioDestinoId",
                table: "Trashumancias",
                column: "ApiarioDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Trashumancias_ApiarioOrigenId",
                table: "Trashumancias",
                column: "ApiarioOrigenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clima");

            migrationBuilder.DropTable(
                name: "Extracciones");

            migrationBuilder.DropTable(
                name: "Reinas");

            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "Trashumancias");

            migrationBuilder.DropTable(
                name: "Revisiones");

            migrationBuilder.DropTable(
                name: "Exportaciones");

            migrationBuilder.DropTable(
                name: "Colmenas");

            migrationBuilder.DropTable(
                name: "Apiarios");
        }
    }
}
