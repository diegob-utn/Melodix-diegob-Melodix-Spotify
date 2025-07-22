namespace Melodix.Models
{
        // Enums
        public enum RolUsuario
        {
                Usuario = 0,
                Musico = 1,
                Admin = 2
        }
        public enum TipoObjetoLike
        {
                Pista = 0,
                Album = 1,
                Lista = 2
        }
        public enum AccionLike
        {
                Like = 0,
                Unlike = 1
        }
        public enum EstadoSuscripcion
        {
                Activa = 0,
                Cancelada = 1,
                Pendiente = 2,
                Vencida = 3
        }
        public enum EstadoPago
        {
                Pendiente = 0,
                Exitoso = 1,
                Fallido = 2,
                Cancelado = 3
        }
        public enum ServicioPago
        {
                Simulado = 0,
                Peigo = 1 /* Agrega otros servicios reales a futuro */
        }
        public enum TipoArchivo
        {
                Musica = 0,
                Imagen = 1,
                Otro = 2
        }
        public enum NivelLog
        {
                Info = 0,
                Warning = 1,
                Error = 2,
                Fatal = 3
        }
        public enum EstadoSolicitudMusico
        {
                Pendiente = 0,
                Aprobada = 1,
                Rechazada = 2
        }


        public enum GeneroUsuario
        {
                Hombre = 0,
                Mujer = 1
        }
}