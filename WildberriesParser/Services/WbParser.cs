using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WildberriesParser.Services
{
    public class WbParser
    {
        public WildberriesSearchResponse ParseResponse(string responseJson)
        {
            return JsonConvert.DeserializeObject<WildberriesSearchResponse>(responseJson);
        }
    }
}