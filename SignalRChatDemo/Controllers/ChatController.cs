using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SignalRChatDemo.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class ChatController : Controller
    {
        private readonly ChatDemo.Services.ConfigService _configServices;
        public ChatController(ChatDemo.Services.ConfigService configServices)
        {
            _configServices = configServices;
        }

        private static ChatDemo.Data.Chat _chat = new ChatDemo.Data.Chat();

        public IActionResult Conversa()
        {
            string? numberId = User.FindFirst("NumberId")?.Value;

            string connectionString = GetConnectionString();
            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices, connectionString);

            // Buscando lista de contatos
            List<ChatDemo.Data.Contacts>? listContacts = contactsDb.GetAllContacts(numberId);

            // Ordenando por última mensagem
            if (listContacts != null && listContacts.Count() > 0)
            {
                listContacts = listContacts.OrderByDescending(p => p.LastMessageDate).ToList();
            }

            _chat.Contacts = listContacts;

            return View("Conversa", _chat);
        }

        [HttpPost]
        public IActionResult ContatoSelecionado([FromBody] ChatDemo.Data.Contacts contato)
        {
            string connectionString = GetConnectionString();
            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices, connectionString);

            ChatDemo.Data.Contacts contact = contactsDb.GetContactByNumberId(contato.NumberId, contato.MyNumberId);
            if (contact == null)
            {
                return View("Conversa");
            }

            var messagesDB = ChatDemo.Helpers.Helpers.CreateDBMessages(_configServices, connectionString);
            var messages = messagesDB.GetMessagesByNumberId(contact);

            if (messages == null)
            {
                messages = new List<ChatDemo.Data.Messages>();
            }

            messages = messages.OrderBy(p => p.Datetime).ToList();

            _chat.SelectedContact = contact;
            _chat.Messages = messages;

            return View("Conversa", _chat);
        }

        private string GetConnectionString()
        {
            string? webId = User.FindFirst("WebId")?.Value;

            if (String.IsNullOrEmpty(webId))
            {
                throw new Exception("WebId not found");
            }

            // Criando banco de dados (ou acessando) para o usuário logado
            string path = Path.Combine("Database", "Users", webId.Substring(0, 1), webId.Substring(1, 1));
            Directory.CreateDirectory(path);
            string dbPath = Path.Combine(path, "database.db");
            string connectionString = $"Data Source={dbPath}";

            return connectionString;
        }
    }
}
