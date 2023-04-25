using System.Collections.Generic;

namespace SimpleWbApi.Model
{
    public class WbSize
    {
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public int Rank { get; set; }
        public int wh { get; set; }
        public List<WbStock> stocks { get; set; }
        public string sign { get; set; }
    }
}