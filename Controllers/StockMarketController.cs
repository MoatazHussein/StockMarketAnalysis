using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMarket.Attributes;
using StockMarket.Data;
using StockMarket.Models;
using StockMarket.Services.PeriodicTaskHosted;
using System.Security.Policy;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace Stock_Market.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StockMarketController : ControllerBase
    {
        private string financial_api_key = Environment.GetEnvironmentVariable("financial_api_key") ?? "";
        private string finHub_api_key = Environment.GetEnvironmentVariable("finHub_api_key") ?? "";
        private readonly DataContext _context;
        private readonly ILogger<StockMarketController> _logger;


        public StockMarketController(DataContext context,ILogger<StockMarketController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Templates
        private async Task<IActionResult> MakeApiRequest(string baseUrl, Dictionary<string, string> queryParams)
        {
            using (HttpClient client = new HttpClient())
            {

                // Construct the query string
                var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

                // Combine the base URL and query string
                string urlWithQuery = $"{baseUrl}?{queryString}";

                try
                {
                    // Make the GET request
                    HttpResponseMessage response = await client.GetAsync(urlWithQuery);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();

                    return Ok(responseData);
                   
                }
                catch (HttpRequestException e)
                {
                    return BadRequest(e.Message);
                }
            }
        }

        #endregion Templates 

        #region SectionsData
        private async Task<List<SymbolData>> GetSymbolsDataFromAPI()
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/symbol/SAU";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "apikey", financial_api_key }

            };

            using (HttpClient client = new HttpClient())
            {

                // Construct the query string
                var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

                // Combine the base URL and query string
                string urlWithQuery = $"{baseUrl}?{queryString}";

                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(urlWithQuery);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();

                // Deserialize the response into an object
                List<SymbolData> SymbolsDataList = JsonSerializer.Deserialize<List<SymbolData>>(responseData);

                return SymbolsDataList;
            }
        }
        private async Task<IActionResult> InsertSymbolsData()
        {
            List<SymbolData> SymbolsDataList = await GetSymbolsDataFromAPI();

            var OneSymbol = SymbolsDataList[0];

            foreach (var item in SymbolsDataList)
            {

                var symbolDataSection = new SymbolDataSection
                {
                    Symbol = item.symbol,
                    Name = item.name,
                    Price = item.price,
                    ChangesPercentage = item.changesPercentage,
                    Change = item.change,
                    DayLow = item.dayLow,
                    DayHigh = item.dayHigh,
                    YearHigh = item.yearHigh,
                    YearLow = item.yearLow,
                    MarketCap = item.marketCap,
                    PriceAvg50 = item.priceAvg50,
                    PriceAvg200 = item.priceAvg200,
                    Exchange = item.exchange,
                    Volume = item.volume,
                    AvgVolume = item.avgVolume,
                    Open = item.open,
                    PreviousClose = item.previousClose,
                    Eps = item.eps,
                    Pe = item.pe,
                    EarningsAnnouncement = item.earningsAnnouncement,
                    SharesOutstanding = item.sharesOutstanding,
                    Timestamp = item.timestamp,
                };

                _context.SymbolDataSections.Add(symbolDataSection);
                await _context.SaveChangesAsync();

            }

            return Ok("All Symbols Data has been inserted");
        }
        private async Task<string> GetSymbolSectionFromAPI(string symbol)
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v4/company-outlook";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "symbol", symbol },
                { "apikey", financial_api_key },

            };

            using (HttpClient client = new HttpClient())
            {

                // Construct the query string
                var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

                // Combine the base URL and query string
                string urlWithQuery = $"{baseUrl}?{queryString}";

                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(urlWithQuery);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();

               // Deserialize the JSON string to a .NET object
                var dynamicResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData);

                var sector = dynamicResponse.profile.sector;

                return sector;
            }

        }
        private async Task<IActionResult> updateAllSection()
        {
            var symbols = await _context.SymbolDataSections.ToListAsync();

            var updatedEntities = new List<SymbolDataSection>();
            var errors = new List<string>();

            foreach (var symbolItem in symbols)
            {
                var symbolData = await _context.SymbolDataSections.FirstOrDefaultAsync(u => u.Symbol == symbolItem.Symbol);
                if (symbolData != null)
                {
                    var section = await GetSymbolSectionFromAPI(symbolItem.Symbol);
                    symbolData.Sector = section;

                    try
                    {
                        var result = await _context.SaveChangesAsync();
                        //Thread.Sleep(300);
                        if (result > 0)
                        {
                            updatedEntities.Add(symbolData);
                        } 
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"An unexpected error occurred while updating entity with ID: {symbolData.Id}. Error: {ex.Message}");
                    }
                }
            }

                if (errors.Count == 0)
                {
                    return Ok(new { message = $"{updatedEntities.Count} records updated successfully." });
                }
                else
                {
                    return StatusCode(500, new { message = $"{updatedEntities.Count} records updated successfully with errors.", errors });
                }
        }

        [HttpGet("GetAllSymbolData")]
        [TokenProtected]  
        //[Authorize]
        public async Task<ActionResult<List<SymbolDataSection>>> GetAllSymbolData()
        {

            var symbolData = await _context.SymbolDataSections.ToListAsync();

            return Ok(symbolData);
        }

        [HttpGet("UpdateSymbolsData")]
        //[TokenProtected]
        public async Task<IActionResult> UpdateSymbolsData()
        {

            List<SymbolData> SymbolsDataList = await GetSymbolsDataFromAPI();
            var updatedEntities = new List<SymbolDataSection>();
            var errors = new List<string>();

            foreach (var item in SymbolsDataList)
            {
                var symbolData = await _context.SymbolDataSections.ToListAsync();

                var SymbolExists = _context.SymbolDataSections.Any(e => e.Symbol == item.symbol);

                if (SymbolExists) {

                    var requiredsymbol = symbolData.Where(e => e.Symbol == item.symbol).ToList()[0];

                    requiredsymbol.Price = item.price;
                    requiredsymbol.ChangesPercentage = item.changesPercentage;
                    requiredsymbol.Change = item.change;
                    requiredsymbol.DayLow = item.dayLow;
                    requiredsymbol.DayHigh = item.dayHigh;
                    requiredsymbol.YearHigh = item.yearHigh;
                    requiredsymbol.YearLow = item.yearLow;
                    requiredsymbol.MarketCap = item.marketCap;
                    requiredsymbol.PriceAvg50 = item.priceAvg50;
                    requiredsymbol.PriceAvg200 = item.priceAvg200;
                    requiredsymbol.Exchange = item.exchange;
                    requiredsymbol.Volume = item.volume;
                    requiredsymbol.AvgVolume = item.avgVolume;
                    requiredsymbol.Open = item.open;
                    requiredsymbol.PreviousClose = item.previousClose;
                    requiredsymbol.Eps = item.eps;
                    requiredsymbol.Pe = item.pe;
                    requiredsymbol.EarningsAnnouncement = item.earningsAnnouncement;
                    requiredsymbol.SharesOutstanding = item.sharesOutstanding;
                    requiredsymbol.Timestamp = item.timestamp;

                    try
                    {
                        var result = await _context.SaveChangesAsync();
                        //Thread.Sleep(300);
                        if (result > 0)
                        {
                            updatedEntities.Add(requiredsymbol);
                        }
                        //else
                        //{
                        //    errors.Add($"No changes detected for entity with ID: {requiredsymbol.Id}.");
                        //}
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"An unexpected error occurred while updating entity with ID: {requiredsymbol.Id}. Error: {ex.Message}");
                    }
                }
            }


            if (errors.Count == 0)
            {
                _logger.LogInformation("UpdateSymbolsData has been done successfully");
                return Ok(new { message = $"{updatedEntities.Count} changes detected, all records updated successfully." });
            }
            else
            {
                _logger.LogInformation($"UpdateSymbolsData has been finished,{updatedEntities.Count} records updated successfully with {errors.Count}error");
                foreach (var error in errors)
                {
                    _logger.LogInformation(error);
                }
                return StatusCode(207, new { message = $"{updatedEntities.Count} records updated successfully with errors.", errors });
            }
        }

        #endregion SectionsData
        

        #region TechnicalAnalysis

        private async Task<SymbolAnalysisDataType> GetGymbolTechnicalAnalysisFromAPI(string symbol)
        {
            string baseUrl = $"https://finnhub.io/api/v1/scan/technical-indicator";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "symbol", symbol },
                { "resolution", "D" },
                { "token", finHub_api_key }
            };

            using (HttpClient client = new HttpClient())
            {

                // Construct the query string
                var queryString = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;

                // Combine the base URL and query string
                string urlWithQuery = $"{baseUrl}?{queryString}";

                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(urlWithQuery);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();

                // Deserialize the response into an object
                SymbolAnalysisDataType SymbolAnalysisDataObj = JsonSerializer.Deserialize<SymbolAnalysisDataType>(responseData);

                return SymbolAnalysisDataObj;
            }
        }
        private async Task<IActionResult> InsertSymbolTechnicalAnalysis(string symbol,string Name)
        {
            SymbolAnalysisDataType SymbolAnalysisDataObj = await GetGymbolTechnicalAnalysisFromAPI(symbol);

            if (SymbolAnalysisDataObj != null) { 
            var buy = SymbolAnalysisDataObj.technicalAnalysis?.count?.buy;
            var neutral = SymbolAnalysisDataObj.technicalAnalysis?.count?.neutral;
            var sell = SymbolAnalysisDataObj.technicalAnalysis?.count?.sell;
            var signal = SymbolAnalysisDataObj.technicalAnalysis?.signal;
            var adx = SymbolAnalysisDataObj.trend?.adx;
            var trending = SymbolAnalysisDataObj.trend?.trending;

                var SymbolAnalysisData = new SymbolAnalysisData
                {
                    Symbol = symbol,
                    Name = Name,
                    Buy = buy,
                    Neutral = neutral,
                    Sell = sell,
                    Signal = signal,
                    Adx = adx,
                    Trending = trending,

                };
                _context.SymbolAnalysisData.Add(SymbolAnalysisData);
                await _context.SaveChangesAsync();

        }
            
            return Ok("inserted");
        }

        //[HttpGet("InsertAllSymbolsTechnicalAnalysis")]
        private async Task<IActionResult> InsertAllSymbolsTechnicalAnalysis()
        {
            var symbolData = await _context.SymbolDataSections.ToListAsync();

            foreach (var item in symbolData)
            {
                if (item != null)
                {
                    string symbol = item.Symbol;
                    string Name = item.Name;
                    await InsertSymbolTechnicalAnalysis(symbol, Name);
                }
            }

            return Ok("All TechnicalAnalysis of the symbols are inserted");
        }

        private async Task<IActionResult> updateSymbolTechnicalAnalysis(string symbol)
        {
            var updatedEntities = new List<SymbolAnalysisData>();
            var errors = new List<string>();
            var symbolData = await _context.SymbolAnalysisData.FirstOrDefaultAsync(u => u.Symbol == symbol);
            if (symbolData == null)
            {
                return BadRequest("Invalid token.");
            }
            var SymbolAnalysisDataObj = await GetGymbolTechnicalAnalysisFromAPI(symbol);

            symbolData.Buy = SymbolAnalysisDataObj.technicalAnalysis?.count?.buy;
            symbolData.Neutral = SymbolAnalysisDataObj.technicalAnalysis?.count?.neutral;
            symbolData.Sell = SymbolAnalysisDataObj.technicalAnalysis?.count?.sell;
            symbolData.Signal = SymbolAnalysisDataObj.technicalAnalysis?.signal;
            symbolData.Adx = SymbolAnalysisDataObj.trend?.adx;
            symbolData.Trending = SymbolAnalysisDataObj.trend?.trending;

            try
            {
                var result = await _context.SaveChangesAsync();
                //Thread.Sleep(300);
                if (result > 0)
                {
                    updatedEntities.Add(symbolData);
                }
                //else
                //{
                //    errors.Add($"No changes detected for entity with ID: {symbolData.Id}.");
                //}
            }
            catch (Exception ex)
            {
                errors.Add($"An unexpected error occurred while updating entity with ID: {symbolData.Id}. Error: {ex.Message}");
            }

            if (errors.Count == 0)
            {
                return Ok(new { message = $"{updatedEntities.Count} changes detected, all records updated successfully." });
            }
            else
            {
                return StatusCode(207, new { message = $"{updatedEntities.Count} records updated successfully with errors.", errors });
            }
        }

        [HttpGet("updateAllSymbolsTechnicalAnalysis")]
        public async Task<IActionResult> updateAllSymbolsTechnicalAnalysis()
        {
            var updatedEntities = new List<SymbolAnalysisData>();
            var errors = new List<string>();
            var symbols = await _context.SymbolAnalysisData.ToListAsync();

            foreach (var symbolItem in symbols)
            {
                var symbolData = await _context.SymbolAnalysisData.FirstOrDefaultAsync(u => u.Symbol == symbolItem.Symbol);
                {
                    try
                    {
                        var SymbolAnalysisDataObj = await GetGymbolTechnicalAnalysisFromAPI(symbolData.Symbol);
                        symbolData.Buy = SymbolAnalysisDataObj.technicalAnalysis?.count?.buy;
                        symbolData.Neutral = SymbolAnalysisDataObj.technicalAnalysis?.count?.neutral;
                        symbolData.Sell = SymbolAnalysisDataObj.technicalAnalysis?.count?.sell;
                        symbolData.Signal = SymbolAnalysisDataObj.technicalAnalysis?.signal;
                        symbolData.Adx = SymbolAnalysisDataObj.trend?.adx;
                        symbolData.Trending = SymbolAnalysisDataObj.trend?.trending;
                        var result = await _context.SaveChangesAsync();

                        if (result > 0)
                        {
                            updatedEntities.Add(symbolData);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"An unexpected error occurred while updating entity with ID: {symbolData?.Id}. Error: {ex.Message}");
                    }
                }
               
            }
            if (errors.Count == 0)
            {
                _logger.LogInformation("updateAllSymbolsTechnicalAnalysis has done successfully");
                return Ok(new { message = $"{updatedEntities.Count} changes detected, all records updated successfully." });
            }
            else
            {
                _logger.LogInformation($"updateAllSymbolsTechnicalAnalysis has finished with {errors.Count} error");

                foreach (var error in errors)
                {
                _logger.LogInformation(error);
                }

                return StatusCode(207, new { message = $"{updatedEntities.Count} records updated successfully with errors.", errors });
            }

        }

        [HttpGet("GetAllSymbolsTechnicalAnalysis")]
        [TokenProtected]
        //[Authorize]
        public async Task<ActionResult<List<SymbolDataSection>>> GetAllSymbolsTechnicalAnalysis()
        {

            var SymbolsTechnicalAnalysis = await _context.SymbolAnalysisData.ToListAsync();

            return Ok(SymbolsTechnicalAnalysis);
        }

        #endregion TechnicalAnalysis 

        #region financial api Calls


        [HttpGet("GetMainSymbolData")]
        public async Task<IActionResult> GetMainSymbolData()
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/symbol/SAU";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "apikey", financial_api_key }

            };

            return await MakeApiRequest(baseUrl, queryParams);
        }

        [HttpGet("GetQuoteSymbolData")]
        public async Task<IActionResult> GetQuoteSymbolData(string symbol)
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/quote/{symbol}";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "apikey", financial_api_key }

            };

            return await MakeApiRequest(baseUrl, queryParams);
        }


        [HttpGet("GetMainSymbolHistoricalData")]
        public async Task<IActionResult> GetMainSymbolHistoricalData()
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/historical-price-full/^TASI.SR";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "apikey", financial_api_key }

            };

            return await MakeApiRequest(baseUrl, queryParams);
        }

        [HttpGet("GetArticlesData")]
        public async Task<IActionResult> GetArticlesData(string page, string size)
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/fmp/articles";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "page", page },
                { "size", size },
                { "apikey", financial_api_key }
            };

            return await MakeApiRequest(baseUrl, queryParams);
        }

        [HttpGet("GetSymbolProfile")]
        public async Task<IActionResult> GetSymbolProfile(string symbol)
        {
            string baseUrl = $"https://financialmodelingprep.com/api/v3/profile/{symbol}";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "apikey", financial_api_key }
            };

            return await MakeApiRequest(baseUrl, queryParams);
        }
        #endregion financial api Calls

        #region finHub api Calls

        [HttpGet("GetSymbolNews")]
        public async Task<IActionResult> GetSymbolNews(string category)
        {
            string baseUrl = $"https://finnhub.io/api/v1/news";

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "category", category },
                { "token", finHub_api_key }
            };

            return await MakeApiRequest(baseUrl, queryParams);
        }

        #endregion finHub api Calls


    }
}
