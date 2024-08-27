using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Stock_Market.Controllers;
using StockMarket.Data;
using StockMarket.Services;
using StockMarket.Services.Email;
using StockMarket.Services.PeriodicTaskHosted;
using StockMarket.Services.Startup;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Microsoft.AspNetCore.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
// Ensure the Logs directory exists
//var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
var logDirectory = "C:/General-Data/API-Server-Logs";
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug() // Set the minimum log level
    .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


// Load environment variables from the .env file
Env.Load();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Register StockMarketController
builder.Services.AddTransient<StockMarketController>();

// Register services
//builder.Services.AddHostedService<UpdateSymbolsDataService>();
//builder.Services.AddHostedService<GeneralPeriodicTaskService>();


builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddSingleton<StartupService>();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//------------------------------------JWT Tokens------------------------------------
builder.Services.AddScoped<AuthService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.SaveToken = true;
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key")??"")),
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_Issuer"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_Audience"),
    };
});
//------------------------------------JWT Tokens------------------------------------

//------------------------------------Configure Quartz.NET ------------------------------------
// Configure Quartz.NET
builder.Services.AddQuartz(q =>
{
    // Define the first job for "UpdateSymbolsData"
    var jobKey = new JobKey("UpdateSymbolsData");
    q.AddJob<ScheduledSymbolsDataService>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ScheduledTaskTrigger-01")
        .WithCronSchedule("0 0 10-15 ? * SUN-THU")); // each hour from 10 AM to 3 PM on SUN-THU
    //.WithCronSchedule("0 0 10-15 * * ?")); // Every hour from 10 AM to 3 PM
    //.WithCronSchedule("0 * 00-23 * * ?")); // Every minute from 00 AM to 11 PM


    // Define the second job for "updateAllSymbolsTechnicalAnalysis"
    var dailyJobKey = new JobKey("updateAllSymbolsTechnicalAnalysis");
    q.AddJob<ScheduledTechnicalAnalysisService>(opts => opts.WithIdentity(dailyJobKey));
    q.AddTrigger(opts => opts
        .ForJob(dailyJobKey)
        .WithIdentity("ScheduledTaskTrigger-02")
        .WithCronSchedule("0 0 19 ? * SAT")); // Runs once weekly at 7:00 AM
    //.WithCronSchedule("0 * 00-23 * * ?")); // Every minute from 00 AM to 23 PM

});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddScoped<ScheduledSymbolsDataService>();
//---------------------------------------------------------------------------------------

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "https://financial-report-umber.vercel.app","https://finrep.net")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

//Custom startup logic
var _startupService = app.Services.GetRequiredService<StartupService>();
_startupService.Initialize();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Server is running!");

app.Run();
