using Microsoft.AspNetCore.SignalR;

namespace SUAP_PortalOficios.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task NotifyTableUpdate()
        {
            await Clients.All.SendAsync("ReceiveTableUpdate");
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task NotifyPdfUpdate(string message)
        {
            await Clients.All.SendAsync("ReceivePdfUpdate");
        }

        public async Task NotifyCantidadesUpdate()
        {
            await Clients.All.SendAsync("ReceiveCantidadesUpdate");
        }

        public async Task SendNotify(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotify", title, message);
        }
    }
}
