using System;
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { "a1b2c3d4-0001-4000-8000-000000000001", "Administrador", "ADMINISTRADOR", "c3d4e5f6-0001-4000-8000-000000000001" },
                    { "a1b2c3d4-0002-4000-8000-000000000002", "Vendedor", "VENDEDOR", "c3d4e5f6-0002-4000-8000-000000000002" },
                    { "a1b2c3d4-0003-4000-8000-000000000003", "Cliente", "CLIENTE", "c3d4e5f6-0003-4000-8000-000000000003" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount" },
                values: new object[,]
                {
                    { "b2c3d4e5-0001-4000-8000-000000000001", "admin", "ADMIN", null, null, false, "AQAAAAIAAYagAAAAEFRBPdsBwEP2s04we4pn1Fv4+TpSaRSKUEcz2DAeb7TWpWCbDDMmO6GWmlVIjGaOnA==", "d4e5f6a1-0001-4000-8000-000000000001", "e5f6a1b2-0001-4000-8000-000000000001", null, false, false, null, false, 0 },
                    { "b2c3d4e5-0002-4000-8000-000000000002", "vendedor", "VENDEDOR", null, null, false, "AQAAAAIAAYagAAAAEHASd7m6a9uNKSdalJ4wrnT8fjlIHon53zlOKduqpJBvStYizESnl5HTui7x52H/Sw==", "d4e5f6a1-0002-4000-8000-000000000002", "e5f6a1b2-0002-4000-8000-000000000002", null, false, false, null, false, 0 },
                    { "b2c3d4e5-0003-4000-8000-000000000003", "cliente1", "CLIENTE1", null, null, false, "AQAAAAIAAYagAAAAEKbxkknM8h/t1wFHf/58AjmbTMo3wVHYj1kR6NJ2MXch+ui6IkEGpThmXJLoTCsSFg==", "d4e5f6a1-0003-4000-8000-000000000003", "e5f6a1b2-0003-4000-8000-000000000003", null, false, false, null, false, 0 },
                    { "b2c3d4e5-0004-4000-8000-000000000004", "cliente2", "CLIENTE2", null, null, false, "AQAAAAIAAYagAAAAEBsoGk0MKTe5lpnogNLj6vkVqZJK2/FTVGfSzcZFA8UDIui5S4tIbUnmLA3KvFq/mQ==", "d4e5f6a1-0004-4000-8000-000000000004", "e5f6a1b2-0004-4000-8000-000000000004", null, false, false, null, false, 0 },
                    { "b2c3d4e5-0005-4000-8000-000000000005", "cliente3", "CLIENTE3", null, null, false, "AQAAAAIAAYagAAAAEBTzxB0TKGvuxZsUe/jZ6VCXl9rfz68qiWowPsWybUef26F1bwV1wUWJxAqpbREf+w==", "d4e5f6a1-0005-4000-8000-000000000005", "e5f6a1b2-0005-4000-8000-000000000005", null, false, false, null, false, 0 }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "b2c3d4e5-0001-4000-8000-000000000001", "a1b2c3d4-0001-4000-8000-000000000001" },
                    { "b2c3d4e5-0002-4000-8000-000000000002", "a1b2c3d4-0002-4000-8000-000000000002" },
                    { "b2c3d4e5-0003-4000-8000-000000000003", "a1b2c3d4-0003-4000-8000-000000000003" },
                    { "b2c3d4e5-0004-4000-8000-000000000004", "a1b2c3d4-0003-4000-8000-000000000003" },
                    { "b2c3d4e5-0005-4000-8000-000000000005", "a1b2c3d4-0003-4000-8000-000000000003" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
