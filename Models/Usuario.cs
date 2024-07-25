using System.ComponentModel.DataAnnotations;

namespace PreguntasRec.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }
        public string Correo { get; set; }
        public string Tag { get; set; }
        public int Puntaje { get; set; }

    }
}
