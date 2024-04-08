using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using KalkamanovaFinalWeb.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace KalkamanovaFinalWeb.Controllers
{
    public class SharpController : Controller
    {
        /// <summary>
        /// HTTP клиент для взаимодействия с сервером.
        /// </summary>
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Токен доступа, используемый для аутентификации пользователя.
        /// </summary>
        private static string _accessToken;
        
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var data = new
                {
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.Password,
                    UserDomainName = model.UserDomainName
                };

                try
                {
                    var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/Register", data);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Регистрация не удалась.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", ex.InnerException.Message);
                    }
                }
            }

            return View(model);
        }

        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = new
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                Console.WriteLine("Email или пароль пусты.");
                ModelState.AddModelError("", "\"Email или пароль пусты.\"");
            }
            
            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/Login", data);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                    _accessToken = tokenResponse.AccessToken;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Вход не удался.");

                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "Возникла ошибка при отправке запроса:");
                ModelState.AddModelError("", ex.Message);

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", "Inner exception:");
                    ModelState.AddModelError("", ex.InnerException.Message);
                }
            }

            return View(model);
        }

        public async Task<ActionResult> CreateTrade(Trade trade)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                ModelState.AddModelError("", "Пожалуйста, сначала выполните вход.");
                return View(trade);
            }
            
            if (ModelState.IsValid)
            {
                trade.CreatedAt = DateTime.Now;
                
                var data = new Trade
                {
                    CreatedAt = trade.CreatedAt,
                    Amount = trade.Amount
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/CreateTrade", data);
                if (response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Сделка успешно создана.");
                }
                else
                {
                    ModelState.AddModelError("", "Не удалось создать сделку.");
                }
            }
            return View(trade);
        }

        public async Task<ActionResult> GetLatestTrade()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                ModelState.AddModelError("", "Пожалуйста, сначала выполните вход.");
                return View();
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await client.GetAsync("http://localhost:62130/api/Account/GetLatestTrade");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                TradeResult latestTrade = JsonConvert.DeserializeObject<TradeResult>(json);
                return View(latestTrade);
            }
            else
            {
                ModelState.AddModelError("", "Не удалось получить последнюю сделку.");
            }
            return View();
        }

        public async Task<ActionResult> LogOff()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                ModelState.AddModelError("", "Пожалуйста, сначала выполните вход.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await client.PostAsJsonAsync("http://localhost:62130/api/Account/CreateTrade", new {});
            if (response.IsSuccessStatusCode)
            {
                _accessToken = null;
                ModelState.AddModelError("", "Вы успешно вышли из системы.");

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Не удалось выйти из системы.");
            }
            
            return View();
        }
        
        public class TokenResponse
        {
            /// <summary>
            /// Токен доступа.
            /// </summary>
            [JsonProperty("AccessToken")]
            public string AccessToken { get; set; }
        }
    }
}