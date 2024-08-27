using Microsoft.OpenApi.Any;
using System.ComponentModel.DataAnnotations;

namespace StockMarket.Models
{
    public class SymbolAnalysisData
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public int? Buy { get; set; }
        public int? Neutral { get; set; }
        public int? Sell { get; set; }
        public string? Signal { get; set; }
        public double? Adx { get; set; }
        public bool? Trending { get; set; }

    }

}
