using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;

namespace TldcFare.WebApi {
   public class Program {
      private readonly static ILog _log = LogManager.GetLogger(typeof(Program));

      public static void Main(string[] args) {

         var repository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
         XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

         _log.Info("Application Start(kentest)");
         CreateHostBuilder(args).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((WebHostBuilder, ConfigurationBinder) => {
                 ConfigurationBinder.AddJsonFile("settings.json", optional: true);
              })
              .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

   }
}