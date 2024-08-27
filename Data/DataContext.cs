using Microsoft.EntityFrameworkCore;
using StockMarket.Models;

namespace StockMarket.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }


        public DbSet<User> Users { get; set; }
        public DbSet<SymbolDataSection> SymbolDataSections { get; set; }
        public DbSet<FavouriteSymbol> FavoriteSymbols { get; set; }
        public DbSet<SymbolAnalysisData> SymbolAnalysisData { get; set; }

    }
}
