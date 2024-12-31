
namespace BuscarRegistroSanitarioService.Loggin
{

public class LogManager : ILoggerManager
{
    private readonly ILogger<LogManager> _logger;

    public LogManager(ILogger<LogManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registra un mensaje de información.
    /// </summary>
    public void LogInfo(string mensaje)
    {
        _logger.LogInformation(mensaje);
        EscribirArchivoLog($"INFO: {mensaje}");
    }

    /// <summary>
    /// Registra un mensaje de advertencia.
    /// </summary>
    public void LogWarning(string mensaje)
    {
        _logger.LogWarning(mensaje);
        EscribirArchivoLog($"WARNING: {mensaje}");
    }

    /// <summary>
    /// Registra un mensaje de error.
    /// </summary>
    public void LogError(string mensaje, Exception? ex = null)
    {
        _logger.LogError(ex, mensaje);
        var errorCompleto = ex != null ? $"{mensaje} - Exception: {ex.Message}" : mensaje;
        EscribirArchivoLog($"ERROR: {errorCompleto}");
    }

    /// <summary>
    /// Escribe un mensaje de log en un archivo de texto.
    /// </summary>
    private void EscribirArchivoLog(string mensaje)
    {
        try
        {
            var rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(rutaArchivo))
            {
                Directory.CreateDirectory(rutaArchivo);
            }

            var archivoLog = Path.Combine(rutaArchivo, $"log_{DateTime.Now:yyyyMMdd}.txt");
            var mensajeConFecha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}";

            File.AppendAllText(archivoLog, mensajeConFecha + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR al escribir el archivo de log: {ex.Message}");
        }
    }
}

}