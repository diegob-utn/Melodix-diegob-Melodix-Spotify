using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melodix.Models.Models
{
    public class ArchivoSubido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public TipoArchivo Tipo { get; set; }

        [Required, MaxLength(255)]
        public string NombreArchivo { get; set; }

        [Required, MaxLength(1024)]
        public string RutaAlmacenamiento { get; set; }

        [Required]
        public long TamanoBytes { get; set; }

        [Required]
        public DateTime FechaSubida { get; set; }

        [Required]
        public bool EsPublico { get; set; }

        [MaxLength(12)]
        public string? Extension { get; set; }

        [MaxLength(128)]
        public string? HashArchivo { get; set; } // UNIQUE si querés evitar duplicados

        [MaxLength(512)]
        public string? Descripcion { get; set; }

        [MaxLength(64)]
        public string? EntidadReferencia { get; set; }

        public int? IdReferencia { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual ApplicationUser Usuario { get; set; }
    }
}