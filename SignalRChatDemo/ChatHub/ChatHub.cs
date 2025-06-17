using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRChatDemo.ChatHub
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> userConnections = new();

        public override Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext().Request.Query["username"].ToString();
            userConnections[Context.ConnectionId] = user;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            userConnections.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        private string? GetConnectionIdByUser(string userId)
        {
            return userConnections.FirstOrDefault(x => x.Value == userId).Key;
        }

        public async Task SendStatusMessage(string userSelected, string guidMessage)
        {
            var connectionId = GetConnectionIdByUser(userSelected);
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveStatusMessage", userSelected, guidMessage);
            }
        }

        public async Task SendMessage(string userLogged, string userSelected, string message, string guidMessage, string time)
        {
            var connectionId = GetConnectionIdByUser(userSelected);

            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", userLogged, message, guidMessage, time);
            }
        }

        public async Task Typing(string userLogged, string userSelected)
        {
            var connectionId = GetConnectionIdByUser(userSelected);

            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", userLogged);
            }
        }
    }
}
