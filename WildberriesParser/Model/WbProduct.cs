using System.Collections.Generic;
using System.ComponentModel;

namespace WildberriesParser
{
    public class WbProduct
    {
        [Category("Unknown")]
        [Description("Неизвестно")]
        public int __sort { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int ksort { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int time1 { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int time2 { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int dist { get; set; }

        [Category("Important")]
        [Description("Артикул")]
        public int id { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int root { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int kindId { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int SubjectId { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int SubjectParentId { get; set; }

        [Category("Important")]
        [Description("Название товара")]
        public string name { get; set; }

        [Category("Important")]
        [Description("Название бренда")]
        public string brand { get; set; }

        [Category("Important")]
        [Description("ID бренда")]
        public int brandId { get; set; }

        [Description("ID сайта бренда")]
        [Category("Important")]
        public int siteBrandId { get; set; }

        [Description("ID поставщика")]
        [Category("Important")]
        public int supplierId { get; set; }

        [Description("Скидка %")]
        [Category("Important")]
        public int sale { get; set; }

        [Category("Important")]
        [Description("Цена без скидки")]
        public int priceU { get; set; }

        [Category("Important")]
        [Description("Цена со скидкой")]
        public int salePriceU { get; set; }

        [Category("Important")]
        [Description("Логистическая стоимость")]
        public int logisticsCost { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int saleConditions { get; set; }

        [Category("Important")]
        [Description("Рейтинг")]
        public int rating { get; set; }

        [Category("Important")]
        [Description("Количество отзывов")]
        public int feedbacks { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int panelPromoId { get; set; }

        [Category("Important")]
        [Description("Промо текст")]
        public string promoTextCat { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public int volume { get; set; }

        [Category("Important")]
        [Description("Варианты цветов")]
        public List<WbClothesColor> colors { get; set; }

        [Category("Important")]
        [Description("Размеры")]
        public List<WbSize> sizes { get; set; }

        [Category("Unknown")]
        [Description("Неизвестно")]
        public bool diffPrice { get; set; }
    }
}