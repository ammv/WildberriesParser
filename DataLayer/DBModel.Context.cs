﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DBEntities : DbContext
    {
        public DBEntities()
            : base("name=DBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CollectorTask> CollectorTask { get; set; }
        public virtual DbSet<CollectorTaskStatus> CollectorTaskStatus { get; set; }
        public virtual DbSet<CollectorTaskType> CollectorTaskType { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<LogType> LogType { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<SearchPatternType> SearchPatternType { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<WbBrand> WbBrand { get; set; }
        public virtual DbSet<WbProduct> WbProduct { get; set; }
        public virtual DbSet<WbProductChanges> WbProductChanges { get; set; }
        public virtual DbSet<WbProductPosChanges> WbProductPosChanges { get; set; }
    }
}
