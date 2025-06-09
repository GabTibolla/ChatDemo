using Microsoft.AspNetCore.SignalR;

namespace SignalRChatDemo.ChatHub
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task Typing(string user)
        {
            await Clients.Others.SendAsync("UserTyping", user);
        }
    }
}
