using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SUAP_PortalOficios.Models;
using SUAP_PortalOficios.Models.DTOs;


namespace SUAP_PortalOficios.Data
{
    public class MyDbContext:IdentityDbContext<MyUser>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
        public DbSet<UserPermissions> UserPermissions { get; set; }
        public DbSet<Sections> Sections { get; set; }
        public DbSet<Scopes> Scopes { get; set; }
        public DbSet<Oficios> Oficios { get; set; }
        public DbSet<TipoOficio> TipoOficios { get; set; }
        public DbSet<Oficios_x_Area> Oficios_x_Area { get; set; }
        public DbSet<Interno_x_Oficio> Interno_x_Oficio { get; set; }
        public DbSet<Estado> Estado { get; set; }
        public DbSet<DocumentPdf> DocumentPdf { get; set; }      
        public DbSet<Observation> Observation { get; set; }
        public DbSet<Plazo> Plazo { get; set; } 
        public DbSet<MedioIng> MedioIng { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define la clave primaria compuesta
            modelBuilder.Entity<UserPermissions>(entity =>
            {
                entity.HasKey(rp => new { rp.UserId, rp.SectionId, rp.ScopeId });

                // Relación con Sections (muchos a uno)
                entity.HasOne(e => e.Section)
                      .WithMany() // No se necesita propiedad inversa en Sections
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de Sections

                // Relación con Scopes (muchos a uno)
                entity.HasOne(e => e.Scope)
                      .WithMany() // No se necesita propiedad inversa en Scopes
                      .HasForeignKey(e => e.ScopeId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de Scopes

                // Relación con MyUser (muchos a uno)
                entity.HasOne(e => e.User)
                      .WithMany() // No se necesita propiedad inversa en MyUser
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de MyUser
            });
                


            modelBuilder.Entity<Oficios_x_Area>(entity =>
            {
                // Definir la clave compuesta
                entity.HasKey(e => new { e.OficiosId, e.SectionId, e.ScopeId });

                // Relación con Oficios (muchos a uno)
                entity.HasOne(e => e.oficios)
                      .WithMany(of => of.oficios_X_Areas) // Un Oficio puede estar en varias áreas
                      .HasForeignKey(e => e.OficiosId)
                      .OnDelete(DeleteBehavior.Restrict); // Si se elimina un Oficio, se eliminan las áreas asociadas

                // Relación con Sections (muchos a uno)
                entity.HasOne(e => e.Sections)
                      .WithMany() // No se necesita propiedad inversa en Sections
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de Sections

                // Relación con Scopes (muchos a uno)
                entity.HasOne(e => e.Scopes)
                      .WithMany() // No se necesita propiedad inversa en Scopes
                      .HasForeignKey(e => e.ScopeId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de Scopes

                // Relación con MyUser (muchos a uno)
                entity.HasOne(e => e.MyUser)
                      .WithMany() // No se necesita propiedad inversa en MyUser
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Evitar la eliminación en cascada de MyUser

                // Propiedades adicionales
                entity.Property(e => e.FechaDerivado)
                      .HasDefaultValueSql("GETDATE()"); // Asigna la fecha actual por defecto

                entity.Property(e => e.EstadoId)
                      .HasDefaultValue(1); // Valor por defecto para EstadoId

                entity.Property(e => e.Visto)
                      .HasDefaultValue(false); // Valor por defecto para Visto

                entity.Property(e => e.conocimiento)
                      .HasDefaultValue(false); // Valor por defecto para conocimiento
            }); 


            modelBuilder.Entity<Interno_x_Oficio>(entity =>
            {
                entity.HasKey(rp => new { rp.OficiosId, rp.Legajo });
            });


            modelBuilder.Entity<Oficios>(entity =>
            {
                entity.HasKey(e => e.IdOficio); // Clave primaria

                // Relaciones (foráneas)
                entity.HasOne(e => e.TipoOficio)
                      .WithMany()
                      .HasForeignKey(e => e.IdTipoOficio)
                      .OnDelete(DeleteBehavior.Restrict); // Si deseas controlar el comportamiento en caso de eliminación

                entity.HasOne(e => e.Estado)
                      .WithMany()
                      .HasForeignKey(e => e.IdEstado)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e=> e.Plazo)
                       .WithMany()
                       .HasForeignKey(e => e.IdPlazo)
                       .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MedioIng)
                      .WithMany()
                      .HasForeignKey(e => e.IdMedio)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MyUser)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Scope)
                      .WithMany()
                      .HasForeignKey(e => e.IdEstabACargo)
                      .OnDelete(DeleteBehavior.NoAction);              

                // Otras configuraciones
                entity.Property(e => e.FechaIngreso)
                      .HasDefaultValueSql("GETDATE()"); // Para que se asigne automáticamente en la base de datos
            });

            modelBuilder.Entity<Observation>(entity =>
            {
                entity.HasKey(e => e.IdObservacion); // Clave primaria

                // Asegurarse de que la clave foránea exista y esté correctamente configurada
                entity.HasOne(o => o.oficios) // Cada Observación está relacionada con un Oficio
                      .WithMany(of => of.Observations) // Un Oficio tiene muchas Observaciones
                      .HasForeignKey(o => o.IdOficio); // La clave foránea es IdOficio
            });

            modelBuilder.Entity<DocumentPdf>(entity =>
            {
                entity.HasKey(e => e.DocId);

                entity.Property(e => e.src)
                    .HasMaxLength(500) 
                    .IsRequired(false);

                entity.Property(e => e.FileName)
                    .HasMaxLength(255);

                entity.Property(e => e.fechaCarga)
                    .HasDefaultValueSql("GETDATE()"); 

                entity.Property(e => e.EliminadoLogico)
                    .HasDefaultValue(false);

                entity.HasOne(d => d.Oficio)
                    .WithMany(o => o.DocumentPdfs) 
                    .HasForeignKey(d => d.OficioId)
                    .OnDelete(DeleteBehavior.Restrict); 

                // Configuración adicional si necesitas índices, por ejemplo:
                entity.HasIndex(e => e.OficioId).HasDatabaseName("IX_DocumentPdf_OficioId");
            });

        }
       
    }
}
