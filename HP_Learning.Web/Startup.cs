using Autofac;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using HP_Learning.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.ComponentModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityModel;
using HP_Learning.Web.Middlewares;
using System.Globalization;
using VietGIS.Infrastructure.Modules;
using VietGIS.Infrastructure;
using VietGIS.Infrastructure.Options;
using VietGIS.Infrastructure.Web;
using VietGIS.Infrastructure.Profiles;
using VietGIS.Infrastructure.Identity.DbContexts;
using VietGIS.Infrastructure.Identity.Initialize;

namespace HP_Learning.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            GlobalConfiguration.WebRootPath = _hostingEnvironment.WebRootPath;
            GlobalConfiguration.ContentRootPath = _hostingEnvironment.ContentRootPath;
            GlobalConfiguration.CDNUrl = _configuration.GetValue<string>("Hosts:CDN");
            GlobalConfiguration.ImagePath = _configuration.GetValue<string>("Hosts:ImagePath");
            GlobalConfiguration.DocumentPath = _configuration.GetValue<string>("Hosts:DocumentPath");
            GlobalConfiguration.ImageUploadPath = _configuration.GetValue<string>("Hosts:ImageUploadPath");
            GlobalConfiguration.DocumentUploadPath = _configuration.GetValue<string>("Hosts:DocumentUploadPath");

            services.AddModules();

            services.Configure<DatabaseOptions>(_configuration.GetSection("ConnectionStrings"));
            // services.Configure<CustomSMSOption>(_configuration.GetSection("SMS"));

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddCustomizedDataStore(_configuration);
            services.AddCustomizedIdentity(_configuration);

            services.AddCustomizedMvc(GlobalConfiguration.Modules);
            services.Configure<RazorViewEngineOptions>(
                options => { options.ViewLocationExpanders.Add(new ModuleViewLocationExpander()); });
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            services.AddTransient<IRazorViewRenderer, RazorViewRenderer>();
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-Token");

            foreach (var module in GlobalConfiguration.Modules)
            {
                var moduleInitializerType = module.Assembly.GetTypes()
                   .FirstOrDefault(t => typeof(IModuleInitializer).IsAssignableFrom(t));
                if ((moduleInitializerType != null) && (moduleInitializerType != typeof(IModuleInitializer)))
                {
                    var moduleInitializer = (IModuleInitializer)Activator.CreateInstance(moduleInitializerType);
                    services.AddSingleton(typeof(IModuleInitializer), moduleInitializer);
                    moduleInitializer.ConfigureServices(services);
                }
            }

            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddScoped<IMediator, Mediator>();

            BaseMapperConfig.Configure();

            services.AddAutoMapper(typeof(IdentityMapperProfile));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacRegistrar.Register(builder);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            CultureInfo.DefaultThreadCurrentCulture = GlobalConfiguration.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = GlobalConfiguration.Culture;

            DatabaseInitializer.Initialize(app, dbContext, new List<Client>(), new List<IdentityResource>(), new List<ApiResource>());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseWhen(
                    context => !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
                    a => a.UseExceptionHandler("/Home/Error")
                );
                app.UseHsts();
            }

            // app.UseWhen(
            //     context => !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
            //     a => a.UseStatusCodePagesWithReExecute("/Home/ErrorWithCode/{0}")
            // );

            foreach (var module in GlobalConfiguration.Modules)
            {
                var moduleInitializerType = module.Assembly.GetTypes()
                   .FirstOrDefault(t => typeof(IModuleInitializer).IsAssignableFrom(t));
                if ((moduleInitializerType != null) && (moduleInitializerType != typeof(IModuleInitializer)))
                {
                }
            }

            // app.UseHttpsRedirection();
            app.UseCustomizedStaticFiles(env);
            app.UseRouting();

            app.UseCookiePolicy();

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Map("/_upload/images", b => b.UseMiddleware<ImageUploadMiddleware>());
            app.Map("/_upload/documents", b => b.UseMiddleware<DocumentUploadMiddleware>());
            app.Map("/_images", b => b.UseMiddleware<ImageServeMiddleware>());
            app.Map("/_documents", b => b.UseMiddleware<DocumentServeMiddleware>());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            var moduleInitializers = app.ApplicationServices.GetServices<IModuleInitializer>();
            foreach (var moduleInitializer in moduleInitializers)
            {
                moduleInitializer.Configure(app, env);
            }
        }

        private static string GetHeaderValue(HttpRequest request, string headerName)
        {
            return request.Headers.TryGetValue(headerName, out Microsoft.Extensions.Primitives.StringValues list)
                ? list.FirstOrDefault()
                : null;
        }
    }
}
