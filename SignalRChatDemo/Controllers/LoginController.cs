using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ChatDemo.Controllers
{
    public class LoginController : Controller
    {
        private readonly ChatDemo.Services.ConfigService _configService;

        public LoginController(ChatDemo.Services.ConfigService configService)
        {
            _configService = configService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> DoLogin(ChatDemo.Data.User user)
        {
            #region [ + Validação ]

            if (string.IsNullOrEmpty(user.Cpf) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.Mensagem = "CPF e senha são obrigatórios.";
                return View("Login", user);
            }

            #endregion

            ChatDemo.DAO.UsersDB usersDB = ChatDemo.Helpers.Helpers.CreateDBUsers(_configService);
            ChatDemo.Data.User? userExists = usersDB.GetUserByCpf(user.Cpf);

            if (userExists == null || userExists.Password != ChatDemo.Helpers.Helpers.GerarHashSenha(user.Password))
            {
                ViewBag.Mensagem = "Usuário ou senha inválidos.";
                return View("Login", user);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userExists.Name),
                new Claim("WebId", userExists.WebId),
                new Claim("User", JsonSerializer.Serialize(userExists))
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Login");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
            return RedirectToAction("Conversa", "Chat");
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return Json(new { redirectUrl = Url.Action("Index", "Home") });
        }
    }
}
