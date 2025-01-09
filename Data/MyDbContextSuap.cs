using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SUAP_PortalOficios.Models;
using SUAP_PortalOficios.Models.DTOs;

namespace SUAP_PortalOficios.Data
{
    public partial class MyDbContextSuap : DbContext
    {
        public MyDbContextSuap(DbContextOptions<MyDbContextSuap> options) : base(options) { }

        public virtual DbSet<Establecimiento> Establecimientos { get; set; }
        public virtual DbSet<Interno> Internos { get; set; }
        public virtual DbSet<InternoXPabellonXEstablecimiento> InternoXPabellonXEstablecimientos { get; set; }
        public virtual DbSet<InternoXTribunal> InternoXTribunals { get; set; }
        public virtual DbSet<Pabellon> Pabellons { get; set; }
        public virtual DbSet<Tribunal> Tribunals { get; set; }
        public virtual DbSet<TipoDetencion> TipoDetenciones{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Establecimiento>(entity =>
            {
                entity.Property(e => e.IdEstablecimiento).ValueGeneratedNever();
            });

            modelBuilder.Entity<Interno>(entity =>
            {
                entity.Property(e => e.CondenadoSinTto).HasDefaultValue(false);
                entity.Property(e => e.Embarazo).HasDefaultValue(false);
                entity.Property(e => e.Idconcepto).IsFixedLength();
            });

            modelBuilder.Entity<InternoXPabellonXEstablecimiento>(entity =>
            {
                entity.Property(e => e.IdHospital).HasDefaultValue(0);

                entity.HasOne(d => d.IdEstablecimientoNavigation).WithMany(p => p.InternoXPabellonXEstablecimientos)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_interno_x_pabellon_x_establecimiento_establecimiento");

                entity.HasOne(d => d.IdLegajoNavigation).WithMany(p => p.InternoXPabellonXEstablecimientos)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_interno_x_pabellon_x_establecimiento_interno");

                entity.HasOne(d => d.IdPabellonNavigation).WithMany(p => p.InternoXPabellonXEstablecimientos)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_interno_x_pabellon_x_establecimiento_Pabellon");
            });

            modelBuilder.Entity<InternoXTribunal>(entity =>
            {
                entity.Property(e => e.EliminadoLogico).HasDefaultValue(false);
                entity.Property(e => e.IdMovimiento).ValueGeneratedOnAdd();

                entity.HasOne(d => d.IdTribunalNavigation).WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Interno_x_Tribunal_tribunal");

                entity.HasOne(d => d.LegajoNavigation).WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Interno_x_Tribunal_interno");
            });

            modelBuilder.Entity<Pabellon>(entity =>
            {
                entity.HasKey(e => e.IdPabellon).HasName("PK_pabellon1");
            });

            modelBuilder.Entity<Tribunal>(entity =>
            {
                entity.Property(e => e.IdTribunal).ValueGeneratedNever();
            });

            modelBuilder.Entity<TipoDetencion>(entity =>
            {
                entity.HasKey(e => e.IdTipoDetencion).HasName("TD_detencion1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}

