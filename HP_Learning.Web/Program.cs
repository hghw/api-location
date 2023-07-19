using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using Serilog;
using Microsoft.Extensions.Logging;
using System;

namespace HP_Learning.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // IConfiguration config = new ConfigurationBuilder()
            //     .SetBasePath(Directory.GetCurrentDirectory())
            //     .AddJsonFile("appsettings.json", false, false)
            //     .AddCommandLine(args)
            //     .Build();
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(SetupWebHost)
                .ConfigureAppConfiguration(SetupConfiguration)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog((ctx, c) => { c.ReadFrom.Configuration(ctx.Configuration); });
        }

        private static void SetupWebHost(IWebHostBuilder webBuilder)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", false, false)
                // .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                // .AddCommandLine(args)
                .Build();

            webBuilder.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = long.MaxValue;
            });
            webBuilder.UseIISIntegration();
            webBuilder.UseStartup<Startup>();
            webBuilder.UseUrls(configurationRoot["Hosts:Urls"]);
        }

        private static void SetupConfiguration(HostBuilderContext hostingContext, IConfigurationBuilder configBuilder)
        {
            var env = hostingContext.HostingEnvironment;
            var configuration = configBuilder.Build();


        }

        private static void SetupLogging(WebHostBuilderContext hostingContext, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddSerilog();
        }
    }
}
