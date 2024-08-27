namespace StockMarket.Models
{
    public class FavouriteSymbol
    {
        public int Id { get; set; }
        public int? UserID { get; set; }
        public string? UserEmail { get; set; }
        public int? SymbolID { get; set; }
        public string? SymbolName { get; set; }
    }
}
