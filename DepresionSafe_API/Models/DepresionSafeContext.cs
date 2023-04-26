using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DepresionSafe_API.Models;

public partial class DepresionSafeContext : DbContext
{
    public DepresionSafeContext()
    {
    }

    public DepresionSafeContext(DbContextOptions<DepresionSafeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RolUsuario> RolUsuarios { get; set; }

    public virtual DbSet<Subscripcion> Subscripcions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RolUsuar__3213E83F8220A21C");

            entity.ToTable("RolUsuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Subscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3213E83FFF66C4B8");

            entity.ToTable("Subscripcion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83F6F418F2C");

            entity.ToTable("Usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cedula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.IdRolUsuario).HasColumnName("idRolUsuario");
            entity.Property(e => e.IdSubscripcion).HasColumnName("idSubscripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Telefono).HasColumnName("telefono");

            entity.HasOne(d => d.IdRolUsuarioNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRolUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__idRolUs__286302EC");

            entity.HasOne(d => d.IdSubscripcionNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdSubscripcion)
                .HasConstraintName("FK__Usuario__idSubsc__29572725");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
