namespace Melodix.Models
{
    // Suscripciones y planes
    public class PlanSuscripcion
    {
        // Necesarios
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionMeses { get; set; }
        public int MaxCuentas { get; set; }
        public bool RequiereVerificacionEstudiante { get; set; }
        public bool PermiteControlExplicito { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navegadores
        public List<Suscripcion> Suscripciones { get; set; } = new();
    }
}