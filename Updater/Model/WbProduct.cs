using System.Collections.Generic;

namespace WildberriesParser
{
    internal class WbProduct
    {
        public int __sort { get; set; }
        public int ksort { get; set; }
        public int time1 { get; set; }
        public int time2 { get; set; }
        public int dist { get; set; }
        public int id { get; set; }
        public int root { get; set; }
        public int kindId { get; set; }
        public int SubjectId { get; set; }
        public int SubjectParentId { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public int brandId { get; set; }
        public int siteBrandId { get; set; }
        public int supplierId { get; set; }
        public int sale { get; set; }
        public int priceU { get; set; }
        public int salePriceU { get; set; }
        public int logisticsCost { get; set; }
        public int saleConditions { get; set; }
        public int rating { get; set; }
        public int feedbacks { get; set; }
        public int panelPromoId { get; set; }
        public string promoTextCat { get; set; }
        public int volume { get; set; }
        public List<WbClothesColor> colors { get; set; }
        public List<WbSize> sizes { get; set; }
        public bool diffPrice { get; set; }
    }
}