using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PreguntasRec.Models;

namespace PreguntasRec.Datos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Preguntas> Preguntas { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
    }
}
