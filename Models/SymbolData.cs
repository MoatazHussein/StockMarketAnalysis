using System.ComponentModel.DataAnnotations;

namespace StockMarket.Models
{
    public class SymbolData
    {
        public string? symbol { get; set; }
        public string? name { get; set; }
        public double? price { get; set; }
        public double? changesPercentage { get; set; }
        public double? change { get; set; }
        public double? dayLow { get; set; }
        public double? dayHigh { get; set; }
        public double? yearHigh { get; set; }
        public double? yearLow { get; set; }
        public double? marketCap { get; set; }
        public double? priceAvg50 { get; set; }
        public double? priceAvg200 { get; set; }
        public string? exchange { get; set; }
        public double? volume { get; set; }
        public double? avgVolume { get; set; }
        public double? open { get; set; }
        public double? previousClose { get; set; }
        public double? eps { get; set; }
        public double? pe { get; set; }
        public string? earningsAnnouncement { get; set; }
        public double? sharesOutstanding { get; set; }
        public double? timestamp { get; set; }
    }
}
