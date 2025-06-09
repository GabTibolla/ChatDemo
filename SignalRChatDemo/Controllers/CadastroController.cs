using Microsoft.AspNetCore.Mvc;

namespace ChatDemo.Controllers
{
    public class CadastroController : Controller
    {
        private readonly ChatDemo.Services.ConfigService _configService;

        public CadastroController(ChatDemo.Services.ConfigService configService)
        {
            _configService = configService;
        }

        public IActionResult Cadastro()
        {
            return View();
        }

        public IActionResult Register(ChatDemo.Data.User user, string confirmarSenha)
        {
            #region [ + Valida os dados ]

            if (string.IsNullOrEmpty(user.Name)
                || string.IsNullOrEmpty(user.Cpf)
                || string.IsNullOrEmpty(user.Password)
                || string.IsNullOrEmpty(confirmarSenha))
            {
                ViewBag.Mensagem = "Todos os campos são obrigatórios";
                return View("Cadastro", user);
            }

            bool cpfValido = ChatDemo.Helpers.Helpers.ValidateCPF(user.Cpf);

            if (!cpfValido)
            {
                ViewBag.Mensagem = "CPF inválido";
                return View("Cadastro", user);
            }

            if (user.Password != confirmarSenha)
            {
                ViewBag.Mensagem = "As senhas não conferem";
                return View("Cadastro", user);
            }

            #endregion

            try
            {
                ChatDemo.DAO.UsersDB usersDB = ChatDemo.Helpers.Helpers.CreateDBUsers(_configService);

                // Verifica se o usuário já existe
                ChatDemo.Data.User? userExistente = usersDB.GetUserByCpf(user.Cpf);
                if (userExistente != null)
                {
                    ViewBag.Mensagem = "Usuário já cadastrado.";
                    return View("Cadastro", user);
                }

                user.Password = ChatDemo.Helpers.Helpers.GerarHashSenha(user.Password);
                user.WebId = ChatDemo.Helpers.Helpers.GenerateWebId();
                user.NumberId = ChatDemo.Helpers.Helpers.GerarNumberId();

                bool value = usersDB.AddUser(user);

                if (!value)
                {
                    ViewBag.Mensagem = "Erro ao cadastrar usuário.";
                    return View("Cadastro", user);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensagem = ex.Message;
                return View("Cadastro", user);
            }

            ViewBag.Mensagem = "Usuário cadastrado com sucesso!";
            return View("Cadastro");
        }
    }
}
