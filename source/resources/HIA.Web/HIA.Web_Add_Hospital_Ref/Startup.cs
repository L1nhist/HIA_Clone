using HIA.Core.Base;
using HIA.Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HIA.Web
{
    public class Startup
    {
        private CultureInfo[] SUPPORTED_CULTURES = new[]
        {
            new CultureInfo("vi"),
            new CultureInfo("en")
        };
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HIA.WebApi", Version = "v1" });
            });

            Core.Startup.ConfigureServices(services, Configuration);
            Repositories.Startup.ConfigureServices(services, Configuration);
            Services.Startup.ConfigureServices(services, Configuration);
            BackgroundServices.Startup.ConfigureServices(services, Configuration);

            //Linhist added 08/06/22
            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler($"/{Uti.GetControllerName(nameof(HomeController))}/{nameof(HomeController.Error)}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            #region mutiple-language
            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: SUPPORTED_CULTURES[0].Name, uiCulture: SUPPORTED_CULTURES[0].Name);
                options.SupportedCultures = new List<CultureInfo> { SUPPORTED_CULTURES[0] };
                options.SupportedUICultures = new List<CultureInfo> { SUPPORTED_CULTURES[0] };
                options.RequestCultureProviders = new List<IRequestCultureProvider>();
            });
            #endregion

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            //app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HIA.WebApi v1"));
            //app.UseSwagger();
            //app.UseSwaggerUI();


            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
