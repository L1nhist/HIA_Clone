using HIA.Services.Auth;
using HIA.Services.Categories;
using HIA.Services.Department;
using HIA.Services.Email;
using HIA.Services.Files;
using HIA.Services.HDSDManagement;
//Linhist removed 29/03/25
//using HIA.Services.HosoManagement;
using HIA.Services.HosoManagement130;
using HIA.Services.HospitalManagement;
using HIA.Services.RoleManagement;
using HIA.Services.SysHistoryLogManagement;
using HIA.Services.UserManagement;
using HIA.Services.Report;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HIA.Services.Verify;

namespace HIA.Services
{
    public class Startup
    {
		static public void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
		{
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<IHospitalService, HospitalService>();
			services.AddScoped<IHDSDThuocService, HDSDThuocService>();
            services.AddScoped<IHDSDDichVuService, HDSDDichVuService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
			services.AddScoped<ISysHistoryLogService, SysHistoryLogService>();
            //Linhist removed 29/03/25
   //         services.AddScoped<IFilesService, FilesService>();
			//services.AddScoped<IHoSoManagementService, HoSoManagementService>();
			services.AddScoped<IReportService, ReportService>();
			services.AddScoped<IDepartmentService, DepartmentService>();
			services.AddScoped(typeof(ICategoryService<, ,>), typeof(CategoryService<, ,>));
			services.AddScoped(typeof(ICategoryByHospitalService<, ,>), typeof(CategoryByHospitalService<, ,>));
			services.AddScoped<IEmailService, EmailService>();
			//Linhist removed 110926
			//services.AddScoped<IDMErrorService, DMErrorService>();
			services.AddScoped<IDMKhoaBVService, DMKhoaBVService>();
            services.AddScoped<IDMErrorService130, DMErrorService130>();
            services.AddScoped<IFilesService130, FilesService130>();
			services.AddScoped<IHoSoManagementService130, HoSoManagementService130>();
			services.AddScoped<IXMLVerifyService, XMLVerifyService>();
			services.AddScoped<IReportHandleService, ReportHandleService>();
            services.AddScoped<IDichVuMaMayService, DichVuMaMayService>();
            services.AddScoped<IChiDinhDongThoiService, ChiDinhDongThoiService >();
        }
	}
}
