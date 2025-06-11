using ChatDemo.Services;
using System.Security.Cryptography;
using System.Text;

namespace ChatDemo.Helpers
{
    public static class Helpers
    {
        #region [ + Senha ]
        public static string GerarHashSenha(string senha)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Converte a senha para bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));

                // Converte bytes para string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // "x2" → 2 dígitos hexadecimais
                }

                return builder.ToString();
            }
        }

        #endregion

        #region [ + CPF ]
        public static bool ValidateCPF(string cpf)
        {
            if (cpf.Contains("."))
                cpf = cpf.Replace(".", "");

            if (cpf.Contains("-"))
                cpf = cpf.Replace("-", "");

            if (cpf.Count() < 11 || cpf.Count() > 11)
                return false;

            int digito1 = ReturnDigit(cpf);
            int digito2 = ReturnDigit(cpf, digito1, true);

            if (int.Parse(cpf.Last().ToString()) != digito2)
            {
                return false;
            }

            if (int.Parse(cpf[cpf.Length - 2].ToString()) != digito1)
            {
                return false;
            }

            return true;
        }

        private static int ReturnDigit(string cpf, int digito1 = 0, bool digito2 = false)
        {
            int soma = 0;
            int resto = 0;
            int digito;

            int decrement = 10;

            if (digito2)
            {
                decrement = 11;
            }

            foreach (char a in cpf)
            {
                soma += int.Parse(a.ToString()) * decrement;
                decrement--;

                if (decrement == 2)
                {
                    if (digito2)
                    {
                        soma += digito1 * decrement;
                    }

                    break;
                }
            }

            resto = (soma * 10) % 11;

            digito = resto;
            if (resto == 10 || resto == 11)
            {
                digito = 0;
            }

            return digito;
        }

        #endregion

        #region [ + DB ]

        public static ChatDemo.DAO.UsersDB CreateDBUsers(ConfigService _configService)
        {
            string? connectionString = _configService.Get<string>("ConnectionString");
            string? className = _configService.Get<string>("Users:ClassName");
            string? projectName = _configService.Get<string>("Users:ProjectName");

            try
            {

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(projectName))
                {
                    throw new Exception("Configurações de banco de dados não encontradas.");
                }

                ChatDemo.DAO.UsersDB? DBAccess = ChatDemo.DAO.UsersDB.Create(className, projectName, connectionString);
                if (DBAccess == null)
                {
                    throw new Exception("Erro ao criar instância do banco de dados.");
                }

                return DBAccess;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ChatDemo.DAO.ContactsDB CreateDBContacts(ConfigService _configService)
        {
            string? connectionString = _configService.Get<string>("ConnectionString");
            string? className = _configService.Get<string>("Contacts:ClassName");
            string? projectName = _configService.Get<string>("Contacts:ProjectName");

            try
            {
                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(projectName))
                {
                    throw new Exception("Configurações de banco de dados não encontradas.");
                }

                ChatDemo.DAO.ContactsDB? DBAccess = ChatDemo.DAO.ContactsDB.Create(className, projectName, connectionString);
                if (DBAccess == null)
                {
                    throw new Exception("Erro ao criar instância do banco de dados.");
                }

                return DBAccess;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ChatDemo.DAO.MessagesDB CreateDBMessages(ConfigService _configService)
        {
            string? connectionString = _configService.Get<string>("ConnectionString");
            string? className = _configService.Get<string>("Messages:ClassName");
            string? projectName = _configService.Get<string>("Messages:ProjectName");

            try
            {
                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(projectName))
                {
                    throw new Exception("Configurações de banco de dados não encontradas.");
                }

                ChatDemo.DAO.MessagesDB? DBAccess = ChatDemo.DAO.MessagesDB.Create(className, projectName, connectionString);
                if (DBAccess == null)
                {
                    throw new Exception("Erro ao criar instância do banco de dados.");
                }

                return DBAccess;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [ + WebID ]

        public static string GenerateWebId()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion

        #region [ + Identificador ]

        public static string GerarNumberId()
        {
            string baseString = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    builder.Append(bytes[i].ToString("x2").ToUpper());
                }

                return builder.ToString();
            }
        }

        #endregion
    }


}
