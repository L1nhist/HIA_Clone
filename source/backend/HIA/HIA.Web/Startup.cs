using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using HIA.Core.Base;
using HIA.Core.Models.Token;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;

namespace HIA.Web;

public class Startup
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new CultureInfo("vi"),
        new CultureInfo("en")
    ];

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var jwtKey = Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");

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

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuerSigningKey = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var refresh = context.Principal?.FindFirst(ClaimKey.REFRESH)?.Value;
                    if (refresh != ClaimKey.REFRESH_CODE)
                        context.Fail(new Exception("Token not valid anymore"));

                    // Doc email giong TokenHandler.IsTokenValid: NameIdentifier, neu rong thi Email
                    var email = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrWhiteSpace(email))
                        email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                    var excludedEmails = context.HttpContext.RequestServices.GetRequiredService<ExcludedEmailValidator>();
                    if (excludedEmails.IsExcluded(email))
                        context.Fail(new Exception("Token not valid anymore"));

                    return Task.CompletedTask;
                }
            };
        });
        services.AddHttpContextAccessor();
        services.AddScoped<HIA.Core.Base.TokenHandler>();
        #endregion

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            options.Limits.MaxRequestHeaderCount = int.MaxValue;
        });

        services.AddControllersWithViews(options =>
            {
                options.MaxModelBindingCollectionSize = 10000;
            })
            .AddJsonOptions(options =>
            {
                // Cung chinh sach ten voi JsonDefaults (toan chu thuong); khong dung UnsafeRelaxedJsonEscaping cho MVC
                options.JsonSerializerOptions.PropertyNamingPolicy = LowerCaseNamingPolicy.Instance;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            })
            .AddMvcLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
            .AddSessionStateTempDataProvider();
        services.AddSession();

        #region mutiple-language
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(culture: SupportedCultures[0].Name, uiCulture: SupportedCultures[0].Name);
            options.SupportedCultures = SupportedCultures;
            options.SupportedUICultures = SupportedCultures;
            options.RequestCultureProviders = new List<IRequestCultureProvider>();
        });
        #endregion

        services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

        services.AddOpenApi();

        HIA.Core.Startup.ConfigureServices(services, Configuration);
        HIA.Repositories.Startup.ConfigureServices(services, Configuration);
        HIA.Services.Startup.ConfigureServices(services, Configuration);

        //Linhist added 08/06/22
        services.AddAntiforgery(options =>
        {
            options.SuppressXFrameOptionsHeader = true;
        });

        // Note: If you are using a standard HTML form submission, you may also need to
        // increase the FormOptions ValueCountLimit, which defaults to 1024 as well.
        services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = 10000;
            options.MultipartBodyLengthLimit = int.MaxValue;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        #region mutiple-language
        app.UseRequestLocalization(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(culture: SupportedCultures[0].Name, uiCulture: SupportedCultures[0].Name);
            options.SupportedCultures = new List<CultureInfo> { SupportedCultures[0] };
            options.SupportedUICultures = new List<CultureInfo> { SupportedCultures[0] };
            options.RequestCultureProviders = new List<IRequestCultureProvider>();
        });
        #endregion

        #region frame-ancestors (CSP)
        var frameAncestors = BuildFrameAncestorsPolicy(
            Configuration.GetSection("AppSettings:AllowedFrameAncestors").Get<string[]>(),
            app.ApplicationServices.GetRequiredService<ILogger<Startup>>());
        app.Use(async (context, next) =>
        {
            // Khong ghi de CSP da duoc dat truoc do
            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
                context.Response.Headers.Append("Content-Security-Policy", frameAncestors);
            await next();
        });
        #endregion

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSession();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapOpenApi();
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}",
                defaults: new { controller = "Home", action = "Index" });
        });
    }

    // Chi nhan origin dang http(s)://host[:port]; muc khac bi bo qua de khong chen duoc ky tu vao header.
    private static readonly Regex FrameAncestorOrigin = new(@"^https?://[A-Za-z0-9.-]+(:[0-9]{1,5})?\z", RegexOptions.Compiled);

    private static string BuildFrameAncestorsPolicy(string[]? configured, ILogger<Startup> logger)
    {
        var sources = new List<string> { "'self'" };
        foreach (var item in configured ?? [])
        {
            var origin = item?.Trim() ?? string.Empty;
            if (FrameAncestorOrigin.IsMatch(origin))
                sources.Add(origin);
            else
                logger.LogWarning("AppSettings:AllowedFrameAncestors bo qua muc khong hop le: {Item}", item);
        }
        return "frame-ancestors " + string.Join(" ", sources);
    }
}
