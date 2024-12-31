
namespace BuscarRegistroSanitarioService.Loggin;

  public interface ILoggerManager
{
    void LogInfo(string mensaje);
    void LogWarning(string mensaje);
    void LogError(string mensaje, Exception? ex = null);

}
