using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuscarRegistroSanitarioService.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SignalDocsController : ControllerBase
    {
    /// <summary>
        /// Documentación del SignalR Hub para notificaciones.
        /// </summary>
        /// <remarks>
        /// Para conectarte, usa esta URL como base para SignalR: `/notifications`
        /// Ejemplo de cliente:
        /// ```javascript
        /// const connection = new signalR.HubConnectionBuilder()
        ///    .withUrl("/notifications")
        ///    .build();
        ///
        /// connection.on("ReceiveStatus", function (message) {
        ///     console.log("Status: " + message);
        /// });
        ///
        /// connection.start().catch(err => console.error(err));
        /// ```
        /// </remarks>
        /// <response code="200">Información del SignalR Hub.</response>
    [HttpGet("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSignalRInfo()
    {
        return Ok(new
        {
            HubEndpoint = "/notifications",
            Methods = new[]
            {
                "ReceiveStatus(string message)",
                "SendMessage(string user, string message)"
            },
            ExampleClientCode = @"
                const connection = new signalR.HubConnectionBuilder()
                   .withUrl('/notifications')
                   .build();

                connection.on('ReceiveStatus', function (message) {
                   console.log('Status: ' + message);
                });

                connection.start().catch(err => console.error(err));
            "
        });
    }
    }
}
