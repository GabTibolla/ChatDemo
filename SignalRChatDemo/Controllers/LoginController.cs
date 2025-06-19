using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ChatDemo.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ChatDemo.Business.Interfaces.IConfigService _configService;

        public LoginController(ChatDemo.Business.Interfaces.IConfigService configService)
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

            ChatDemo.DAO.UsersDB usersDB = ChatDemo.Business.Helper.CreateDBUsers(_configService);
            ChatDemo.Data.User? userExists = usersDB.GetUserByCpf(user.Cpf);

            if (userExists == null || userExists.Password != ChatDemo.Business.Helper.GerarHashSenha(user.Password))
            {
                ViewBag.Mensagem = "Usuário ou senha inválidos.";
                return View("Login", user);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userExists.Name),
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
