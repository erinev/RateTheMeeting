﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RateTheMeeting.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class RateTheMeetingEntities : DbContext
    {
        public RateTheMeetingEntities()
            : base("name=RateTheMeetingEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Evaluations> Evaluations { get; set; }
        public DbSet<Meeting_attenders> Meeting_attenders { get; set; }
        public DbSet<Meeting_questions> Meeting_questions { get; set; }
        public DbSet<Meetings> Meetings { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<sysdiagrams> sysdiagrams { get; set; }
        public DbSet<Users> Users { get; set; }
    }
}