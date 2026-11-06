using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace HIA.Core
{
    public class Startup
    {
        static public void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
        {
            #region configure strongly typed settings object
            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            #endregion
            #region connect db
            services.AddDbContext<HIADbContext>(options => options
                //.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), options =>
                {
                    options.CommandTimeout(180); // 3 minutes
                })
            );

            services.AddScoped<Func<HIADbContext>>((provider) => () => provider.GetService<HIADbContext>());
            services.AddScoped<DbFactory>();
            #endregion

            #region auto mapper
            var mapper = AutoMapperConfig.Initialize();
            services.AddSingleton(mapper);
            #endregion

            #region Seria Logger
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            #endregion
        }
    }
}
