namespace StockMarket.Models
{
    public class SymbolDataSection
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Sector { get; set; }
        public double? Price { get; set; }
        public double? ChangesPercentage { get; set; }
        public double? Change { get; set; }
        public double? DayLow { get; set; }
        public double? DayHigh { get; set; }
        public double? YearHigh { get; set; }
        public double? YearLow { get; set; }
        public double? MarketCap { get; set; }
        public double? PriceAvg50 { get; set; }
        public double? PriceAvg200 { get; set; }
        public string? Exchange { get; set; }
        public double? Volume { get; set; }
        public double? AvgVolume { get; set; }
        public double? Open { get; set; }
        public double? PreviousClose { get; set; }
        public double? Eps { get; set; }
        public double? Pe { get; set; }
        public string? EarningsAnnouncement { get; set; }
        public double? SharesOutstanding { get; set; }
        public double? Timestamp { get; set; }
    }
}
