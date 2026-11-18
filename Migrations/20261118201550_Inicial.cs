using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecetasApi.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CaloriasPor100g = table.Column<double>(type: "REAL", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alimentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Porciones = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoPreparacionMin = table.Column<int>(type: "INTEGER", nullable: false),
                    Dificultad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecetaAlimentos",
                columns: table => new
                {
                    RecetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlimentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<double>(type: "REAL", nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetaAlimentos", x => new { x.RecetaId, x.AlimentoId });
                    table.ForeignKey(
                        name: "FK_RecetaAlimentos_Alimentos_AlimentoId",
                        column: x => x.AlimentoId,
                        principalTable: "Alimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetaAlimentos_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "Recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Alimentos",
                columns: new[] { "Id", "CaloriasPor100g", "Categoria", "FechaActualizacion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, 165.0, "Carne", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pechuga de pollo" },
                    { 2, 360.0, "Cereal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Arroz" },
                    { 3, 18.0, "Verdura", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tomate" },
                    { 4, 884.0, "Grasa", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aceite de oliva" },
                    { 5, 143.0, "Otro", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Huevo" },
                    { 6, 61.0, "Lacteo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Leche entera" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_Nombre",
                table: "Alimentos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecetaAlimentos_AlimentoId",
                table: "RecetaAlimentos",
                column: "AlimentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecetaAlimentos");

            migrationBuilder.DropTable(
                name: "Alimentos");

            migrationBuilder.DropTable(
                name: "Recetas");
        }
    }
}
