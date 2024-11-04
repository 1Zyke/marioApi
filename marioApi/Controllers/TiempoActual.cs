using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using Grupo3.Models;

namespace Grupo3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiempoActual : ControllerBase
    {
        [HttpGet("listar-zonas")]
        public IActionResult ListarZonasHorarias()
        {
            var zonas = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new { Id = tz.Id, StandardName = tz.StandardName })
                .ToList();

            return Ok(zonas);
        }

        [HttpGet("hora-actual")]
        public IActionResult GetCurrentTimeZone(string zonaHorarioId = null)
        {
            var timeZone = TimeZoneInfo.Local;

            if (!string.IsNullOrEmpty(zonaHorarioId))
            {
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(zonaHorarioId);
                }
                catch (TimeZoneNotFoundException)
                {
                    return NotFound(new { error = "Zona horaria no válida." });
                }
                catch (InvalidTimeZoneException)
                {
                    return BadRequest(new { error = "Zona horaria no válida." });
                }
            }

            // Convertir la hora actual a la zona horaria seleccionada
            var currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);

            return Ok(new
            {
                CurrentTimeZone = timeZone.StandardName,
                CurrentTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        [HttpPost("hora-sincronizada")]
        public IActionResult SyncTime([FromBody] tiempoActual request)
        {
            if (string.IsNullOrEmpty(request.zonaHorarioId))
            {
                return BadRequest(new { error = "El campo targetTimeZoneId es obligatorio." });
            }

            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(request.zonaHorarioId);
                var currentTimeInTargetZone = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

                return Ok(new
                {
                    message = $"Hora sincronizada con la zona horaria: {request.zonaHorarioId}",
                    currentTimeInTargetZone = currentTimeInTargetZone.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (TimeZoneNotFoundException)
            {
                return BadRequest(new { error = "Zona horaria no válida." });
            }
        }

        public static class SystemTime
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetSystemTime(ref SystemTimeStruct st);

            public static void SetSystemTime(DateTime dateTime)
            {
                SystemTimeStruct st = new SystemTimeStruct
                {
                    Year = (short)dateTime.Year,
                    Month = (short)dateTime.Month,
                    Day = (short)dateTime.Day,
                    Hour = (short)dateTime.Hour,
                    Minute = (short)dateTime.Minute,
                    Second = (short)dateTime.Second,
                    Milliseconds = (short)dateTime.Millisecond
                };

                SetSystemTime(ref st);
            }

            private struct SystemTimeStruct
            {
                public short Year;
                public short Month;
                public short DayOfWeek;
                public short Day;
                public short Hour;
                public short Minute;
                public short Second;
                public short Milliseconds;
            }
        }
    }
}
