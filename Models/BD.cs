using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TP07.Models;

public static class BD
{
    private static string _connectionString = @"Server=localhost;DataBase=DBRedSocial;Integrated Security=True;TrustServerCertificate=True;";

    public static List<Usuarios> ObtenerUsuarios()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Usuarios";
            return db.Query<Usuarios>(sql).ToList();
        }
    }

    public static void AgregarUsuario(Usuarios usuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido) VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";
            db.Execute(sql, usuario);
        }
    }
    public static string ObtenerNombreUsuario(int idUsuario)
{
    using (SqlConnection db = new SqlConnection(_connectionString))
    {
        string sql = "SELECT NombreUsuario FROM Usuarios WHERE Id = @idUsuario";
        return db.QueryFirstOrDefault<string>(sql, new { idUsuario });
    }
}

    public static Usuarios IniciarSesion(string nombreUsuario, string contrasena)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Usuarios WHERE NombreUsuario = @nombreUsuario AND Contraseña = @contrasena";
            return db.QueryFirstOrDefault<Usuarios>(sql, new { nombreUsuario, contrasena });
        }
    }

    public static List<Publicaciones> ObtenerPublicaciones()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Publicaciones ORDER BY FechaPublicacion DESC";
            return db.Query<Publicaciones>(sql).ToList();
        }
    }

    public static List<Publicaciones> ObtenerPrimerasPublicaciones()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"
                SELECT * FROM Publicaciones 
                ORDER BY FechaPublicacion DESC 
                OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

            return db.Query<Publicaciones>(sql).ToList();
        }
    }

    public static List<Publicaciones> ObtenerMasPublicaciones(int desde)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"
                SELECT * FROM Publicaciones 
                ORDER BY FechaPublicacion DESC 
                OFFSET @desde ROWS FETCH NEXT 10 ROWS ONLY";

            return db.Query<Publicaciones>(sql, new { desde }).ToList();
        }
    }

    public static void AgregarPublicacion(Publicaciones publicacion)
{
    using (SqlConnection db = new SqlConnection(_connectionString))
    {
        string sql = @"INSERT INTO Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion) 
                       VALUES (@IdUsuario, @Titulo, @Descripcion, @Imagen, @FechaPublicacion)";
        db.Execute(sql, publicacion);
    }
}

    public static void AgregarComentario(Comentarios comentario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "INSERT INTO Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario) VALUES (@IdPublicacion, @IdUsuarioComenta, @Texto, @FechaComentario)";
            db.Execute(sql, comentario);
        }
    }

    public static bool UsuarioDioMeGusta(int idPublicacion, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT COUNT(1) FROM PublicacionesMeGusta WHERE [IdPublicación] = @idPublicacion AND IdUsuario = @idUsuario";
            return db.ExecuteScalar<int>(sql, new { idPublicacion, idUsuario }) > 0;
        }
    }

    public static int ObtenerCantidadLikes(int idPublicacion)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT COUNT(1) FROM PublicacionesMeGusta WHERE [IdPublicación] = @idPublicacion";
            return db.ExecuteScalar<int>(sql, new { idPublicacion });
        }
    }

    public static bool ToggleMeGusta(int idPublicacion, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            bool yaDioLike = UsuarioDioMeGusta(idPublicacion, idUsuario);

            if (yaDioLike)
            {
                string sqlDelete = "DELETE FROM PublicacionesMeGusta WHERE [IdPublicación] = @idPublicacion AND IdUsuario = @idUsuario";
                db.Execute(sqlDelete, new { idPublicacion, idUsuario });
                return false;
            }
            else
            {
                string sqlInsert = "INSERT INTO PublicacionesMeGusta ([IdPublicación], IdUsuario) VALUES (@idPublicacion, @idUsuario)";
                db.Execute(sqlInsert, new { idPublicacion, idUsuario });
                return true;
            }
        }
    }
    public static List<dynamic> ObtenerComentariosConUsuario(int idPublicacion)
{
    using (SqlConnection db = new SqlConnection(_connectionString))
    {
        string sql = @"
            SELECT c.Id, c.Texto, c.FechaComentario, u.NombreUsuario 
            FROM Comentarios c
            INNER JOIN Usuarios u ON c.IdUsuarioComenta = u.Id
            WHERE c.IdPublicacion = @idPublicacion
            ORDER BY c.FechaComentario ASC";

        return db.Query(sql, new { idPublicacion }).ToList();
    }
}
}