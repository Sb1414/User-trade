using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using KalkamanovaFinal.Models;

namespace KalkamanovaFinal.Data
{
    public class DataContext : DbContext
    {
    
        public DataContext() : base("name=DataContext")
        {
        }
    
        public System.Data.Entity.DbSet<User> Users { get; set; }
        public System.Data.Entity.DbSet<Models.Data> Data { get; set; }
    }
}
