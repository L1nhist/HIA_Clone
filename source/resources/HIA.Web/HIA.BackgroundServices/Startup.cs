using HIA.BackgroundServices.VerifyXML;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HIA.BackgroundServices
{
    public class Startup
    {
        static public void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddCronJob<VerifyXMLCronJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = $"*/{Configuration["AppSettings:BackgroundServices:VerifyXML:MinuteVerifyTime"]} * * * *";
            });

            services.AddVerify<ValidateXML>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = $"*/{Configuration["AppSettings:BackgroundServices:VerifyXML:MinuteVerifyTime"]} * * * *";
            });

            services.AddValidate130<ValidateXML130>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = $"*/{Configuration["AppSettings:BackgroundServices:VerifyXML:MinuteVerifyTime"]} * * * *";
            });
        }
    }
}
