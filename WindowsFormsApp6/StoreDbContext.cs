using System.Data.Entity;

namespace WindowsFormsApp6
{
    public class StoreDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<CategorieProdus> Categorii { get; set; }
        public DbSet<IstoricVanzari> Istoric { get; set; }

        public StoreDbContext() : base("name=StoreDb")
        {
        }
    }
}