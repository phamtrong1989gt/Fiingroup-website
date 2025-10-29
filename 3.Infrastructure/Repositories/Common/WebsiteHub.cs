using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace PT.UI.SignalR
{
    public class WebsiteHub : Hub
    {
        public async Task SendMessage(object message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
