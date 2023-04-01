using System.Collections.Generic;
using WildberriesParser.Model;

namespace WildberriesParser
{
    internal class WbSize
    {
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public int Rank { get; set; }
        public int wh { get; set; }
        public List<WbStock> stocks { get; set; }
        public string sign { get; set; }
    }
}