using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechStore1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Icono", "Nombre" },
                values: new object[,]
                {
                    { 1, "Computadoras portátiles", "bi bi-laptop", "Laptops" },
                    { 2, "Teléfonos inteligentes", "bi bi-phone", "Smartphones" },
                    { 3, "Periféricos y más", "bi bi-headset", "Accesorios" },
                    { 4, "Pantallas de alta resolución", "bi bi-display", "Monitores" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Estado", "Imagen", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, 1, "Core i7, 16GB RAM, 512GB SSD", true, "novabook-pro-14.jpg", "Laptop Pro X", 1200.00m, 10 },
                    { 2, 2, "Pantalla OLED, 128GB, 5G", true, "pixelwave-x1.jpg", "Smartphone G", 800.00m, 20 },
                    { 3, 3, "Cancelación de ruido act.", true, "sonicpulse-anc.jpg", "Auriculares Inalámbricos", 150.00m, 50 },
                    { 4, 4, "Monitor IPS para diseño", true, "visiondock-27.jpg", "Monitor 4K 27", 350.00m, 15 },
                    { 5, 3, "LIGHTSYNC RGB personalizable", true, "gamecore-m5.jpg", "Mouse gamer RGB", 80.00m, 25 },
                    { 6, 3, "Switches RGB y blue", true, "hyperstrike-k7.jpg", "Teclado Mecánico", 90.00m, 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
