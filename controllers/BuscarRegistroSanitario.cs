
using BuscarRegistroSanitarioService.services;
using Microsoft.AspNetCore.Mvc;

namespace BuscarRegistroSanitarioService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistroSanitarioController : ControllerBase
    {
        private readonly ScrapingService _scrapingService;

        public RegistroSanitarioController(ScrapingService scrapingService)
        {
            _scrapingService = scrapingService;
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarRegistroSanitario([FromQuery] string? nombreProducto=null)
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

        [HttpGet("siguiente")]
        public async Task<IActionResult> PaginaSiguiente()
        {
            var resultado = await _scrapingService.paginar(BotonesPaginador.siguiente);

            if (resultado == null || resultado.Data == null || resultado.Data.Count == 0)
            {
                return NotFound("No se encontraron resultados para el producto especificado.");
            }

            return Ok(resultado);
        }

        [HttpGet("anterior")]
        public async Task<IActionResult> PaginaAnterior()
        {
            var resultado = await _scrapingService.paginar(BotonesPaginador.anterior);

            if (resultado == null || resultado.Data == null || resultado.Data.Count == 0)
            {
                return NotFound("No se encontraron resultados para el producto especificado.");
            }

            return Ok(resultado);
        }
    }
}
