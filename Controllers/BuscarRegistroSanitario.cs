
using System.Net;
using BuscarRegistroSanitarioService.models;
using BuscarRegistroSanitarioService.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuscarRegistroSanitarioService.Controllers
{
    [ApiController]
    [Route("api/")]
    public class RegistroSanitarioController : ControllerBase
    {
        private readonly ScrapingService _scrapingService;

        public RegistroSanitarioController(ScrapingService scrapingService)
        {
            _scrapingService = scrapingService;
            _scrapingService.OnInitialized += (sender, args) =>
            {
                Console.WriteLine("El servicio de scraping se ha inicializado completamente.");
            };
        }

        /// <summary>
        /// Busca registros sanitarios por el nombre del producto.
        /// </summary>
        /// <param name="nombreProducto">Nombre completo o parcial del producto a buscar.</param>
        /// <returns>Información sobre los registros sanitarios encontrados.</returns>
        /// <response code="200">Registros encontrados.</response>
        /// <response code="400">El nombre del producto está vacío.</response>
        /// <response code="404">No se encontraron registros para el producto especificado.</response>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(ApiResponse<ProductData>), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, "Text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound, "Text/plain")]
        public async Task<IActionResult> BuscarRegistroSanitario([FromQuery] string nombreProducto)
        {
            if (string.IsNullOrEmpty(nombreProducto))
            {
                return BadRequest("El nombre del producto no puede estar vacío.");
            }

            var resultado = await _scrapingService.BuscarRegistroSanitario(nombreProducto);

            if (resultado == null || resultado.Data == null || resultado.Data.Count == 0)
            {
                return NotFound("No se encontraron resultados para el producto especificado.");
            }

            return Ok(resultado);
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

        /// <summary>
        /// Cambia el tipo de producto en el contexto actual.
        /// </summary>
        /// <param name="tipoProducto"></param>
        /// <returns>Confirmación del cambio o error si no fue exitoso.</returns>
        /// <response code="200">El tipo de producto se cambió exitosamente.</response>
        /// <response code="400">El nombre del tipo de producto no es válido. | Debe especificar el tipo de producto.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("cambiarTipo")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError, "application/json")]
        public IActionResult CambiarTipo([FromQuery] string tipoProducto)
        {
            TipoProducto tipo;

            if (!string.IsNullOrEmpty(tipoProducto))
            {
                if (Enum.TryParse<TipoProducto>(tipoProducto, out var parsedTipoNombre))
                {
                    tipo = parsedTipoNombre;
                }
                else
                {
                    return BadRequest("El nombre del tipo de producto no es válido.");
                }
            }
            else
            {
                return BadRequest("Debe especificar el tipo de producto.");
            }

            var resultado = _scrapingService.CambiarTipo(tipo);

            if (resultado.StatusCode == 200)
            {
                return Ok(resultado);
            }
            else
            {
                return StatusCode(500, "Error interno al cambiar el tipo de producto.");
            }
        }


        /// <summary>
        /// Obtiene la lista de tipos de productos disponibles.
        /// </summary>
        /// <returns>Lista de tipos de productos.</returns>
        /// <response code="200">Lista obtenida con éxito.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("tiposProducto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerTipos()
        {
            var resultado = _scrapingService.ObtenerTipos();

            if (resultado.StatusCode == 200)
            {
                return Ok(resultado);
            }
            else
            {
                return StatusCode(resultado.StatusCode, resultado);

            }
        }

        /// <summary>
        /// Verifica el estado del servicio de scraping.
        /// </summary>
        /// <returns>Estado del servicio.</returns>
        /// <response code="200">El servicio está listo.</response>
        /// <response code="503">El servicio se está inicializando.</response>
        [HttpGet("estado")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
        public IActionResult GetEstado()
        {
            if (_scrapingService.IsInitialized)
            {
                return Ok("El servicio está listo.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "El servicio está inicializando.");
            }

        }
    }
}
