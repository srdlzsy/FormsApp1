using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FormsApp1.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }



    }
}
