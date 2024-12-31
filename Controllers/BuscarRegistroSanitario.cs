namespace BuscarRegistroSanitarioService.Controllers;

using System.Net;
using BuscarRegistroSanitarioService.Enums;
using BuscarRegistroSanitarioService.DTO;
using BuscarRegistroSanitarioService.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BuscarRegistroSanitarioService.Exceptions;
using BuscarRegistroSanitarioCR.DTO;
using BuscarRegistroSanitarioService.Loggin;

[ApiController]
[Route("api/")]
public class RegistroSanitarioController : ControllerBase
{
    private readonly ScrapingService _scrapingService;
    private readonly ILoggerManager _logger;

    public RegistroSanitarioController(ScrapingService scrapingService, ILoggerManager logger)
    {
        _logger = logger;
        _scrapingService = scrapingService;
        _scrapingService.OnInitialized += (sender, args) =>
        {
            Console.WriteLine("El servicio de scraping se ha inicializado completamente.");
        };
    }

    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<RegistroSanitarioResultado<ProductData>>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "text/plain")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/json")]
    public async Task<IActionResult> BuscarRegistroSanitario([FromQuery] string nombreProducto)
    {
        if (string.IsNullOrWhiteSpace(nombreProducto))
        {
            return BadRequest("El nombre del producto no puede estar vacío.");
        }

        try
        {
            var resultado = await _scrapingService.BuscarRegistroSanitario(nombreProducto);
            if (resultado?.Data == null || resultado.Data.Count == 0)
            {
                return NotFound("No se encontraron resultados para el producto especificado.");
            }

            return Ok(new ApiResponse<RegistroSanitarioResultado<ProductData>>
            {
                Success = true,
                Data = resultado,
                Message = "Resultados obtenidos con éxito."
            });
        }
        catch (DriverException ex)
        {
            _logger.LogError($"Error durante la búsqueda: {ex.ErrorCode}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = ex.Message,
                Details = ex.InnerException?.Message ?? ex.Message,
                ErrorCode = ex is AppException appEx ? appEx.ErrorCode.ToString() : "Unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error inesperado: ", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "Ocurrió un error inesperado.",
                Details = ex.Message,
                ErrorCode = "500"
            });
        }
    }



    /// <summary>
    /// Navega a través de los resultados de búsqueda utilizando paginación.
    /// </summary>
    /// <param name="comando">Dirección de navegación: "siguiente" o "anterior".</param>
    /// <returns>Resultados de la página actual.</returns>
    /// <response code="200">Página obtenida con éxito.</response>
    /// <response code="204">No hay más páginas disponibles.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("paginacion")]
    [ProducesResponseType(typeof(ApiResponse<ProductData>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Paginar([FromQuery] string comando)
    {
        BotonesPaginador boton = BotonesPaginador.siguiente;
        if (comando.ToLower() == "anterior")
        {
            boton = BotonesPaginador.anterior;
        }
        var resultado = await _scrapingService.paginar(boton);


        if (resultado.StatusCode == 204)
        {

            return NoContent();
        }
        else if (resultado.StatusCode == 200)
        {
            return Ok(resultado);
        }
        else
        {
            return StatusCode(resultado.StatusCode, resultado.Message);
        }
    }

   [HttpGet("cambiarTipo")]
[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK, "application/json")]
[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest, "application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/json")]
public IActionResult CambiarTipo([FromQuery] string tipoProducto)
{
    try
    {
        if (string.IsNullOrEmpty(tipoProducto))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "Debe especificar el tipo de producto."
            });
        }

        if (!Enum.TryParse<TipoProducto>(tipoProducto, out var tipo))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "El nombre del tipo de producto no es válido."
            });
        }

        var resultado = _scrapingService.CambiarTipo(tipo);

        if (resultado.StatusCode == (int)HttpStatusCode.OK)
        {
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = resultado.Data,
                Message = "Tipo de producto cambiado con éxito."
            });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
        {
            Message = "Error interno al cambiar el tipo de producto.",
            Details = resultado.Message,
            ErrorCode = "500"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError("Error al cambiar el tipo de producto: ", ex);
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
        {
            Message = "Ocurrió un error al cambiar el tipo de producto.",
            Details = ex.Message,
            ErrorCode = "500"
        });
    }
}


[HttpGet("tiposProducto")]
[ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK, "application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/json")]
public IActionResult ObtenerTipos()
{
    try
    {
        var resultado = _scrapingService.ObtenerTipos();

        if (resultado.StatusCode == (int)HttpStatusCode.OK)
        {
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = resultado.Data,
                Message = "Tipos de productos obtenidos con éxito."
            });
        }

        return StatusCode(resultado.StatusCode, new ErrorResponse
        {
            Message = resultado.Message ?? "Error desconocido al obtener tipos de productos.",
            ErrorCode = resultado.StatusCode.ToString()
        });
    }
    catch (Exception ex)
    {
        _logger.LogError("Error al obtener tipos de productos: ", ex);
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
        {
            Message = "Ocurrió un error al obtener los tipos de productos.",
            Details = ex.Message,
            ErrorCode = "500"
        });
    }
}


    [HttpGet("estado")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status503ServiceUnavailable, "application/json")]
    public IActionResult ObtenerEstado()
    {
        try
        {
            if (_scrapingService.IsInitialized)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "El servicio está listo.",
                    Message = "Servicio en línea."
                });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ApiResponse<string>
            {
                Success = false,
                Data = "El servicio está inicializando.",
                Message = "Servicio no disponible."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error al verificar el estado del servicio: ", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "Ocurrió un error al verificar el estado del servicio.",
                Details = ex.Message,
                ErrorCode = "500"
            });
        }
    }

}
