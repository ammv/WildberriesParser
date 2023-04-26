using Newtonsoft.Json;
using SimpleWbApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleWbApi
{
    public class WbAPI
    {
        private HttpClient _client;
        private Dictionary<string, string> _urls;

        public WbAPI(string userAgent = @"Mozilla/5.0 (Linux; Android 10; SM-A205F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.111 Mobile Safari/537.36")
        {
            _LoadUrl();
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Accept", "*/*");
            _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        private void _LoadUrl()
        {
            _urls = new Dictionary<string, string>();
            _urls.Add("CardBasketArticle", @"https://card.wb.ru/cards/detail?nm={article}");
            _urls.Add("CardSiteArticle", @"https://card.wb.ru/cards/detail?curr=rub&dest=-1257786&regions=80,64,38,4,115,83,33,68,70,69,30,86,75,40,1,66,48,110,22,31,71,114,111&spp=0&nm={article}");
            _urls.Add("CardsSiteArticle", @"https://card.wb.ru/cards/detail?curr=rub&dest=-1257786&regions=80,64,38,4,115,83,33,68,70,69,30,86,75,40,1,66,48,110,22,31,71,114,111&spp=0&nm={articles}");
            _urls.Add("CardsSiteSearch", @"https://search.wb.ru/exactmatch/ru/common/v4/search?appType=1&curr=rub&dest=-1257786&page={PAGE}&query={QUERY}&regions=80,64,38,4,115,83,33,68,70,69,30,86,75,40,1,66,48,110,31,22,71,114&resultset=catalog&sort=popular&spp=0&suppressSpellcheck=false");
        }

        /// <summary>
        /// Парсит JSON ответ от переданного url в экземпляр класса WbResponse
        /// </summary>
        /// <param name="url">запрос</param>
        /// <returns></returns>
        private async Task<WbResponse> _ParseResponse(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WbResponse>(content);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получает карточку товара с сервера Basket, без указания остатков
        /// </summary>
        /// <param name="article"></param>
        /// <returns>Артикул товара</returns>
        public async Task<WbResponse> GetCardFromBasketByArticle(int article)
        {
            return await _ParseResponse(_urls["CardBasketArticle"]
                .Replace("{article}", article.ToString()));
        }

        /// <summary>
        /// Получает карточку товара напрямую с сайта Wildberries, с указанием остатков
        /// </summary>
        /// <param name="article"></param>
        /// <returns>Артикул товара</returns>
        public async Task<WbResponse> GetCardFromSiteByArticle(int article)
        {
            return await _ParseResponse(_urls["CardSiteArticle"]
                .Replace("{article}", article.ToString()));
        }

        /// <summary>
        /// Получает карточки товаров по их артикулам с сайта Wildberries, с указанием остатков
        /// </summary>
        /// <param name="articles"></param>
        /// <returns></returns>
        public async Task<WbResponse> GetCardsByArticleFromSite(List<int> articles)
        {
            return await _ParseResponse(_urls["CardsSiteArticle"]
                .Replace("{articles}", string.Join(";", articles)));
        }

        /// <summary>
        /// Получает карточки из поисковой выдачи на сайте Wildberries
        /// </summary>
        /// <param name="search">Поисковой запрос</param>
        /// <param name="pages">Количество страниц</param>
        /// <returns></returns>
        public async Task<List<WbResponse>> GetCardsFromSiteBySearch(string search, int pages = 1)
        {
            string url = _urls["CardsSiteSearch"].Replace("{QUERY}", HttpUtility.UrlEncode(search).ToUpper());

            List<WbResponse> wbResponses = new List<WbResponse>();

            WbResponse response;

            for (int i = 1; i <= pages; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    response = await _ParseResponse(url.Replace("{PAGE}", i.ToString()));
                    if (response != null)
                    {
                        wbResponses.Add(response);
                        break;
                    }
                }
            }

            return wbResponses;
        }
    }
}