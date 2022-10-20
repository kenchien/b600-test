using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TldcFare.Dal;
using TldcFare.WebApi.IService;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi
{
    public static class ServiceExtensions
    {
        public static void ConfigureRepository(this IServiceCollection services)
        {
            //註冊 Repository (EF,很難用)
            services.AddScoped<IRepository<Achrecord>, Repository<Achrecord>>();
            services.AddScoped<IRepository<Announces>, Repository<Announces>>();
            services.AddScoped<IRepository<Bankinfo>, Repository<Bankinfo>>();
            services.AddScoped<IRepository<Branch>, Repository<Branch>>();
            services.AddScoped<IRepository<Codetable>, Repository<Codetable>>();
            services.AddScoped<IRepository<Execrecord>, Repository<Execrecord>>();
            services.AddScoped<IRepository<Execsmallrecord>, Repository<Execsmallrecord>>();
            services.AddScoped<IRepository<Faredetail>, Repository<Faredetail>>();
            services.AddScoped<IRepository<Farerpt>, Repository<Farerpt>>();
            services.AddScoped<IRepository<Faresource>, Repository<Faresource>>();
            services.AddScoped<IRepository<Funcauthdetail>, Repository<Funcauthdetail>>();
            services.AddScoped<IRepository<Functable>, Repository<Functable>>();
            services.AddScoped<IRepository<Iplock>, Repository<Iplock>>();

            services.AddScoped<IRepository<Logofchange>, Repository<Logofchange>>();
            services.AddScoped<IRepository<Logofexception>, Repository<Logofexception>>();
            services.AddScoped<IRepository<Logofpromote>, Repository<Logofpromote>>();
            
            services.AddScoped<IRepository<Mem>, Repository<Mem>>();
            services.AddScoped<IRepository<Memdetail>, Repository<Memdetail>>();
            services.AddScoped<IRepository<Memripfund>, Repository<Memripfund>>();
            services.AddScoped<IRepository<Oper>, Repository<Oper>>();
            services.AddScoped<IRepository<Opergrprule>, Repository<Opergrprule>>();
            services.AddScoped<IRepository<Orglist>, Repository<Orglist>>();
            services.AddScoped<IRepository<Payrecord>, Repository<Payrecord>>();
            services.AddScoped<IRepository<Payslip>, Repository<Payslip>>();

            services.AddScoped<IRepository<Settingfarefund>, Repository<Settingfarefund>>();
            services.AddScoped<IRepository<Settingfaretype>, Repository<Settingfaretype>>();
            services.AddScoped<IRepository<Settinggroup>, Repository<Settinggroup>>();
            services.AddScoped<IRepository<Settingmonthlyfee>, Repository<Settingmonthlyfee>>();
            services.AddScoped<IRepository<Settingpromote>, Repository<Settingpromote>>();
            services.AddScoped<IRepository<Settingripfund>, Repository<Settingripfund>>();
            services.AddScoped<IRepository<Settingtldc>, Repository<Settingtldc>>();

            services.AddScoped<IRepository<Sev>, Repository<Sev>>();
            services.AddScoped<IRepository<Sevdetail>, Repository<Sevdetail>>();
            services.AddScoped<IRepository<Sevtranrecord>, Repository<Sevtranrecord>>();
            services.AddScoped<IRepository<Zipcode>, Repository<Zipcode>>();

            services.AddScoped<IExceptionLogService, ExceptionLogService>();

            //註冊 Service (可以用interface,也可以不用,這邊邏輯層架構沒設計好,先把interface拔掉)
            //services.AddScoped<IMemberService, MemberService>();
            //services.AddScoped<ISevService, SevService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IOperService, OperService>();
            services.AddScoped<ICommonService, CommonService>();
            
            services.AddScoped<MemberService>();
            services.AddScoped<SevService>();

            services.AddScoped<SystemService>();
            services.AddScoped<AdminService>();
            services.AddScoped<OperService>();
            services.AddScoped<CommonService>();
            
            //後面設計就完全不用interface
            services.AddScoped<PayService>();
            services.AddScoped<ReportService>();

            services.AddScoped<JwtHelper>();


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}