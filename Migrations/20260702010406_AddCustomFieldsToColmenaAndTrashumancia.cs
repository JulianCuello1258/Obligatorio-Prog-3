using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeKeeperApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldsToColmenaAndTrashumancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColmenaId",
                table: "Trashumancias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing trashumancia records to reference a valid colmena ID before adding constraint
            migrationBuilder.Sql("UPDATE Trashumancias SET ColmenaId = (SELECT TOP 1 Id FROM Colmenas)");

            migrationBuilder.AddColumn<double>(
                name: "ProduccionAcumulada",
                table: "Colmenas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Trashumancias_ColmenaId",
                table: "Trashumancias",
                column: "ColmenaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trashumancias_Colmenas_ColmenaId",
                table: "Trashumancias",
                column: "ColmenaId",
                principalTable: "Colmenas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trashumancias_Colmenas_ColmenaId",
                table: "Trashumancias");

            migrationBuilder.DropIndex(
                name: "IX_Trashumancias_ColmenaId",
                table: "Trashumancias");

            migrationBuilder.DropColumn(
                name: "ColmenaId",
                table: "Trashumancias");

            migrationBuilder.DropColumn(
                name: "ProduccionAcumulada",
                table: "Colmenas");
        }
    }
}
