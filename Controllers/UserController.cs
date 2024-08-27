using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMarket.Data;
using StockMarket.Models;
using StockMarket.Services;
using System.Security.Cryptography;

namespace StockMarket.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMailService _mailService;

        private readonly AuthService _authService;
        private readonly ILogger _logger;



        public UserController(DataContext context, IMailService mailService , AuthService authService, ILogger<UserController> logger
            )
        {
            _context = context;
            _mailService = mailService;
            _authService = authService;
            _logger = logger;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { msg = "User already exists." });
            }

            CreatePasswordHash(request.Password,
                 out byte[] passwordHash,
                 out byte[] passwordSalt);

            var user = new User
            {
                UserName = request.UserName,
                Mobile = request.Mobile,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //System.Diagnostics.Debug.WriteLine("TEST");

            await SendVerificationMail(user.Id);

            return Ok(new { msg = "User successfully created and Verification Mail has been send to the user" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest(new { msg = "User not found." });
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { msg = "Password is incorrect." });
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest(new { msg = "Not verified!" });
            }

            //var token = await _authService.GenerateGeneralJwtToken();
            var token = await _authService.GenerateUserJwtToken(request);

            return Ok(new { msg = $"Welcome back, {user.Email}! :)", Token = token });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid token." });
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();


            string? Verification_Successful_URL = Environment.GetEnvironmentVariable("Verification_Successful_URL")?.ToString();

            if (Verification_Successful_URL !=null)
            {
                return Redirect(Verification_Successful_URL+ "?token=" + token);
            }
            else
            {
                return Ok(new { msg = "User verified! :)" });
            }


        }
        private async Task SendVerificationMail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is not null)
            {
                var token = user.VerificationToken;
                var mail = user.Email;
                string? URL = Environment.GetEnvironmentVariable("Host")?.ToString();
                if (URL != null && mail != null)
                {
                    var verificationLink = $"{Environment.GetEnvironmentVariable("URL")}/api/User/verify?token={token}";
                    var message = $"Please verify your email by clicking on the link: <a href='{verificationLink}'>Verify Email</a>";
                    await _mailService.SendEmailAsync(mail, "Finance Report Verification Mail", message);
                }
            }
        }
        [HttpGet("IsVerified")]
        public async Task<IActionResult> IsVerified(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest(new { msg = "Invalid token." });
            }
            else
            {
                string status = "User is Not Verified";

                if (user.VerifiedAt != null)
                {
                     status = "User is already Verified";
                }

                return Ok(new { msg = status });
            }

        }
        private async Task SendForgotPasswordMail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user !=null) { 
            var mail = user.Email;
               if (mail != null)
               {
                   var resetToken = user.PasswordResetToken;
                   var message = $"Please use the code below to reset your account password: <br> <b>{resetToken}</b>";
                   await _mailService.SendEmailAsync(mail, "Finance Report Reset Password", message);
               }
           }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest(new { msg = "User not found." });
            }

            user.PasswordResetToken = CreateRandomToken(8);
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            await SendForgotPasswordMail(user.Id);

            return Ok(new { msg = "You may now reset your password." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest(new { msg = "Invalid Token." });
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { msg = "Password successfully reset." });
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken(int tokenLength = 64)
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(tokenLength));
        }

        [HttpGet("AppTest")]
        //[Authorize]
        public async Task<ActionResult> AppTest()
        {
            try
            {
            return Ok("This is a Successful Response!!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in AppTest: {Message}", ex.Message);
                return StatusCode(500); 
            }
        }
    }

}
