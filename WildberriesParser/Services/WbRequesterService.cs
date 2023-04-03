using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WildberriesParser.Services
{
    public class WbRequesterService : IWbRequesterService
    {
        private string _GenerateUserAgent()
        {
            return @"Mozilla/5.0 (Linux; Android 10; SM-A205F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.111 Mobile Safari/537.36";
        }

        public async Task<string> GetProductByArticleBasket(int article)
        {
            string url = $@"https://card.wb.ru/cards/detail?nm={article}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", _GenerateUserAgent());

            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Ошибка! Ответ от сервера: " + response.StatusCode);
            }
        }

        public async Task<string> GetProductByArticleSite(int article)
        {
            string url = $@"https://card.wb.ru/cards/detail?curr=rub&dest=-1257786&regions=80,64,38,4,115,83,33,68,70,69,30,86,75,40,1,66,48,110,22,31,71,114,111&spp=0&nm={article}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", _GenerateUserAgent());

            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Ошибка! Ответ от сервера: " + response.StatusCode);
            }
        }
    }
}