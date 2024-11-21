
using System.Net;
using BuscarRegistroSanitarioService.services;
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


        [HttpGet("buscar")]
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

        [HttpGet("paginacion")]
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
        public IActionResult CambiarTipo([FromQuery] TipoProducto tipoProducto)
        {
            var resultado = _scrapingService.CambiarTipo(tipoProducto);

            if (resultado.StatusCode == 200)
            {
                return Ok(resultado);
            }
            else
            {
                return StatusCode(resultado.StatusCode, resultado);

            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
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
