using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TechStore1.Models;
using TechStore1.Models.TechStore1.Models;

namespace TechStore1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Relacion configuracion
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            //Categorias prueba
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Laptops", Descripcion = "Computadoras portátiles", Icono = "bi bi-laptop" },
                new Categoria { Id = 2, Nombre = "Smartphones", Descripcion = "Teléfonos inteligentes", Icono = "bi bi-phone" },
                new Categoria { Id = 3, Nombre = "Accesorios", Descripcion = "Periféricos y más", Icono = "bi bi-headset" },
                new Categoria { Id = 4, Nombre = "Monitores", Descripcion = "Pantallas de alta resolución", Icono = "bi bi-display" }
            );

            // Productos prueba
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop Pro X", Descripcion = "Core i7, 16GB RAM, 512GB SSD", Precio = 1200.00m, CategoriaId = 1, Imagen = "novabook-pro-14.jpg", Stock = 10, Estado = true },
                new Producto { Id = 2, Nombre = "Smartphone G", Descripcion = "Pantalla OLED, 128GB, 5G", Precio = 800.00m, CategoriaId = 2, Imagen = "pixelwave-x1.jpg", Stock = 20, Estado = true },
                new Producto { Id = 3, Nombre = "Auriculares Inalámbricos", Descripcion = "Cancelación de ruido act.", Precio = 150.00m, CategoriaId = 3, Imagen = "sonicpulse-anc.jpg", Stock = 50, Estado = true },
                new Producto { Id = 4, Nombre = "Monitor 4K 27", Descripcion = "Monitor IPS para diseño", Precio = 350.00m, CategoriaId = 4, Imagen = "visiondock-27.jpg", Stock = 15, Estado = true },
                new Producto { Id = 5, Nombre = "Mouse gamer RGB", Descripcion = "LIGHTSYNC RGB personalizable", Precio = 80.00m, CategoriaId = 3, Imagen = "gamecore-m5.jpg", Stock = 25, Estado = true },
                new Producto { Id = 6, Nombre = "Teclado Mecánico", Descripcion = "Switches RGB y blue", Precio = 90.00m, CategoriaId = 3, Imagen = "hyperstrike-k7.jpg", Stock = 30, Estado = true }
            );
        }
    }
}