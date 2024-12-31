using System;
using BuscarRegistroSanitarioCR.Enums;

namespace BuscarRegistroSanitarioService.Exceptions;

public class AppException : Exception
{
    public ErrorCode ErrorCode { get; }

    public AppException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AppException(ErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, ErrorCode: {ErrorCode}";
    }
}
