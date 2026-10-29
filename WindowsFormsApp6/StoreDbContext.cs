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
            Database.SetInitializer<StoreDbContext>(null);

            string sqlCategorii = "CREATE TABLE IF NOT EXISTS Categorii (" +
                         "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                         "Denumire TEXT NOT NULL)";
            this.Database.ExecuteSqlCommand(sqlCategorii);

            string sqlProducts = "CREATE TABLE IF NOT EXISTS Products (" +
                         "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                         "Denumire TEXT NOT NULL, " +
                         "Descriere TEXT, " +
                         "IdCategorie INTEGER NOT NULL, " +
                         "DataIntrarii DATETIME NOT NULL, " +
                         "DataExpirarii DATETIME NOT NULL, " +
                         "Cantitate INTEGER NOT NULL)";
            this.Database.ExecuteSqlCommand(sqlProducts);

            string sqlIstoric = "CREATE TABLE IF NOT EXISTS IstoricVanzari (" +
                         "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                         "IdProdus INTEGER NOT NULL, " +
                         "Cantitate INTEGER NOT NULL, " +
                         "DenumireProdus TEXT, " +
                         "IdCategorie INTEGER NOT NULL, " +
                         "DataVanzarii DATETIME NOT NULL)";
            this.Database.ExecuteSqlCommand(sqlIstoric);
        }
    }
}