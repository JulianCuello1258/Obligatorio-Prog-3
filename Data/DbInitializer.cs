using BeeKeeperApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BeeKeeperApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any Apiarios.
            if (context.Apiarios.Any())
            {
                return;   // DB has been seeded
            }

            var apiarios = new Apiario[]
            {
                new Apiario { Nombre = "Apiario Norte", Latitud = -34.8011, Longitud = -56.0645, Tipo = TipoApiario.Fijo, SeccionPolicial = "1ra", Zona = "Rural", TrashumanciaHabilitada = false, Departamento = "Canelones", Paraje = "Ruta 5" },
                new Apiario { Nombre = "Apiario Sur", Latitud = -34.9011, Longitud = -56.1645, Tipo = TipoApiario.Trasladable, SeccionPolicial = "2da", Zona = "Rural", TrashumanciaHabilitada = true, Departamento = "Montevideo", Paraje = "Melilla" },
                new Apiario { Nombre = "Apiario Este", Latitud = -34.7011, Longitud = -55.9645, Tipo = TipoApiario.Fijo, SeccionPolicial = "3ra", Zona = "Suburbana", TrashumanciaHabilitada = false, Departamento = "Canelones", Paraje = "Pando" }
            };
            context.Apiarios.AddRange(apiarios);
            context.SaveChanges();

            var apiarioNorteId = apiarios[0].Id;
            var apiarioSurId = apiarios[1].Id;

            var colmenas = new Colmena[]
            {
                new Colmena { ApiarioId = apiarioNorteId, Estado = EstadoColmena.Activa, Tipo = "Langstroth", Poblacion = "Fuerte", Temperamento = "Mansa", FechaCreacion = DateTime.Now.AddMonths(-6) },
                new Colmena { ApiarioId = apiarioNorteId, Estado = EstadoColmena.Activa, Tipo = "Langstroth", Poblacion = "Media", Temperamento = "Agresiva", FechaCreacion = DateTime.Now.AddMonths(-5) },
                new Colmena { ApiarioId = apiarioNorteId, Estado = EstadoColmena.Inactiva, Tipo = "Dadant", Poblacion = "Debil", Temperamento = "Mansa", FechaCreacion = DateTime.Now.AddMonths(-4) },
                new Colmena { ApiarioId = apiarioSurId, Estado = EstadoColmena.Activa, Tipo = "Langstroth", Poblacion = "Fuerte", Temperamento = "Mansa", FechaCreacion = DateTime.Now.AddMonths(-2) },
                new Colmena { ApiarioId = apiarioSurId, Estado = EstadoColmena.Perdida, Tipo = "Dadant", Poblacion = "Perdida", Temperamento = "Desconocido", FechaCreacion = DateTime.Now.AddMonths(-10) }
            };
            context.Colmenas.AddRange(colmenas);
            context.SaveChanges();

            var reinas = new Reina[]
            {
                new Reina { ColmenaId = colmenas[0].Id, Salud = "Buena", Presencia = true, FechaNacimiento = DateTime.Now.AddMonths(-5) },
                new Reina { ColmenaId = colmenas[1].Id, Salud = "Regular", Presencia = true, FechaNacimiento = DateTime.Now.AddMonths(-4) },
                new Reina { ColmenaId = colmenas[2].Id, Salud = "Mala", Presencia = false },
                new Reina { ColmenaId = colmenas[3].Id, Salud = "Excelente", Presencia = true, FechaNacimiento = DateTime.Now.AddMonths(-1) }
                // Colmena 4 is Perdida, maybe no queen
            };
            context.Reinas.AddRange(reinas);
            context.SaveChanges();

            var revisiones = new Revision[]
            {
                new Revision { ColmenaId = colmenas[0].Id, Fecha = DateTime.Now.AddDays(-15), Tipo = "Rutinaria", Observaciones = "Todo en orden", ReinaPresente = true, HayCrias = true, PoblacionEstimada = "Fuerte", Temperamento = "Mansa" },
                new Revision { ColmenaId = colmenas[1].Id, Fecha = DateTime.Now.AddDays(-10), Tipo = "Sanitaria", Observaciones = "Presencia de varroa", Sintomas = "Abejas deformes", Enfermedades = "Varroasis", Tratamiento = "Ácido oxálico", Dosis = "5ml", ProximaDosis = DateTime.Now.AddDays(5), ReinaPresente = true, HayCrias = true },
                new Revision { ColmenaId = colmenas[3].Id, Fecha = DateTime.Now.AddDays(-2), Tipo = "Extraccion", Observaciones = "Lista para extraer miel", ReinaPresente = true, HayCrias = true }
            };
            context.Revisiones.AddRange(revisiones);
            context.SaveChanges();

            var extracciones = new Extraccion[]
            {
                new Extraccion { ColmenaId = colmenas[0].Id, CantidadKg = 15.5, Fecha = DateTime.Now.AddDays(-30) },
                new Extraccion { ColmenaId = colmenas[0].Id, CantidadKg = 12.0, Fecha = DateTime.Now.AddDays(-5) },
                new Extraccion { ColmenaId = colmenas[3].Id, CantidadKg = 20.0, Fecha = DateTime.Now.AddDays(-1) }
            };
            context.Extracciones.AddRange(extracciones);
            context.SaveChanges();

            var tareas = new Tarea[]
            {
                new Tarea { ApiarioId = apiarioNorteId, Titulo = "Limpieza de Apiario", Descripcion = "Cortar pasto y arreglar el alambrado del apiario.", FechaProgramada = DateTime.Now.AddDays(2), Completada = false },
                new Tarea { ColmenaId = colmenas[1].Id, Titulo = "Aplicar tratamiento", Descripcion = "Aplicar segunda dosis de tratamiento contra varroa.", FechaProgramada = DateTime.Now.AddDays(5), Completada = false },
                new Tarea { ApiarioId = apiarioSurId, Titulo = "Revisión general", Descripcion = "Revisar todas las colmenas antes de la trashumancia.", FechaProgramada = DateTime.Now.AddDays(-1), Completada = true }
            };
            context.Tareas.AddRange(tareas);
            context.SaveChanges();

            var trashumancias = new Trashumancia[]
            {
                new Trashumancia { ApiarioOrigenId = apiarioSurId, ApiarioDestinoId = apiarios[2].Id, Fecha = DateTime.Now.AddDays(-60), DistanciaKm = 45.5 }
            };
            context.Trashumancias.AddRange(trashumancias);
            context.SaveChanges();
        }
    }
}
