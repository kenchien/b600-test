using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Text;
using TldcFare.Dal;
using TldcFare.Dal.DBComponment;
using TldcFare.WebApi.Middleware;

namespace TldcFare.WebApi {
   public class Startup {
      private readonly IConfiguration _configuration;

      public Startup(IConfiguration configuration) {
         _configuration = configuration;

         //Syncfusion需要license,我有取得免費社群license,一開始就要加入下面這行
         Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
             "MzE4NTY4QDMxMzgyZTMyMmUzMFE3OVB5d2ZlNTMrcEtwRmxCTE54U2F4SjJUT2F4RW5SejJlYWliNnFIakE9");

         //.net core為了讀取big5(ansi)的檔案,必須要額外加入System.Text.Encoding.CodePages並在一開始加入下面這行
         Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      }


      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services) {
         services.AddControllersWithViews();

         //string test = HashPwd("123456");//製作密碼專用

         //ken,設定預設cultureInfo
         var cultureInfo = new CultureInfo("en-US");
         CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
         CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

         //setting cors
         services.AddCors(options => {
            options.AddPolicy("CorsPolicy", policy => { policy.AllowAnyOrigin().AllowAnyHeader(); });
         });

         services.Configure<IdentityOptions>(options => {
            // Lockout settings
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  //5分鐘沒有動靜就自動鎖住定網站，預設5分鐘
            options.Lockout.MaxFailedAccessAttempts = 5;                       //5次密碼誤就鎖定網站, 預設5次
            options.Lockout.AllowedForNewUsers = true;                         //新增的使用者也會被鎖定，就是犯規沒有新人優待
         });

         services.ConfigureRepository();

         //string conn = "server=localhost;database=labour;user=labourUser;password=#One0620Punch;treattinyasboolean=true;AllowUserVariables=true";
         //string conn = DecryptAes(_configuration.GetConnectionString("LocalhostConnection"));//localhost
         string conn = _configuration.GetConnectionString("LocalhostConnection");
         try {

            //AddDbContext open connect dispose
            services.AddDbContext<labourContext>(options => { options.UseMySql(conn); });

         } catch (Exception ex) {
            throw new Exception(conn);
         }

         services.AddTransient<IDapper, DapperBase>(_ =>
             new DapperBase(conn));
         services.AddSingleton(_configuration);

         services
             .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options => {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                   ValidateIssuer = true,
                   ValidIssuer = _configuration.GetValue<string>("Jwt:Issuer"),
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   IssuerSigningKey =
                           new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")))
                };
             });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env, labourContext dbContext) {
         if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
         } else {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
         }

         app.UseCors("CorsPolicy");

         //Middleware
         app.UseMiddleware<ExceptionHandleMiddleware>();
         app.UseMiddleware<UserActLogMiddleware>();


         dbContext.Database.EnsureCreated();

         app.UseHttpsRedirection();
         app.UseStaticFiles();

         app.UseRouting();
         app.UseAuthentication();
         app.UseAuthorization();

         app.UseEndpoints(endpoints => {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }

   }
}