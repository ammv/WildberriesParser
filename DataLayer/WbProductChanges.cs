//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class WbProductChanges
    {
        public int ID { get; set; }
        public Nullable<int> WbProductID { get; set; }
        public System.DateTime Date { get; set; }
        public int Discount { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public decimal PriceWithoutDiscount { get; set; }
        public int Quantity { get; set; }
    
        public virtual WbProduct WbProduct { get; set; }
    }
}