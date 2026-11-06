using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace HIA.ImportExportApp
{
    internal static class Program
    {
        public static IConfiguration? Configuration;
        public class ApplicationLogs
        {
        }
        private static CultureInfo[] SUPPORTED_CULTURES = new[]
        {
            new CultureInfo("vi"),
            new CultureInfo("en")
        };
        private static void ConfigureServices(ServiceCollection services)
        {
            #region configuration
            var builder = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
            services.AddScoped<IConfiguration>(_ => Configuration);
            #endregion

            #region Authenticate
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString(Configuration["Jwt:DeniedPath"]);
                options.LoginPath = new PathString(Configuration["Jwt:LoginPath"]);
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,

                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidateIssuer = true,

                    ValidAudience = Configuration["Jwt:Issuer"],
                    ValidateAudience = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidateIssuerSigningKey = true,
                };
            });
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
            services.AddScoped<Core.Base.TokenHandler>();
            #endregion

            services.Configure<FormOptions>(x =>
            {
                x.ValueCountLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
                options.Limits.MaxRequestHeaderCount = int.MaxValue;
            });

            services.AddControllersWithViews()
                .AddMvcLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddSessionStateTempDataProvider();
            services.AddSession();

            #region mutiple-language
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: SUPPORTED_CULTURES[0].Name, uiCulture: SUPPORTED_CULTURES[0].Name);
                options.SupportedCultures = SUPPORTED_CULTURES;
                options.SupportedUICultures = SUPPORTED_CULTURES;
                options.RequestCultureProviders = new List<IRequestCultureProvider>();
            });
            #endregion

            Core.Startup.ConfigureServices(services, Configuration);
            Repositories.Startup.ConfigureServices(services, Configuration);
            Services.Startup.ConfigureServices(services, Configuration);

            services.AddLogging();
            services.AddScoped<HIA_App>();
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                var mainForm = serviceProvider.GetRequiredService<HIA_App>();
                Application.Run(mainForm);
            }
        }
    }
}
