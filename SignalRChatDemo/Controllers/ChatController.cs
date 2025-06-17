using ChatDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Conversa()
        {
            var chat = MyModel();
            return View("Conversa", chat);
        }

        [HttpPost]
        public IActionResult AddUser(string name, string numberId, string myNumberId)
        {
            if (string.IsNullOrEmpty(numberId) || string.IsNullOrEmpty(myNumberId))
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Todos os campos são obrigatórios.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            if (numberId == myNumberId)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Não é possível adicionar com o mesmo NumberId.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var userDb = ChatDemo.Helpers.Helpers.CreateDBUsers(_configServices);
            if (userDb == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Erro ao acessar o banco de dados de usuários.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            ChatDemo.Data.User? user = userDb.GetUserByNumberId(numberId);
            if (user == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Usuário não encontrado.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var contactDB = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices);
            if (contactDB == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Erro ao acessar o banco de dados de contatos.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var newContact = new ChatDemo.Data.Contacts
            {
                Name = name,
                NumberId = user.NumberId,
                MyNumberId = myNumberId,
                WebId = user.WebId
            };

            bool created = contactDB.CreateContact(newContact);
            if (!created)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Não foi possível criar o contato.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            return RedirectToAction("Conversa");
        }

        [HttpPost]
        public IActionResult ContatoSelecionado([FromBody] ChatDemo.Data.Contacts contato)
        {
            ChatDemo.Data.Chat chat = MyModel();
            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices);

            ChatDemo.Data.Contacts contact = contactsDb.GetContactByWebIdAndNumberId(contato.WebId, contato.MyNumberId);
            if (contact == null)
            {
                // Se não achou o contato, manda uma área de chat vazia
                chat.SelectedContact = null;
                chat.Messages = new List<ChatDemo.Data.Message>();
                return PartialView("_ChatArea", chat);
            }

            // Atualiza mensagens não lidas
            AtualizaStatusMessage(chat.UserLogged.WebId, contact.WebId);

            var messagesDB = ChatDemo.Helpers.Helpers.CreateDBMessages(_configServices);
            var messages = messagesDB.GetMessagesByNumberId(contact.MyNumberId, contact.NumberId);

            if (messages == null)
            {
                messages = new List<ChatDemo.Data.Message>();
            }

            messages = messages.OrderBy(p => p.Datetime).ToList();

            chat.SelectedContact = contact;
            chat.Messages = messages;

            return PartialView("_ChatArea", chat);
        }

        [HttpGet]
        public IActionResult AtualizaStatusMessage(string WIDFrom, string WIDTo)
        {
            if (string.IsNullOrEmpty(WIDFrom) || string.IsNullOrEmpty(WIDTo))
            {
                return StatusCode(500, "Mensagem não enviada.");
            }

            ChatDemo.DAO.UsersDB usersDB = ChatDemo.Helpers.Helpers.CreateDBUsers(_configServices);
            ChatDemo.Data.User? userFrom = usersDB.GetUserByWebId(WIDFrom);
            if (userFrom == null)
            {
                return StatusCode(500, "Usuário não encontrado.");
            }

            ChatDemo.Data.User? userTo = usersDB.GetUserByWebId(WIDTo);
            if (userTo == null)
            {
                return StatusCode(500, "Usuário não encontrado.");
            }

            ChatDemo.DAO.MessagesDB messagesDB = ChatDemo.Helpers.Helpers.CreateDBMessages(_configServices);
            bool messageSaved = messagesDB.UpdateStatusMessageToRead(userFrom, userTo);

            if (!messageSaved)
            {
                return StatusCode(500, "Mensagens não atualizadas.");
            }

            return Ok("Mensagem salva com sucesso.");
        }

        [HttpGet]
        public IActionResult AtualizaListaDeContatos()
        {
            System.Threading.Thread.Sleep(1000);
            var chat = MyModel();
            return PartialView("_ContactsList", chat);
        }

        [HttpPost]
        public IActionResult SalvarMensagem(string WIDFrom, string WIDTo, string message, DateTime datetime)
        {
            if (string.IsNullOrEmpty(WIDFrom) || string.IsNullOrEmpty(WIDTo) || string.IsNullOrEmpty(message))
            {
                return StatusCode(500, "Mensagem não enviada.");
            }

            ChatDemo.DAO.UsersDB usersDB = ChatDemo.Helpers.Helpers.CreateDBUsers(_configServices);
            ChatDemo.Data.User? userFrom = usersDB.GetUserByWebId(WIDFrom);
            if (userFrom == null)
            {
                return StatusCode(500, "Usuário não encontrado.");
            }

            ChatDemo.Data.User? userTo = usersDB.GetUserByWebId(WIDTo);
            if (userTo == null)
            {
                return StatusCode(500, "Usuário não encontrado.");
            }

            ChatDemo.DAO.MessagesDB messagesDB = ChatDemo.Helpers.Helpers.CreateDBMessages(_configServices);
            ChatDemo.Data.Message newMessage = new ChatDemo.Data.Message
            {
                Text = message,
                Datetime = datetime.ToLocalTime(),
                FromNumberId = userFrom.NumberId,
                ToNumberId = userTo.NumberId
            };

            bool messageSaved = messagesDB.AddMessage(newMessage);

            if (!messageSaved)
            {
                return StatusCode(500, "Mensagem não inserida");
            }

            return Ok("Mensagem salva com sucesso.");
        }

        // Função para criar o modelo de chat
        private ChatDemo.Data.Chat MyModel()
        {
            string? userJson = User.FindFirst("User")?.Value;
            ChatDemo.Data.User? user = JsonSerializer.Deserialize<ChatDemo.Data.User>(userJson);

            var contactsDb = ChatDemo.Helpers.Helpers.CreateDBContacts(_configServices);

            // Buscando lista de contatos
            List<ChatDemo.Data.Contacts>? listContacts = contactsDb.GetAllContacts(user.NumberId);

            // Ordenando por última mensagem
            if (listContacts != null && listContacts.Count() > 0)
            {
                listContacts = listContacts.OrderByDescending(p => p.LastMessageDate).ToList();
            }

            ChatDemo.Data.Chat chat = new ChatDemo.Data.Chat();
            chat.Contacts = listContacts;
            chat.SelectedContact = null;
            chat.UserLogged = user;
            chat.Messages = new List<ChatDemo.Data.Message>();

            return chat;
        }
    }
}
