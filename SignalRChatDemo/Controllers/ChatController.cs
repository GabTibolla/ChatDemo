using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Conversa()
        {
            return View();
        }
    }
}
