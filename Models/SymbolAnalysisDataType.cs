using Microsoft.OpenApi.Any;
using System.ComponentModel.DataAnnotations;

namespace StockMarket.Models
{
    public class SymbolAnalysisDataType
    {
        public TechnicalAnalysis? technicalAnalysis { get; set; }
        public Trend? trend { get; set; }
    }

    public class TechnicalAnalysis
    {
        public Count? count { get; set; }
        public string? signal { get; set; }
    }

    public class Count
    {
        public int? buy { get; set; }
        public int? neutral { get; set; }
        public int? sell { get; set; }

    }

    public class Trend
    {
        public double? adx  { get; set; }
        public bool? trending { get; set; }
    }

}
