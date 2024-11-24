using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BuscarRegistroSanitarioService.Hubs;


public class NotificationHub : Hub
{
    public async Task SendMessage(string message)
    {

        await Clients.All.SendAsync("ReceiveStatus", message);
    }
}