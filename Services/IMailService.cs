﻿namespace StockMarket.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body);

    }
}
