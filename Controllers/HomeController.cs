using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TP07.Models;

namespace TP07.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioLogueado")))
        {
            return RedirectToAction("Registrarse", "Home");
        }

        List<Publicaciones> listaPublicaciones = BD.ObtenerPrimerasPublicaciones();
        return View(listaPublicaciones);
    }

    public IActionResult Registrarse()
    {
        return View();
    }

    [HttpPost]
    public IActionResult GuardarRegistro(Usuarios user)
    {
        BD.AgregarUsuario(user);
        Usuarios usuarioRegistrado = BD.IniciarSesion(user.NombreUsuario, user.Contraseña);

        if (usuarioRegistrado != null)
        {
            HttpContext.Session.SetInt32("IdUsuario", usuarioRegistrado.Id);
            HttpContext.Session.SetString("UsuarioLogueado", usuarioRegistrado.NombreUsuario);
            HttpContext.Session.SetString("NombreCompleto", $"{usuarioRegistrado.Nombre} {usuarioRegistrado.Apellido}");
        }

        return RedirectToAction("Index");
    }

    public IActionResult Loguearse()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ProcesarLogin(string nombreUsuario, string contrasena)
    {
        Usuarios user = BD.IniciarSesion(nombreUsuario, contrasena);

        if (user != null)
        {
            HttpContext.Session.SetInt32("IdUsuario", user.Id);
            HttpContext.Session.SetString("UsuarioLogueado", user.NombreUsuario);
            HttpContext.Session.SetString("NombreCompleto", $"{user.Nombre} {user.Apellido}");

            return RedirectToAction("Index");
        }
        else
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View("Loguearse");
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Loguearse");
    }

    [HttpPost]
    public IActionResult ToggleMeGusta(int idPublicacion)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

        if (idUsuario == null)
        {
            return Json(new { success = false, message = "Usuario no autenticado" });
        }

        bool dioLike = BD.ToggleMeGusta(idPublicacion, idUsuario.Value);
        int totalLikes = BD.ObtenerCantidadLikes(idPublicacion);

        return Json(new { success = true, dioLike = dioLike, cantidadLikes = totalLikes });
    }

    [HttpPost]
    public IActionResult AgregarComentario(int idPublicacion, string texto)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
        string nombreUsuario = HttpContext.Session.GetString("UsuarioLogueado");

        if (idUsuario == null)
        {
            return Json(new { success = false, message = "Debes estar logueado para comentar." });
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            return Json(new { success = false, message = "El comentario no puede estar vacío." });
        }

        Comentarios nuevoComentario = new Comentarios
        {
            IdPublicacion = idPublicacion,
            IdUsuarioComenta = idUsuario.Value,
            Texto = texto.Trim(),
            FechaComentario = DateTime.Now
        };

        BD.AgregarComentario(nuevoComentario);

        return Json(new
        {
            success = true,
            nombreUsuario = nombreUsuario,
            texto = nuevoComentario.Texto,
            fecha = nuevoComentario.FechaComentario.ToString("dd/MM/yyyy HH:mm")
        });
    }

    public IActionResult CrearPublicacion()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioLogueado")))
        {
            return RedirectToAction("Registrarse");
        }

        return View();
    }

    [HttpPost]
    public IActionResult GuardarPublicacion(string titulo, string descripcion, IFormFile archivoImagen)
    {
        int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

        if (idUsuario == null)
        {
            return RedirectToAction("Registrarse");
        }

        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
        {
            ViewBag.Error = "El título y la descripción son obligatorios.";
            return View("CrearPublicacion");
        }

        string rutaImagenBD = null;

        if (archivoImagen != null && archivoImagen.Length > 0)
        {
            string carpetaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(carpetaUploads))
            {
                Directory.CreateDirectory(carpetaUploads);
            }

            string nombreArchivoUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
            string rutaAbsoluta = Path.Combine(carpetaUploads, nombreArchivoUnico);

            using (FileStream stream = new FileStream(rutaAbsoluta, FileMode.Create))
            {
                archivoImagen.CopyTo(stream);
            }

            rutaImagenBD = "/uploads/" + nombreArchivoUnico;
        }

        Publicaciones nuevaPub = new Publicaciones
        {
            IdUsuario = idUsuario.Value,
            Titulo = titulo.Trim(),
            Descripcion = descripcion.Trim(),
            Imagen = rutaImagenBD,
            FechaPublicacion = DateTime.Now
        };

        BD.AgregarPublicacion(nuevaPub);

        return RedirectToAction("Index");
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

    [HttpGet]
    public IActionResult CargarMasPublicaciones(int desde)
    {
        List<Publicaciones> publicaciones = BD.ObtenerMasPublicaciones(desde);

        IEnumerable<object> listaResultado = publicaciones.Select(pub => new
        {
            id = pub.Id,
            titulo = pub.Titulo,
            descripcion = pub.Descripcion,
            imagen = pub.Imagen,
            fecha = pub.FechaPublicacion.ToString("dd MMM, HH:mm"),
            nombreAutor = BD.ObtenerNombreUsuario(pub.IdUsuario),
            cantidadLikes = BD.ObtenerCantidadLikes(pub.Id),
            yaDioLike = HttpContext.Session.GetInt32("IdUsuario").HasValue 
                && BD.UsuarioDioMeGusta(pub.Id, HttpContext.Session.GetInt32("IdUsuario").Value),
            comentarios = BD.ObtenerComentariosConUsuario(pub.Id).Select(c => new
            {
                nombreUsuario = c.NombreUsuario,
                texto = c.Texto,
                fechaComentario = Convert.ToDateTime(c.FechaComentario).ToString("dd/MM HH:mm")
            })
        }).ToList();

        return Json(listaResultado);
    }
}