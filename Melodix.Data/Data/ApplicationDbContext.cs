using Melodix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melodix.Models.Models;

namespace Melodix.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets principales (nombres en plural)
        public DbSet<Album> Albums { get; set; }
        public DbSet<Pista> Pistas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<ListaReproduccion> ListasReproduccion { get; set; }
        public DbSet<ListaPista> ListasPista { get; set; }
        public DbSet<HistorialEscucha> HistorialesEscucha { get; set; }
        public DbSet<HistorialLike> HistorialesLike { get; set; }
        public DbSet<UsuarioLikeAlbum> UsuariosLikeAlbum { get; set; }
        public DbSet<UsuarioLikeLista> UsuariosLikeLista { get; set; }
        public DbSet<UsuarioLikePista> UsuariosLikePista { get; set; }
        public DbSet<UsuarioSigue> UsuariosSigue { get; set; }
        public DbSet<UsuarioSigueLista> UsuariosSigueLista { get; set; }
        public DbSet<PlanSuscripcion> PlanesSuscripcion { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<SuscripcionUsuario> SuscripcionesUsuario { get; set; }
        public DbSet<TransaccionPago> TransaccionesPago { get; set; }
        public DbSet<ArchivoSubido> ArchivosSubidos { get; set; }
        public DbSet<LogSistema> LogsSistema { get; set; }
        public DbSet<SolicitudMusico> SolicitudesMusico { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Elimina la configuración 1:1 de usuario y perfil extendido
            // Ya NO necesitas esto:
            // builder.Entity<ApplicationUser>()
            //     .HasOne(u => u.Perfil)
            //     .WithOne(p => p.Usuario)
            //     .HasForeignKey<PerfilUsuario>(p => p.UsuarioId)
            //     .IsRequired();

            // Índices en campos de búsqueda frecuente
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.SpotifyId);
            builder.Entity<Album>()
                .HasIndex(a => a.SpotifyAlbumId);
            builder.Entity<Pista>()
                .HasIndex(p => p.SpotifyPistaId);
            builder.Entity<ListaReproduccion>()
                .HasIndex(l => l.SpotifyListaId);

            // Unicidad en relaciones muchos-a-muchos de follows (UsuarioSigue)
            builder.Entity<UsuarioSigue>()
                .HasKey(us => new { us.SeguidorId, us.SeguidoId });

            builder.Entity<UsuarioSigue>()
                .HasOne(us => us.Seguidor)
                .WithMany(u => u.Seguidos)
                .HasForeignKey(us => us.SeguidorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UsuarioSigue>()
                .HasOne(us => us.Seguido)
                .WithMany(u => u.Seguidores)
                .HasForeignKey(us => us.SeguidoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UsuarioSigue>()
                .HasIndex(us => new { us.SeguidorId, us.SeguidoId })
                .IsUnique();

            // Unicidad en UsuarioLikeAlbum, UsuarioLikeLista, UsuarioLikePista
            builder.Entity<UsuarioLikeAlbum>()
                .HasIndex(x => new { x.UsuarioId, x.AlbumId })
                .IsUnique();
            builder.Entity<UsuarioLikeLista>()
                .HasIndex(x => new { x.UsuarioId, x.ListaId })
                .IsUnique();
            builder.Entity<UsuarioLikePista>()
                .HasIndex(x => new { x.UsuarioId, x.PistaId })
                .IsUnique();

            // ...existing code...
            builder.Entity<UsuarioSigueLista>()
                .HasIndex(x => new { x.UsuarioId, x.ListaId })
                .IsUnique();

            // Relaciones para UsuarioLikeAlbum
            builder.Entity<UsuarioLikeAlbum>()
                .HasOne(ula => ula.Usuario)
                .WithMany(u => u.UsuarioLikeAlbums)
                .HasForeignKey(ula => ula.UsuarioId);

            // Relaciones para UsuarioLikeLista
            builder.Entity<UsuarioLikeLista>()
                .HasOne(ull => ull.Usuario)
                .WithMany(u => u.UsuarioLikeListas)
                .HasForeignKey(ull => ull.UsuarioId);

            // Relaciones para UsuarioLikePista
            builder.Entity<UsuarioLikePista>()
                .HasOne(ulp => ulp.Usuario)
                .WithMany(u => u.UsuarioLikePistas)
                .HasForeignKey(ulp => ulp.UsuarioId);

            // ...existing code...

            // Relaciones para UsuarioSigueLista
            builder.Entity<UsuarioSigueLista>()
                .HasOne(usl => usl.Usuario)
                .WithMany(u => u.UsuarioSigueListas)
                .HasForeignKey(usl => usl.UsuarioId);

            // Relaciones para HistorialEscucha
            builder.Entity<HistorialEscucha>()
                .HasOne(he => he.Usuario)
                .WithMany(u => u.HistorialEscuchas)
                .HasForeignKey(he => he.UsuarioId);

            // Relaciones para HistorialLike
            builder.Entity<HistorialLike>()
                .HasOne(hl => hl.Usuario)
                .WithMany(u => u.HistorialLikes)
                .HasForeignKey(hl => hl.UsuarioId);

            // Relaciones para SuscripcionUsuario
            builder.Entity<SuscripcionUsuario>()
                .HasOne(su => su.Usuario)
                .WithMany(u => u.SuscripcionUsuarios)
                .HasForeignKey(su => su.UsuarioId);

            builder.Entity<Suscripcion>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.Suscripciones)
                .HasForeignKey(s => s.UsuarioId);

            // Relaciones para TransaccionPago
            builder.Entity<TransaccionPago>()
                .HasOne(tp => tp.Usuario)
                .WithMany(u => u.TransaccionesPago)
                .HasForeignKey(tp => tp.UsuarioId);

            builder.Entity<TransaccionPago>()
                .HasOne(tp => tp.Suscripcion)
                .WithMany(s => s.TransaccionesPago)
                .HasForeignKey(tp => tp.SuscripcionId);

            builder.Entity<TransaccionPago>()
                .Property(tp => tp.Monto)
                .HasColumnType("decimal(18,2)");

            // Relaciones para ArchivoSubido
            builder.Entity<ArchivoSubido>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.ArchivosSubidos)
                .HasForeignKey(a => a.UsuarioId);

            // Relaciones para LogSistema
            builder.Entity<LogSistema>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.LogsGenerados)
                .HasForeignKey(l => l.UsuarioId)
                .IsRequired(false);

            // Relaciones para SolicitudMusico
            builder.Entity<SolicitudMusico>()
                .HasOne(s => s.Usuario)
                .WithMany(u => u.SolicitudesMusico)
                .HasForeignKey(s => s.UsuarioId);

            builder.Entity<SolicitudMusico>()
                .HasOne(s => s.AdminRevisor)
                .WithMany(u => u.SolicitudesRevisadas)
                .HasForeignKey(s => s.AdminRevisorId)
                .IsRequired(false);

            // Ejemplo: Unicidad en SpotifyId de usuario (opcional pero recomendado para integridad)
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.SpotifyId)
                .IsUnique(false); // Cambia a true si quieres forzar unicidad global
        }

        // Auditoría automática de ApplicationUser (puedes expandirla a otras entidades si lo deseas)
        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries()
                        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                foreach (var property in entry.Properties.Where(p => p.Metadata.ClrType == typeof(DateTime)))
                {
                    var dt = (DateTime)property.CurrentValue;
                    if (dt.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                    else
                    {
                        property.CurrentValue = dt.ToUniversalTime();
                    }
                }
                foreach (var property in entry.Properties.Where(p => p.Metadata.ClrType == typeof(DateTime?)))
                {
                    var dt = (DateTime?)property.CurrentValue;
                    if (dt.HasValue)
                    {
                        if (dt.Value.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
                        }
                        else
                        {
                            property.CurrentValue = dt.Value.ToUniversalTime();
                        }
                    }
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ApplicationUser &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                if (entry.State == EntityState.Added)
                {
                    ((ApplicationUser)entry.Entity).CreadoEn = now;
                }
                ((ApplicationUser)entry.Entity).ActualizadoEn = now;
            }
        }
    }
}