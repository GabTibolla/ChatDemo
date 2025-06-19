using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRChatDemo.ChatHub
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> userConnections = new();

        #region [ Funções de gerenciamento de conexão do usuário ] 

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

        #endregion

        public async Task SendStatusMessage(string currentContact, string guidMessage)
        {
            var connectionId = GetConnectionIdByUser(currentContact);
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveStatusMessage", guidMessage);
            }
        }

        public async Task SendMessage(string ownerNumberId, string currentContact, string message, string guidMessage, string time)
        {
            var connectionId = GetConnectionIdByUser(currentContact);

            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", ownerNumberId, message, guidMessage, time);
            }
        }

        public async Task Typing(string ownerNumberId, string currentContact)
        {
            var connectionId = GetConnectionIdByUser(currentContact);

            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", ownerNumberId);
            }
        }
    }
}
