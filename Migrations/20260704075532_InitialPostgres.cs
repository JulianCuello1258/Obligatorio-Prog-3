using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: false),
                    Longitud = table.Column<double>(type: "double precision", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    SeccionPolicial = table.Column<string>(type: "text", nullable: false),
                    Zona = table.Column<string>(type: "text", nullable: false),
                    TrashumanciaHabilitada = table.Column<bool>(type: "boolean", nullable: false),
                    Departamento = table.Column<string>(type: "text", nullable: true),
                    Paraje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apiarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exportaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CantidadBarriles = table.Column<int>(type: "integer", nullable: false),
                    Destino = table.Column<string>(type: "text", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exportaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colmenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiarioId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Poblacion = table.Column<string>(type: "text", nullable: true),
                    Temperamento = table.Column<string>(type: "text", nullable: true),
                    ProduccionAcumulada = table.Column<double>(type: "double precision", nullable: false)
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
                name: "Extracciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColmenaId = table.Column<int>(type: "integer", nullable: false),
                    CantidadKg = table.Column<double>(type: "double precision", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExportacionId = table.Column<int>(type: "integer", nullable: true)
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
                    ColmenaId = table.Column<int>(type: "integer", nullable: false),
                    Salud = table.Column<string>(type: "text", nullable: false),
                    Presencia = table.Column<bool>(type: "boolean", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiarioId = table.Column<int>(type: "integer", nullable: true),
                    ColmenaId = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Sintomas = table.Column<string>(type: "text", nullable: true),
                    Enfermedades = table.Column<string>(type: "text", nullable: true),
                    Tratamiento = table.Column<string>(type: "text", nullable: true),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    ProximaDosis = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PoblacionEstimada = table.Column<string>(type: "text", nullable: true),
                    Temperamento = table.Column<string>(type: "text", nullable: true),
                    ReinaPresente = table.Column<bool>(type: "boolean", nullable: false),
                    ReinaSalud = table.Column<string>(type: "text", nullable: true),
                    HayCrias = table.Column<bool>(type: "boolean", nullable: false),
                    Plagas = table.Column<string>(type: "text", nullable: true),
                    NivelInfestacion = table.Column<string>(type: "text", nullable: true),
                    ResultadoTratamiento = table.Column<string>(type: "text", nullable: true),
                    Enjambrazon = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revisiones_Apiarios_ApiarioId",
                        column: x => x.ApiarioId,
                        principalTable: "Apiarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Revisiones_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiarioId = table.Column<int>(type: "integer", nullable: true),
                    ColmenaId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Completada = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Trashumancias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiarioOrigenId = table.Column<int>(type: "integer", nullable: false),
                    ApiarioDestinoId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistanciaKm = table.Column<double>(type: "double precision", nullable: false),
                    ColmenaId = table.Column<int>(type: "integer", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_Trashumancias_Colmenas_ColmenaId",
                        column: x => x.ColmenaId,
                        principalTable: "Colmenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clima",
                columns: table => new
                {
                    RevisionId = table.Column<int>(type: "integer", nullable: false),
                    Temperatura = table.Column<double>(type: "double precision", nullable: true),
                    Humedad = table.Column<double>(type: "double precision", nullable: true),
                    Presion = table.Column<double>(type: "double precision", nullable: true),
                    VelocidadViento = table.Column<double>(type: "double precision", nullable: true),
                    DireccionViento = table.Column<string>(type: "text", nullable: true)
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
                name: "IX_Revisiones_ApiarioId",
                table: "Revisiones",
                column: "ApiarioId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Trashumancias_ColmenaId",
                table: "Trashumancias",
                column: "ColmenaId");
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
