using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PreguntasRec.Datos;
using PreguntasRec.Models;
using System;
using System.Diagnostics;

namespace PreguntasRec.Controllers
{
    public class InicioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InicioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Inicio()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuario
                .OrderByDescending(u => u.Puntaje)
                .ToListAsync();

            return View(usuarios);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Usuario model, string action)
        {
            if (!ModelState.IsValid)
            {
                return View("Inicio", model);
            }

            if (action == "correo")
            {
                var existingUser = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Correo == model.Correo);

                if (existingUser != null)
                {
                    HttpContext.Session.SetString("UsuarioCorreo", existingUser.Correo);
                    HttpContext.Session.SetInt32("UsuarioId", existingUser.UsuarioID);
                    HttpContext.Session.SetString("UsuarioTag", existingUser.Tag);
                    return RedirectToAction("Index", new { tag = existingUser.Tag, correo = existingUser.Correo, usuarioId = existingUser.UsuarioID });
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Tag))
                    {
                        ViewBag.ShowTagField = true;
                        return View("Inicio", model);
                    }
                    else
                    {
                        _context.Usuario.Add(model);
                        await _context.SaveChangesAsync();

                        HttpContext.Session.SetString("UsuarioCorreo", model.Correo);
                        HttpContext.Session.SetInt32("UsuarioId", model.UsuarioID);
                        HttpContext.Session.SetString("UsuarioTag", model.Tag);
                        return RedirectToAction("Index", new { tag = model.Tag, correo = model.Correo, usuarioId = model.UsuarioID });
                    }
                }
            }
            else if (action == "invitado")
            {
                model.Correo = model.Correo ?? string.Empty;
                model.Tag = "invitado" + new Random().Next(1, 1000).ToString();

                _context.Usuario.Add(model);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UsuarioCorreo", model.Correo);
                HttpContext.Session.SetInt32("UsuarioId", model.UsuarioID);
                HttpContext.Session.SetString("UsuarioTag", model.Tag);
                return RedirectToAction("Index");
            }

            ViewBag.Correo = model.Correo;
            ViewBag.Tag = model.Tag;
            ViewBag.Puntaje = model.Puntaje;

            return View("Inicio", model);
        }


        [HttpGet]
        public async Task<IActionResult> Preguntas(int? usuarioId, string correo, string tag)
        {
            Usuario usuario = null;

            if (string.IsNullOrEmpty(correo))
            {
                correo = HttpContext.Session.GetString("UsuarioCorreo");
            }

            if (!usuarioId.HasValue)
            {
                usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = HttpContext.Session.GetString("UsuarioTag");
            }

            if (!string.IsNullOrEmpty(correo))
            {
                usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
            }

            if (usuario == null && usuarioId.HasValue)
            {
                usuario = await _context.Usuario.FindAsync(usuarioId.Value);
            }

            if (usuario == null)
            {
                return BadRequest("No se encontró el usuario.");
            }

            var preguntas = await _context.Preguntas.OrderBy(r => Guid.NewGuid()).Take(10).ToListAsync();

            ViewBag.Usuario = usuario;
            ViewBag.Preguntas = preguntas;
            ViewBag.PreguntaActual = preguntas.FirstOrDefault();
            ViewBag.PreguntaIndex = 0;
            ViewBag.Puntaje = usuario.Puntaje;
            ViewBag.Correo = usuario.Correo;
            ViewBag.Tag = usuario.Tag;

            return View(preguntas);
        }


        [HttpPost]
        public async Task<IActionResult> Preguntas(int usuarioId, int preguntaId, string respuestaSeleccionada, int puntaje, int preguntaIndex)
        {
            var usuario = await _context.Usuario.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            var pregunta = await _context.Preguntas.FindAsync(preguntaId);
            if (pregunta == null)
            {
                return NotFound();
            }

            if (respuestaSeleccionada == pregunta.Correcta)
            {
                puntaje++;
            }

            preguntaIndex++;

            var preguntasRespondidas = ViewBag.PreguntasRespondidas as HashSet<int> ?? new HashSet<int>();
            preguntasRespondidas.Add(preguntaId);

            var preguntasDisponibles = await _context.Preguntas
                .Where(p => !preguntasRespondidas.Contains(p.PreguntaID))
                .OrderBy(r => Guid.NewGuid())
                .Take(10)
                .ToListAsync();

            if (preguntaIndex < preguntasDisponibles.Count)
            {
                ViewBag.PreguntaActual = preguntasDisponibles[preguntaIndex];
                ViewBag.PreguntaIndex = preguntaIndex;
                ViewBag.Puntaje = puntaje;
                ViewBag.Usuario = usuario;
                ViewBag.Preguntas = preguntasDisponibles;
                ViewBag.Correo = usuario.Correo;
                ViewBag.Tag = usuario.Tag;
                ViewBag.PreguntasRespondidas = preguntasRespondidas;

                return View();
            }
            else
            {
                usuario.Puntaje = puntaje;
                _context.Update(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new
                {
                    usuarioId = usuario.UsuarioID,
                    correo = usuario.Correo,
                    tag = usuario.Tag
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
