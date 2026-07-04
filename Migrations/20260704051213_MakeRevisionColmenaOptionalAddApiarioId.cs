using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeRevisionColmenaOptionalAddApiarioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revisiones_Colmenas_ColmenaId",
                table: "Revisiones");

            migrationBuilder.AlterColumn<int>(
                name: "ColmenaId",
                table: "Revisiones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ApiarioId",
                table: "Revisiones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Revisiones_ApiarioId",
                table: "Revisiones",
                column: "ApiarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Revisiones_Apiarios_ApiarioId",
                table: "Revisiones",
                column: "ApiarioId",
                principalTable: "Apiarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Revisiones_Colmenas_ColmenaId",
                table: "Revisiones",
                column: "ColmenaId",
                principalTable: "Colmenas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revisiones_Apiarios_ApiarioId",
                table: "Revisiones");

            migrationBuilder.DropForeignKey(
                name: "FK_Revisiones_Colmenas_ColmenaId",
                table: "Revisiones");

            migrationBuilder.DropIndex(
                name: "IX_Revisiones_ApiarioId",
                table: "Revisiones");

            migrationBuilder.DropColumn(
                name: "ApiarioId",
                table: "Revisiones");

            migrationBuilder.AlterColumn<int>(
                name: "ColmenaId",
                table: "Revisiones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Revisiones_Colmenas_ColmenaId",
                table: "Revisiones",
                column: "ColmenaId",
                principalTable: "Colmenas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
