using BuscarRegistroSanitarioCR.Enums;

namespace BuscarRegistroSanitarioService.Exceptions;

public class DriverException : AppException
{
    public DriverException(string message)
        : base(ErrorCode.DriverInitializationError, message) { }

    public DriverException(string message, Exception innerException)
        : base(ErrorCode.DriverInitializationError, message, innerException) { }
}

public class ElementNotFoundException : AppException
{
    public ElementNotFoundException(string message)
        : base(ErrorCode.ElementNotFound, message) { }

    public ElementNotFoundException(string message, Exception innerException)
        : base(ErrorCode.ElementNotFound, message, innerException) { }
}


