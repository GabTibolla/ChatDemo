using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

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

        [HttpGet]
        public IActionResult Conversa()
        {
            string? userJson = User.FindFirst("User")?.Value;
            ChatDemo.Data.User? user = JsonSerializer.Deserialize<ChatDemo.Data.User>(userJson);

            string connectionString = GetConnectionString();
            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices, connectionString);

            // Buscando lista de contatos
            List<ChatDemo.Data.Contacts>? listContacts = contactsDb.GetAllContacts(user.NumberId);

            // Ordenando por última mensagem
            if (listContacts != null && listContacts.Count() > 0)
            {
                listContacts = listContacts.OrderByDescending(p => p.LastMessageDate).ToList();
            }

            ChatDemo.Data.Chat chat = new ChatDemo.Data.Chat();
            chat.Contacts = listContacts;
            chat.SelectedContact = null;// new ChatDemo.Data.Contacts();
            chat.UserLogged = user;
            chat.Messages = new List<ChatDemo.Data.Messages>();

            return View("Conversa", chat);
        }

        [HttpPost]
        public IActionResult ContatoSelecionado([FromBody] ChatDemo.Data.Contacts contato)
        {
            ChatDemo.Data.Chat chat = new ChatDemo.Data.Chat();

            string connectionString = GetConnectionString();
            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices, connectionString);

            ChatDemo.Data.Contacts contact = contactsDb.GetContactByWebIdAndNumberId(contato.WebId, contato.MyNumberId);
            if (contact == null)
            {
                // Se não achou o contato, manda uma área de chat vazia
                chat.SelectedContact = null;
                chat.Messages = new List<ChatDemo.Data.Messages>();
                return PartialView("_ChatArea", chat);
            }

            var messagesDB = ChatDemo.Helpers.Helpers.CreateDBMessages(_configServices, connectionString);
            var messages = messagesDB.GetMessagesByNumberId(contact);

            if (messages == null)
            {
                messages = new List<ChatDemo.Data.Messages>();
            }

            messages = messages.OrderBy(p => p.Datetime).ToList();

            chat.SelectedContact = contact;
            chat.Messages = messages;

            return PartialView("_ChatArea", chat);
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
