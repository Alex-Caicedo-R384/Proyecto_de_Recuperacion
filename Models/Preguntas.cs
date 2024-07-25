using System.ComponentModel.DataAnnotations;

namespace PreguntasRec.Models
{
    public class Preguntas
    {
        [Key]
        public int PreguntaID { get; set; }
        public string TextoPregunta { get; set; }
        public string Correcta { get; set; }
        public string Incorrecta1 { get; set; }
        public string Incorrecta2 { get; set; }
        public string Incorrecta3 { get; set; }
    }
}
