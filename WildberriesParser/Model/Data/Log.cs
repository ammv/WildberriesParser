//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WildberriesParser.Model.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Log
    {
        public int ID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> Type { get; set; }
        public string Description { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual LogType LogType { get; set; }
        public virtual User User { get; set; }
    }
}
