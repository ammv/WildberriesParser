using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleWbApi
{
    public class WbCard
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

        [Category("NotImportant")]
        [Description("ID бренда")]
        public int brandId { get; set; }

        [Description("ID сайта бренда")]
        [Category("NotImportant")]
        public int siteBrandId { get; set; }

        [Description("ID поставщика")]
        [Category("NotImportant")]
        public int supplierId { get; set; }

        [Description("Скидка %")]
        [Category("Important")]
        public int sale { get; set; }

        private double _priceU;

        [Category("Important")]
        [Description("Цена без скидки")]
        public double priceU
        {
            get => _priceU;
            set
            {
                _priceU = value / 10;
            }
        }

        private double _salePriceU;

        [Category("Important")]
        [Description("Цена со скидкой")]
        public double salePriceU
        {
            get => _salePriceU;
            set
            {
                _salePriceU = value / 10;
            }
        }

        [Category("Important")]
        [Description("Логистическая стоимость")]
        public double logisticsCost { get; set; }

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

        private int? _quantity = -1;

        public int? Quantity
        {
            get
            {
                if (_quantity == -1)
                {
                    int quantity = 0;
                    foreach (var size in sizes)
                    {
                        if (size.stocks != null)
                        {
                            foreach (var stock in size.stocks)
                            {
                                quantity += stock.qty;
                            }
                        }
                    }
                    _quantity = quantity;
                }
                return _quantity;
            }
        }
    }
}