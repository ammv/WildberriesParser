using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WildberriesParser
{
    /*
     */

    internal class WbParser
    {
        /// <summary>
        /// Получить каталоги и подкатологи
        /// </summary>
        ///
        private string _GenerateUserAgent()
        {
            return @"Mozilla/5.0 (Linux; Android 10; SM-A205F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.111 Mobile Safari/537.36";
            //return ProviderFakeUserAgent.Random;
        }

        public async Task<string> GetCatalogiesAsJsonStringAsync()
        {
            string url = "https://www.wildberries.ru/webapi/menu/main-menu-ru-ru.json";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", _GenerateUserAgent());

            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string s = await response.Content.ReadAsStringAsync();
                return s;
            }
            else
            {
                throw new Exception("Ошибка! Ответ от сервера: " + response.StatusCode);
            }
        }

        public async Task<List<string>> GetProductCardsBySearch(string search, int pages = 1)
        {
            string url = @"https://search.wb.ru/exactmatch/ru/common/v4/search?appType=1&couponsGeo=12,3,18,15,21&curr=rub&dest=-1257786&emp=0&lang=ru&locale=ru&page=PAGE&pricemarginCoeff=1.0&query=QUERY&reg=0&regions=80,64,38,4,115,83,33,68,70,69,30,86,75,40,1,66,31,48,110,22,71&resultset=catalog&sort=popular&spp=0&suppressSpellcheck=false";

            url = url.Replace("QUERY", HttpUtility.UrlEncode(search).ToUpper());

            List<string> data = new List<string>();

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", _GenerateUserAgent());

            HttpResponseMessage response;
            bool success;

            for (int i = 1; i <= pages; i++)
            {
                success = false;
                for (int j = 0; j < 5; j++)
                {
                    response = await client.GetAsync(url.Replace("PAGE", i.ToString()));
                    //Console.WriteLine(url.R);
                    string s = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        data.Add(s);
                        success = true;
                        break;
                    }
                    else
                    {
                        throw new Exception($"Ошибка! {s}");
                    }
                }
                if (!success)
                {
                    throw new Exception("Ошибка! Не удалось совершить запрос!");
                }
            }

            return data;
        }
    }
}