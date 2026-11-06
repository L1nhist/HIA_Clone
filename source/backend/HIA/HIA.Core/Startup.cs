using HIA.Core.Base;
using HIA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace HIA.Core;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        #region connect db
        services.AddDbContext<HIADbContext>(options => options
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sql =>
            {
                sql.CommandTimeout(180); // 3 minutes
            })
        );

        services.AddScoped<Func<HIADbContext>>(provider => () => provider.GetRequiredService<HIADbContext>());
        services.AddScoped<DbFactory>();
        #endregion

        #region auto mapper
        var mapper = AutoMapperConfig.Initialize();
        services.AddSingleton(mapper);
        #endregion

        #region Seria Logger
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
        #endregion
    }
}
