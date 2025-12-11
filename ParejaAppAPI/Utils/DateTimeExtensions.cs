namespace ParejaAppAPI.Utils;

/// <summary>
/// Extensiones para manejo de conversiones de zona horaria en DateTime
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convierte una fecha UTC a la zona horaria especificada
    /// </summary>
    /// <param name="utcDateTime">Fecha en UTC</param>
    /// <param name="timeZoneId">ID de la zona horaria (ej: "America/Mexico_City")</param>
    /// <returns>Fecha convertida a la zona horaria especificada</returns>
    public static DateTime ToTimeZone(this DateTime utcDateTime, string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return utcDateTime;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch
        {
            // Si hay error en la conversión, devolver la fecha original
            return utcDateTime;
        }
    }

    /// <summary>
    /// Convierte una fecha de una zona horaria específica a UTC
    /// </summary>
    /// <param name="localDateTime">Fecha en zona horaria local</param>
    /// <param name="timeZoneId">ID de la zona horaria (ej: "America/Mexico_City")</param>
    /// <returns>Fecha convertida a UTC</returns>
    public static DateTime ToUtcFromTimeZone(this DateTime localDateTime, string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return localDateTime;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }
        catch
        {
            // Si hay error en la conversión, devolver la fecha original
            return localDateTime;
        }
    }

    /// <summary>
    /// Valida si un ID de zona horaria es válido
    /// </summary>
    /// <param name="timeZoneId">ID de la zona horaria a validar</param>
    /// <returns>True si el ID es válido, False si no</returns>
    public static bool IsValidTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return false;

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
