using Microsoft.AspNetCore.Mvc;

namespace SignalRChatDemo.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Conversa()
        {
            return View();
        }

        public IActionResult Cadastro()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
