using HIA.Repositories.Base;
using HIA.Repositories.CheckThe;
//Linhist removed 29/03/25
//using HIA.Repositories.FileAPI;
using HIA.Repositories.Files;
using HIA.Repositories.HDSD;
using HIA.Repositories.ThongKe;
using HIA.Repositories.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HIA.Repositories
{
    public class Startup
    {
		static public void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
		{
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            //Linhist removed 29/03/25
            //services.AddScoped<IXml1ErrorRepository, Xml1ErrorRepository>();
            //services.AddScoped<IXml1Repository, Xml1Repository>();
            //services.AddScoped<IXml1APIRepository, Xml1APIRepository>();
            //services.AddScoped<IXml1Error_APIRepository, Xml1Error_APIRepository>();
            //services.AddScoped<IXml1Repository_Temp, Xml1Repository_Temp>();
            services.AddScoped<IHDSDThuocRepository, HDSDThuocRepository>();
            services.AddScoped<IHDSDDichVuRepository, HDSDDichVuRepository>();
            services.AddScoped<IThongKeRepository, ThongKeRepository>();
            services.AddScoped<IXml01Repository, Xml01Repository>();
            services.AddScoped<IXMLValidateRepository, XMLValidateRepository>();
            //services.AddScoped<IXMLVerifyRepository, XMLVerifyRepository>();
            services.AddScoped<ICheckThe130Repository, CheckThe130Repository>();
            services.AddScoped<IChiDinhDongThoiRepository, ChiDinhDongThoiRepository>();
        }
	}
}
