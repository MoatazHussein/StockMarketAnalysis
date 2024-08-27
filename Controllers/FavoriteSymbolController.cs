using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMarket.Data;
using StockMarket.Models;

namespace StockMarket.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteSymbolController : ControllerBase
    {

        private readonly DataContext _context;


        public FavoriteSymbolController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("AddSymbol")]
        public async Task<IActionResult> AddSymbol(string UserEmail, string SymbolName)
        {
            var _emailExist = _context.Users.Any(u => u.Email == UserEmail);
            var _emailVerified = _context.Users.Any(u => u.Email == UserEmail && u.VerifiedAt != null);
            var _symbolExist = _context.SymbolDataSections.Any(u => u.Symbol == SymbolName);

            if (!_emailExist)
            {
                return BadRequest(new { msg = "User Not found." });
            }

            if (!_emailVerified)
            {
                return BadRequest(new { msg = "User Not Verified." });
            }

            if (!_symbolExist)
            {
                return BadRequest(new { msg = "Symbol Not found." });
            }

            var _symbolExistInFavourite = _context.FavoriteSymbols.Any(e => e.UserEmail == UserEmail && e.SymbolName == SymbolName);

            if (_symbolExistInFavourite)
            {
                return BadRequest(new { msg = $"{SymbolName} is already exist in favourite for this user" });
            }


            var _UsersData = await _context.Users.ToListAsync();
            var _userID = _UsersData.Where(e => e.Email == UserEmail).ToList()[0].Id;

            var _symbolsData = await _context.SymbolDataSections.ToListAsync();
            var _symbolID = _symbolsData.Where(e => e.Symbol == SymbolName).ToList()[0].Id;

            var FavouriteSymbol = new FavouriteSymbol
            {
                UserID = _userID,
                UserEmail = UserEmail,
                SymbolID = _symbolID,
                SymbolName = SymbolName,
            };

            _context.FavoriteSymbols.Add(FavouriteSymbol);
            await _context.SaveChangesAsync();
            //System.Diagnostics.Debug.WriteLine("TEST");

            return Ok(new { msg = "The Favourite Symbol has been successfully Added" });
        }

        [HttpGet("GetSymbols")]
        public async Task<IActionResult> GetSymbols(string UserEmail)
        {
            var _emailExist = _context.Users.Any(u => u.Email == UserEmail);

            if (!_emailExist)
            {
                return BadRequest(new { msg = "User Not found." });
            }

            var _emailhasFavourite = _context.FavoriteSymbols.Any(u => u.UserEmail == UserEmail);

            if (!_emailhasFavourite)
            {
                return BadRequest(new { msg = "This User has no favourite symbols" });
            }

            var _symbolsData = await _context.FavoriteSymbols.ToListAsync();
            var _symbolsList = _symbolsData.Where(e => e.UserEmail == UserEmail).ToList();

            return Ok(_symbolsList);
        }

        [HttpPost("deleteSymbol")]
        public async Task<IActionResult> deleteSymbol(string UserEmail, string SymbolName)
        {
            var _emailExist = _context.Users.Any(u => u.Email == UserEmail);
            var _symbolExist = _context.SymbolDataSections.Any(u => u.Symbol == SymbolName);

            if (!_emailExist)
            {
                return BadRequest(new { msg = "User Not found." });
            }

            if (!_symbolExist)
            {
                return BadRequest(new { msg = "Symbol Not found." });
            }

            var _emailhasFavourite = _context.FavoriteSymbols.Any(u => u.UserEmail == UserEmail);

            if (!_emailhasFavourite)
            {
                return BadRequest(new { msg = "This User has no favourite symbols" });
            }

            var _symbolExistInFavourite = _context.FavoriteSymbols.Any(e => e.UserEmail == UserEmail && e.SymbolName == SymbolName);

            if (!_symbolExistInFavourite)
            {
                return BadRequest(new { msg = $"{SymbolName} is not existed in favourite list for this user" });
            }

            var _symbolsData = await _context.FavoriteSymbols.ToListAsync();
            var _undesiredSymbol = _symbolsData.Where(e => e.UserEmail == UserEmail && e.SymbolName == SymbolName).ToList()[0];

            _context.FavoriteSymbols.Remove(_undesiredSymbol);
            await _context.SaveChangesAsync();

            return Ok(new { msg = $"{SymbolName} has been removed from favourite list" });

        }
    }
}
