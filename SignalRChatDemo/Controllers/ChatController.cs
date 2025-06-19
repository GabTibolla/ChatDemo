using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SignalRChatDemo.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")]
    public class ChatController : Controller
    {
        private readonly ChatDemo.Business.Interfaces.IConfigService _configServices;
        public ChatController(ChatDemo.Business.Interfaces.IConfigService configServices)
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
        public IActionResult AddContact(string Alias, string ContactNumberId, string OwnerNumberId)
        {
            if (string.IsNullOrEmpty(ContactNumberId) || string.IsNullOrEmpty(OwnerNumberId))
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Todos os campos são obrigatórios.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            if (ContactNumberId == OwnerNumberId)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Não é possível adicionar com o mesmo NumberId.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var userDb = ChatDemo.Business.Helper.CreateDBUsers(_configServices);
            if (userDb == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Erro ao acessar o banco de dados de usuários.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            ChatDemo.Data.User? user = userDb.GetUserByNumberId(ContactNumberId);
            if (user == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Usuário não encontrado.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var contactDB = ChatDemo.Business.Helper.CreateDBContacts(_configServices);
            if (contactDB == null)
            {
                ViewBag.OpenModal = true;
                ViewBag.Error = "Erro ao acessar o banco de dados de contatos.";
                var chat = MyModel();
                return View("Conversa", chat);
            }

            var newContact = new ChatDemo.Data.Contacts
            {
                Alias = Alias,
                ContactNumberId = user.NumberId,
                OwnerNumberId = OwnerNumberId
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
        public IActionResult ConversationSelected([FromBody] ChatDemo.Data.Conversation payload)
        {
            ChatDemo.Data.Chat chat = MyModel();
            string conversationId = ChatDemo.Business.Helper.GerarConversationId(payload.OwnerNumberId, payload.ContactNumberId);

            var conversationDB = ChatDemo.Business.Helper.CreateDBConversations(_configServices);
            ChatDemo.Data.Conversation? conversation = conversationDB.GetConversation(conversationId);

            if (conversation == null)
            {
                // Se não achou a conversa, cria uma conversa nova
                conversation = new ChatDemo.Data.Conversation
                {
                    Id = conversationId,
                    ContactNumberId = payload.ContactNumberId,
                    OwnerNumberId = payload.OwnerNumberId,
                    LastMessage = null
                };

                bool value = conversationDB.CreateConversation(conversation);
                if (!value)
                {
                    // Retorna uma área vazia
                    chat.CurrentConversation = null;
                    chat.Messages = new List<ChatDemo.Data.Message>();
                    return PartialView("_ChatArea", chat);
                }
            }

            conversation.OwnerNumberId = payload.OwnerNumberId;
            conversation.ContactNumberId = payload.ContactNumberId;

            var messagesDB = ChatDemo.Business.Helper.CreateDBMessages(_configServices);
            var messages = messagesDB.GetMessages(conversationId);

            if (messages == null)
            {
                messages = new List<ChatDemo.Data.Message>();
            }

            messages = messages.OrderBy(p => p.Datetime).ToList();

            chat.CurrentConversation = conversation;
            chat.Messages = messages;

            return PartialView("_ChatArea", chat);
        }

        [HttpPost]
        public IActionResult ReturnMessagesUnread([FromBody] ChatDemo.Data.Conversation payload)
        {
            var messages = GetUnreadMessages(payload.OwnerNumberId, payload.ContactNumberId);

            return new JsonResult(new { messages = messages });
        }

        [HttpGet]
        public IActionResult UpdateStatusMessage(string OwnerNumberId, string ContactNumberId)
        {
            if (string.IsNullOrEmpty(OwnerNumberId) || string.IsNullOrEmpty(ContactNumberId))
            {
                return StatusCode(500, "Mensagem não enviada.");
            }

            ChatDemo.DAO.MessagesDB messagesDB = ChatDemo.Business.Helper.CreateDBMessages(_configServices);

            // Gera o ConversationId
            string conversationId = ChatDemo.Business.Helper.GerarConversationId(OwnerNumberId, ContactNumberId);

            bool messageSaved = messagesDB.UpdateStatusMessageToRead(conversationId, OwnerNumberId);

            if (!messageSaved)
            {
                return StatusCode(500, "Mensagens não atualizadas.");
            }

            return Ok("Mensagem salva com sucesso.");
        }

        [HttpGet]
        public IActionResult UpdateContactsList()
        {
            System.Threading.Thread.Sleep(1000);
            var chat = MyModel();
            return PartialView("_ConversationList", chat);
        }

        [HttpPost]
        public IActionResult UpdateNotifierIcon(string ContactNumberId, string OwnerNumberId)
        {
            System.Threading.Thread.Sleep(500);
            var messages = GetUnreadMessages(OwnerNumberId, ContactNumberId);

            messages = messages.Where(p => p.SenderNumberId != OwnerNumberId).ToList();

            ChatDemo.DAO.ContactsDB contactDB = ChatDemo.Business.Helper.CreateDBContacts(_configServices);
            int unreadMessages = messages?.Count ?? 0;

            // Atualiza
            contactDB.UpdateUnreadMessages(unreadMessages, ContactNumberId, OwnerNumberId);
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveMessage(string OwnerNumberId, string ContactNumberId, string Message, string GuidMessage, DateTime datetime)
        {
            if (string.IsNullOrEmpty(OwnerNumberId) || string.IsNullOrEmpty(ContactNumberId) || string.IsNullOrEmpty(Message))
            {
                return StatusCode(500, "Mensagem não enviada.");
            }

            string conversationId = ChatDemo.Business.Helper.GerarConversationId(OwnerNumberId, ContactNumberId);

            ChatDemo.DAO.UsersDB usersDB = ChatDemo.Business.Helper.CreateDBUsers(_configServices);

            ChatDemo.DAO.MessagesDB messagesDB = ChatDemo.Business.Helper.CreateDBMessages(_configServices);

            ChatDemo.Data.Message newMessage = new ChatDemo.Data.Message
            {
                Text = Message,
                Datetime = datetime.ToLocalTime(),
                SenderNumberId = OwnerNumberId,
                ConversationId = conversationId,
                Status = ChatDemo.Data.Message.StatusMessage.Sent,
                WebId = GuidMessage
            };

            bool messageSaved = messagesDB.AddMessage(newMessage, ContactNumberId);

            if (!messageSaved)
            {
                return StatusCode(500, "Mensagem não inserida");
            }

            return Ok("Mensagem salva com sucesso.");
        }

        private List<ChatDemo.Data.Message> GetUnreadMessages(string OwnerNumberId, string ContactNumberId)
        {
            string conversationId = ChatDemo.Business.Helper.GerarConversationId(OwnerNumberId, ContactNumberId);

            var messagesDB = ChatDemo.Business.Helper.CreateDBMessages(_configServices);
            var messages = messagesDB.GetMessages(conversationId);

            if (messages == null)
            {
                messages = new List<ChatDemo.Data.Message>();
            }

            messages = messages.OrderBy(p => p.Datetime).ToList();
            messages = messages.Where(p => p.Status != ChatDemo.Data.Message.StatusMessage.Read).ToList();

            return messages;
        }

        private ChatDemo.Data.Chat MyModel()
        {
            string? userJson = User.FindFirst("User")?.Value;
            ChatDemo.Data.User? user = JsonSerializer.Deserialize<ChatDemo.Data.User>(userJson);

            var contactsDb = ChatDemo.Business.Helper.CreateDBContacts(_configServices);


            // Buscando lista de conversas/contatos
            List<ChatDemo.Data.Contacts>? listContacts = contactsDb.GetContacts(user.NumberId);

            // Percorre a lista de contatos procurando se existe a conversa
            List<ChatDemo.Data.Conversation>? listConversations = new List<ChatDemo.Data.Conversation>();

            if (listContacts != null && listContacts.Count > 0)
            {
                var conversationsDB = ChatDemo.Business.Helper.CreateDBConversations(_configServices);

                foreach (var i in listContacts)
                {
                    var conversationId = ChatDemo.Business.Helper.GerarConversationId(i.ContactNumberId, user.NumberId);
                    var conversation = conversationsDB.GetConversation(conversationId);

                    if (conversation != null)
                    {
                        listConversations.Add(conversation);
                    }
                }
            }

            if (listConversations != null && listConversations.Count > 0)
            {
                listContacts = listContacts
                    .OrderByDescending(contact =>
                    {
                        var convId = ChatDemo.Business.Helper.GerarConversationId(contact.ContactNumberId, user.NumberId);
                        var conv = listConversations.FirstOrDefault(c => c.Id == convId);
                        return conv?.LastMessage?.Datetime;
                    })
                    .ToList();
            }


            ChatDemo.Data.Chat chat = new ChatDemo.Data.Chat();
            chat.Conversations = listConversations;
            chat.Contacts = listContacts;
            chat.User = user;
            chat.Messages = new List<ChatDemo.Data.Message>();

            chat.CurrentConversation = null;

            return chat;
        }
    }
}
