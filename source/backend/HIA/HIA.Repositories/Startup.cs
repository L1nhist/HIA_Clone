using HIA.Repositories.Base;
using HIA.Repositories.CheckThe;
using HIA.Repositories.Files;
using HIA.Repositories.HDSD;
using HIA.Repositories.ThongKe;
using HIA.Repositories.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HIA.Repositories;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHDSDThuocRepository, HDSDThuocRepository>();
        services.AddScoped<IHDSDDichVuRepository, HDSDDichVuRepository>();
        services.AddScoped<IThongKeRepository, ThongKeRepository>();
        services.AddScoped<IXml01Repository, Xml01Repository>();
        services.AddScoped<IXMLValidateRepository, XMLValidateRepository>();
        services.AddScoped<ICheckThe130Repository, CheckThe130Repository>();
        services.AddScoped<IChiDinhDongThoiRepository, ChiDinhDongThoiRepository>();
    }
}
