using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KalkamanovaFinalConsole
{
    /// <summary>
    /// Главный класс программы.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// HTTP клиент для взаимодействия с сервером.
        /// </summary>
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Токен доступа, используемый для аутентификации пользователя.
        /// </summary>
        private static string _accessToken;

        /// <summary>
        /// Главный метод программы, запускающий основной цикл интерфейса.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Вход");
                Console.WriteLine("3. Создать сделку");
                Console.WriteLine("4. Получить последнюю сделку");
                Console.WriteLine("5. Выйти из аккаунта");
                Console.WriteLine("0. Завершить программу");

                int.TryParse(Console.ReadLine(), out int choice);

                switch (choice)
                {
                    case 1:
                        await Register();
                        break;
                    case 2:
                        await Login();
                        break;
                    case 3:
                        await CreateTrade();
                        break;
                    case 4:
                        await GetLatestTrade();
                        break;
                    case 5:
                        await LogOff();
                        break;
                    case 0:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        /// <summary>
        /// Метод для регистрации нового пользователя.
        /// </summary>
        private static async Task Register()
        {
            Console.Write("Email: ");
            var email = Console.ReadLine();
            if (!ValidateEmail(email))
            {
                Console.WriteLine("Неверный формат email.");
                return;
            }

            Console.Write("Пароль: ");
            var password = Console.ReadLine();
            if (!ValidatePassword(password))
            {
                Console.WriteLine("Пароль должен содержать хотя бы одну заглавную букву, одну цифру и один специальный символ.");
                return;
            }

            Console.Write("Подтвердите пароль: ");
            var confirmPassword = Console.ReadLine();
            if (password != confirmPassword)
            {
                Console.WriteLine("Пароли не совпадают.");
                return;
            }


            Console.Write("Доменное имя пользователя: ");
            var userDomainName = Console.ReadLine();

            var data = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                UserDomainName = userDomainName
            };

            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/Register", data);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var registrationResult = JsonConvert.DeserializeObject<RegistrationResult>(json);
                    Console.WriteLine(registrationResult.Message);
                }
                else
                {
                    Console.WriteLine("Регистрация не удалась.");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Возникла ошибка при отправке запроса:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception:");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Метод для входа пользователя в систему.
        /// </summary>
        private static async Task Login()
        {
            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Пароль: ");
            var password = Console.ReadLine();

            var data = new
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Email или пароль пусты.");
                return;
            }

            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/Login", data);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                    _accessToken = tokenResponse.AccessToken;
                    Console.WriteLine("Вход выполнен успешно.");
                }
                else
                {
                    Console.WriteLine("Вход не удался.");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Возникла ошибка при отправке запроса:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception:");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Метод для создания новой сделки.
        /// </summary>
        private static async Task CreateTrade()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("Пожалуйста, сначала выполните вход.");
                return;
            }

            Console.Write("Сумма: ");
            decimal.TryParse(Console.ReadLine(), out decimal amount);

            var trade = new Trade
            {
                Amount = amount
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/CreateTrade", trade);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Сделка успешно создана.");
            }
            else
            {
                Console.WriteLine("Не удалось создать сделку.");
            }
        }

        /// <summary>
        /// Метод для получения последней сделки.
        /// </summary>
        private static async Task GetLatestTrade()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("Пожалуйста, сначала выполните вход.");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await client.GetAsync("http://localhost:62130/api/Account/GetLatestTrade");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tradeResult = JsonConvert.DeserializeObject<TradeResult>(json);
                Console.WriteLine($"Дата: {tradeResult.Date}");
                Console.WriteLine($"Сумма: {tradeResult.Amount}");
            }
            else
            {
                Console.WriteLine("Не удалось получить последнюю сделку.");
            }
        }

        /// <summary>
        /// Метод для выхода пользователя из системы.
        /// </summary>
        private static async Task LogOff()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("Пожалуйста, сначала выполните вход.");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/CreateTrade", new {});
            if (response.IsSuccessStatusCode)
            {
                _accessToken = null;
                Console.WriteLine("Вы успешно вышли из системы.");
            }
            else
            {
                Console.WriteLine("Не удалось выйти из системы.");
            }
        }

        /// <summary>
        /// Валидация email адреса.
        /// </summary>
        /// <param name="email">Email адрес для проверки.</param>
        /// <returns>True, если адрес валидный, иначе False.</returns>
        private static bool ValidateEmail(string email)
        {
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        /// <summary>
        /// Валидация пароля.
        /// </summary>
        /// <param name="password">Пароль для проверки.</param>
        /// <returns>True, если пароль валидный, иначе False.</returns>
        private static bool ValidatePassword(string password)
        {
            const string pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+{};:,<.>]).+$";
            return Regex.IsMatch(password, pattern);
        }
    }

    /// <summary>
    /// Класс, представляющий ответ с токеном.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Токен доступа.
        /// </summary>
        [JsonProperty("AccessToken")]
        public string AccessToken { get; set; }
    }

    /// <summary>
    /// Класс, представляющий сделку.
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Сумма сделки.
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Класс, представляющий результат сделки.
    /// </summary>
    public class TradeResult
    {
        /// <summary>
        /// Дата сделки.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Сумма сделки.
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Класс, представляющий результат регистрации пользователя.
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Сообщение о результате регистрации.
        /// </summary>
        [JsonProperty("Message")]
        public string Message { get; set; }

        /// <summary>
        /// Пользователь, зарегистрированный в системе.
        /// </summary>
        [JsonProperty("User")]
        public User User { get; set; }
    }

    /// <summary>
    /// Класс, представляющий пользователя.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// Email пользователя.
        /// </summary>
        [JsonProperty("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [JsonProperty("UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// Доменное имя пользователя.
        /// </summary>
        [JsonProperty("UserDomainName")]
        public string UserDomainName { get; set; }
    }

    /// <summary>
    /// Расширения для HttpClient.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Отправляет POST запрос с данными в формате JSON.
        /// </summary>
        /// <typeparam name="T">Тип данных для отправки.</typeparam>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="requestUri">URI запроса.</param>
        /// <param name="data">Данные для отправки.</param>
        /// <returns>Task с ответом сервера.</returns>
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync(requestUri, content);
        }
    }
}