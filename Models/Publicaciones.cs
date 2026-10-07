using System;

namespace TP07.Models;

public class Publicaciones
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Imagen { get; set; }
    public DateTime FechaPublicacion { get; set; }
}